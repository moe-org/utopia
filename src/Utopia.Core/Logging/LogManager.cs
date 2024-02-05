#region

using System.Text;
using Mingmoe.Demystifier;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

#endregion

namespace Utopia.Core.Logging;

/// <summary>
///     日志管理器
/// </summary>
public static class LogManager
{
    private static void SetupFileTarget(LogOption option, LoggingConfiguration configuration)
    {
        if (!option.EnableLogFile) return;

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

        logfile.Layout = new JsonLayout
        {
            Attributes =
            {
                new JsonAttribute("time", "${longdate}"),
                new JsonAttribute("level", "${level}"),
                new JsonAttribute("thread", "${threadname}"),
                new JsonAttribute("logger", "${logger}"),
                new JsonAttribute("raw-message", "${message:raw=true}"),
                new JsonAttribute("message", "${message}"),
                new JsonAttribute("properties", new JsonLayout { IncludeEventProperties = true, MaxRecursionLimit = 8 },
                    false),
                new JsonAttribute("exception", new JsonLayout
                    {
                        Attributes =
                        {
                            new JsonAttribute("callsite", "${callsite}"),
                            new JsonAttribute("type", "${exception:format=type}"),
                            new JsonAttribute("message", "${exception:format=message}"),
                            new JsonAttribute("stacktrace", "${exception:format=tostring}")
                        }
                    },
                    false) // don't escape layout
            },
            IndentJson = true
        };

        configuration.AddRule(LogLevel.Trace, LogLevel.Off, logfile);
    }

    private static void SetupConsoleTarget(LogOption option, LoggingConfiguration configuration)
    {
        if (!option.EnableConsoleOutput) return;

        // detect output level
        var min = option.EnableConsoleTraceOutput ? LogLevel.Trace : LogLevel.Debug;

        // detect color option
        if (option.ColorfulOutput)
        {
            configuration.AddRule(
                min,
                LogLevel.Off,
                new StandardConsoleTarget
                {
                    Name = "Colored Console Log Target",
                    WriteLineAction = option.WriteLineAction
                });
            return;
        }

        configuration.AddRule(
            min,
            LogLevel.Off,
            new ConsoleTarget("Console Log Target")
            {
                Encoding = Encoding.UTF8,
                Layout =
                    @"[${longdate}][${level}][${threadname}::${logger}]:${message}${onexception:inner=${newline}${exception}}"
            });
    }

    /// <summary>
    ///     初始化日志
    /// </summary>
    /// <param name="enableRegexColored">是否启用正则表达式进行着色，这会导致性能降低，但是输出将会变得beautiful</param>
    public static void Init(LogOption option)
    {
        var config = new LoggingConfiguration();

        // set up
        SetupConsoleTarget(option, config);
        SetupFileTarget(option, config);

        NLog.LogManager.Configuration = config;
        NLog.LogManager.ReconfigExistingLoggers();
        NLog.LogManager.Flush();
    }

    /// <summary>
    ///     关闭日志
    /// </summary>
    public static void Shutdown()
    {
        NLog.LogManager.Shutdown();
    }

    /// <summary>
    ///     log option
    /// </summary>
    public class LogOption
    {
        /// <summary>
        ///     We will call this method if it is not null with the string that will be output to console.
        ///     Available only when both <see cref="EnableConsoleOutput" /> and
        ///     <see cref="ColorfulOutput" /> are true.
        /// </summary>
        public Action<string>? WriteLineAction { get; set; } = null;

        public bool EnableConsoleOutput { get; set; } = true;

        /// <summary>
        ///     This option is only for Console Output.
        /// </summary>
        public bool ColorfulOutput { get; set; } = true;

        /// <summary>
        ///     Enable trace level output.
        ///     This option is only for Console Output.
        ///     If false, it will log from Debug level.
        /// </summary>
        public bool EnableConsoleTraceOutput { get; set; } = true;

        /// <summary>
        ///     If ture,output log to file
        /// </summary>
        public bool EnableLogFile { get; set; } = true;

        /// <summary>
        ///     Create a new default log option. e.g. enable colorful output.
        /// </summary>
        /// <returns></returns>
        public static LogOption CreateDefault()
        {
            return new()
            {
                ColorfulOutput = true,
                EnableConsoleTraceOutput = true
            };
        }

        /// <summary>
        ///     Create a new log option for batch. e.g. disable colorful output.
        /// </summary>
        /// <returns></returns>
        public static LogOption CreateBatch()
        {
            return new()
            {
                ColorfulOutput = false,
                EnableConsoleTraceOutput = true
            };
        }
    }

    /// <summary>
    ///     A class for colored console output
    /// </summary>
    private class StandardConsoleTarget : Target
    {
        /// <summary>
        ///     No readonly
        /// </summary>
        private SpinLock _spin;

        public Action<string>? WriteLineAction { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            var taken = false;
            try
            {
                this._spin.Enter(ref taken);

                StyledBuilder builder = new();

                builder.Append("[");
                builder.Append(new Style { ForeColor = Color.Cornflowerblue },
                    logEvent.TimeStamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss:ffff"));
                builder.Append("]");
                builder.Append("[");

                if (logEvent.Level == LogLevel.Trace)
                    builder.Append(new Style { ForeColor = Color.Gray }, "Trace");
                else if (logEvent.Level == LogLevel.Debug)
                    builder.Append(new Style { ForeColor = Color.Blue, isUnderline = true }, "Debug");
                else if (logEvent.Level == LogLevel.Info)
                    builder.Append(new Style(), "Info");
                else if (logEvent.Level == LogLevel.Warn)
                    builder.Append(new Style { ForeColor = Color.Yellow, isUnderline = true }, "Warn");
                else if (logEvent.Level == LogLevel.Error)
                    builder.Append(new Style { ForeColor = Color.Red, isUnderline = true }, "Error");
                else // Fatal
                    builder.Append(
                        new Style { ForeColor = Color.Red, isUnderline = true, isItalic = true, isBold = true },
                        "Fatal");

                builder.Append("]");
                builder.Append("[");
                builder.Append(Thread.CurrentThread.Name ?? "<Null Thread Name>");
                builder.Append("]");
                builder.Append("[");
                builder.Append(logEvent.LoggerName ?? "<Null Logger Name>");
                builder.Append("]");
                builder.Append(":");
                builder.Append(logEvent.FormattedMessage);

                if (logEvent.Exception != null)
                {
                    // logEvent.Exception();
                    builder.AppendLine();
                    builder.AppendColoredDemystified(logEvent.Exception, StyledBuilderOption.GlobalOption);
                }

                this.WriteLineAction?.Invoke(builder.ToString());
                Console.Out.WriteLine(builder.ToString());
            }
            finally
            {
                if (taken) this._spin.Exit();
            }
        }
    }
}