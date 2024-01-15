// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Diagnostics;
using System.Text;
using NLog;
using NLog.Layouts;
using NLog.Targets;
using Spectre.Console;

namespace Utopia.Core.Logging;

/// <summary>
/// 日志管理器
/// </summary>
public static class LogManager
{

    /// <summary>
    /// log option
    /// </summary>
    public class LogOption
    {
        public bool EnableConsoleOutput { get; set; } = true;

        /// <summary>
        /// This option is only for Console Output.
        /// </summary>
        public bool ColorfulOutput { get; set; } = true;

        /// <summary>
        /// Enable trace level output.
        /// This option is only for Console Output.
        /// </summary>
        public bool EnableConsoleTraceOutput { get; set; } = true;

        public LogOption() { }

        /// <summary>
        /// Create a new default log option. e.g. enable colorful output.
        /// </summary>
        /// <returns></returns>
        public static LogOption CreateDefault() => new()
        {
            ColorfulOutput = true,
            EnableConsoleTraceOutput = true,
        };

        /// <summary>
        /// Create a new log option for batch. e.g. disable colorful output.
        /// </summary>
        /// <returns></returns>
        public static LogOption CreateBatch() => new()
        {
            ColorfulOutput = false,
            EnableConsoleTraceOutput = true,
        };
    }

    private class StandardConsoleTarget : Target
    {
        /// <summary>
        /// No readonly
        /// </summary>
        private SpinLock _spin = new();

        protected override void Write(LogEventInfo logEvent)
        {
            bool taken = false;
            try
            {
                _spin.Enter(ref taken);

                var logMessage = Markup.Escape(logEvent.FormattedMessage);

                Markup level;

                if(logEvent.Level == LogLevel.Trace)
                {
                    level = new("[gray]Trace[/]");
                }
                else if(logEvent.Level == LogLevel.Debug)
                {
                    level = new("[blue underline]Debug[/]");
                }
                else if(logEvent.Level == LogLevel.Info)
                {
                    level = new("Info");
                }
                else if (logEvent.Level == LogLevel.Warn)
                {
                    level = new("[yellow]Warn[/]");
                }
                else if (logEvent.Level == LogLevel.Error)
                {
                    level = new("[red]Error[/]");
                }
                else // Fatal
                {
                    level = new("[red bold underline]Fatal[/]");
                }

                var datetime = new Markup($"[blue]{logEvent.TimeStamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss:ffff")}[/]");

                var threadName = new Markup($"{Thread.CurrentThread.Name}");
                var loggerName = new Markup($"{logEvent.LoggerName}");

                AnsiConsole.Write('[');
                AnsiConsole.Write(datetime);
                AnsiConsole.Write(']');
                AnsiConsole.Write('[');
                AnsiConsole.Write(level);
                AnsiConsole.Write(']');
                AnsiConsole.Write('[');
                AnsiConsole.Write(threadName);
                AnsiConsole.Write('-');
                AnsiConsole.Write(loggerName);
                AnsiConsole.Write(']');
                AnsiConsole.Write(':');
                AnsiConsole.Write(logMessage);
                AnsiConsole.WriteLine();

                if (logEvent.Exception != null)
                {
                    logEvent.Exception.PrintColoredStringDemystified();
                    AnsiConsole.WriteLine();
                }
            }
            finally
            {
                if (taken)
                {
                    _spin.Exit();
                }
            }            
        }
    }

    private static Target SetupFileTarget(LogOption option)
    {
        var logfile = new FileTarget("logfile")
        {
            FileName = "Log/Current.log",
            LineEnding = LineEndingMode.LF,
            Encoding = Encoding.UTF8,
            ArchiveFileName = "Log/Archived.{###}.log",
            ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
            ArchiveDateFormat = "yyyy.MM.dd",
            MaxArchiveFiles = 128,
            ArchiveOldFileOnStartup = true,
            EnableArchiveFileCompression = true,
            ArchiveEvery = FileArchivePeriod.Day
        };

        logfile.Layout = new JsonLayout()
        {
            Attributes =
            {
                new JsonAttribute("time", "${longdate}"),
                new JsonAttribute("level", "${level}"),
                new JsonAttribute("thread","${threadname}"),
                new JsonAttribute("logger","${logger}"),
                new JsonAttribute("raw-message","${message:raw=true}"),
                new JsonAttribute("message", "${message}"),
                new JsonAttribute("properties", new JsonLayout { IncludeEventProperties = true, MaxRecursionLimit = 8 }, encode: false),
                new JsonAttribute("exception", new JsonLayout
                {
                    Attributes =
                    {
                        new JsonAttribute("callsite","${callsite}"),
                        new JsonAttribute("type", "${exception:format=type}"),
                        new JsonAttribute("message", "${exception:format=message}"),
                        new JsonAttribute("stacktrace", "${exception:format=tostring}"),
                    }
                },
                encode: false) // don't escape layout
            },
            IndentJson = true,
        };

        return logfile;
    }

    private static Target? SetupConsoleTarget(LogOption option)
    {
        if (!option.EnableConsoleOutput)
        {
            return null;
        }

        Target logconsole = null!;

        // detect color option
        if (option.ColorfulOutput)
        {
            logconsole = new StandardConsoleTarget()
            {
                Name = nameof(logconsole)
            };
        }
        else
        {
            var console = new ConsoleTarget(nameof(logconsole))
            {
                Encoding = Encoding.UTF8,
                Layout = @"[${longdate}][${level}][${threadname}::${logger}]:${message}${onexception:inner=${newline}${exception}}"
            };
            logconsole = console;
        }

        return logconsole;
    }

    /// <summary>
    /// 初始化日志
    /// </summary>
    /// <param name="enableRegexColored">是否启用正则表达式进行着色，这会导致性能降低，但是输出将会变得beautiful</param>
    public static void Init(LogOption option)
    {
        var config = new NLog.Config.LoggingConfiguration();

        // set up
        var console = SetupConsoleTarget(option);

        if (console is not null && option.EnableConsoleTraceOutput)
        {
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, SetupConsoleTarget(option));
        }
        else if(console is not null)
        {
            config.AddRule(LogLevel.Info, LogLevel.Fatal, SetupConsoleTarget(option));
        }

        config.AddRule(LogLevel.Debug, LogLevel.Fatal, SetupFileTarget(option));

        NLog.LogManager.Configuration = config;
        NLog.LogManager.ReconfigExistingLoggers();
        NLog.LogManager.Flush();
    }

    /// <summary>
    /// 关闭日志
    /// </summary>
    public static void Shutdown() => NLog.LogManager.Shutdown();

}
