#region

using System.Text;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NLog;
using Utopia.Core;
using Utopia.Core.Translation;
using ArgumentSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax;
using IdentifierNameSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax;
using InvocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax;
using LiteralExpressionSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax;
using MemberAccessExpressionSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax;

#endregion

namespace Utopia.Tool;

/// <summary>
///     翻译寻找器
/// </summary>
public static class TranslationFinder
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static async Task<ExtractedItem[]> _WalkDocument(Document file, Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(compilation);

        List<ExtractedItem> items = new();
        var tree = await file.GetSyntaxTreeAsync();

        if (tree == null)
        {
            Logger.Info("skip file {file} because there is no syntax tree", file.FilePath);
            return Array.Empty<ExtractedItem>();
        }

        var root = tree.GetCompilationUnitRoot();

        var model = compilation.GetSemanticModel(tree);

        var nodes = root
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(
                item =>
                {
                    // only access getter.I18n() an so on
                    if (item.Expression is MemberAccessExpressionSyntax syntax)
                    {
                        var type = model.GetTypeInfo(syntax.Expression);

                        if (type.Type == null) return false;

                        var trueType = $"{type.Type?.ContainingNamespace?.Name}.{type.Type?.Name}";

                        foreach (var caller in SyntaxInformation.CallerClassName)
                        {
                            var callerName = caller ?? string.Empty;
                            if (callerName.IndexOf(trueType) != -1) return true;
                        }
                    }

                    // warn
                    if (item.Expression is IdentifierNameSyntax identifierName)
                    {
                        var text = identifierName.Identifier.ValueText;

                        if (SyntaxInformation.CallerFormatMethod.Contains(text)
                            || SyntaxInformation.CallerMethod.Contains(text))
                            Logger.Warn(
                                "The IdentifierNameSyntax calls for I18n/I18nf method are not accepted. Using xxx.I18n*() instead. at {document} {span}",
                                file.FilePath, item.FullSpan.ToString());
                    }

                    return false;
                });

        foreach (var node in nodes)
        {
            bool CheckArgument(IReadOnlyList<ArgumentSyntax> args, int requireArguments, params Type?[] argumentTypes)
            {
                if (args.Count != requireArguments)
                {
                    Logger.Error("Need {require} arguments but get {actual} at {file} {span}",
                        requireArguments,
                        args.Count,
                        file.FilePath,
                        args.First().Span
                    );
                    return false;
                }

                for (var index = 0; index != argumentTypes.Length; index++)
                {
                    var arg = args[index];
                    var type = argumentTypes[index];

                    if (type == null) continue;

                    if (!arg.ChildNodes().Any(node => node.GetType().Equals(type)))
                    {
                        Logger.Error("Need {type} syntax at argument but get none at {file} {span}",
                            type,
                            file.FilePath,
                            arg.Span
                        );
                        return false;
                    }
                }

                return true;
            }

            // get access method
            var args = node.ArgumentList.Arguments;
            var access = (MemberAccessExpressionSyntax)node.Expression;

            var method = access.Name.Identifier.ValueText;

            string text;
            string comment;
            var formatted = false;

            if (SyntaxInformation.CallerMethod.Contains(method))
            {
                // get arguments
                if (!CheckArgument(args, 2, typeof(LiteralExpressionSyntax), typeof(LiteralExpressionSyntax))) continue;
                // remove " in the beginning and the end
                text = ((LiteralExpressionSyntax)args[0].Expression).GetText(Encoding.UTF8).ToString()[1..^1];
                comment = ((LiteralExpressionSyntax)args[1].Expression).GetText(Encoding.UTF8).ToString()[1..^1];
            }
            else if (SyntaxInformation.CallerFormatMethod.Contains(method))
            {
                if (!CheckArgument(args, 3, typeof(LiteralExpressionSyntax), null,
                        typeof(LiteralExpressionSyntax))) continue;
                formatted = true;
                // remove " in the beginning and the end
                text = ((LiteralExpressionSyntax)args[0].Expression).GetText(Encoding.UTF8).ToString()[1..^1];
                comment = ((LiteralExpressionSyntax)args[2].Expression).GetText(Encoding.UTF8).ToString()[1..^1];
            }
            else
            {
                Logger.Warn(
                    "Unknown ITranslationGetter call at {file} {span}",
                    file.FilePath, node.FullSpan.ToString());
                continue;
            }

            items.Add(new ExtractedItem
            {
                Text = text,
                Comment = comment,
                WithFormat = formatted,
                Place = node.FullSpan,
                Document = file
            });
        }

        return items.ToArray();
    }

    public sealed class ExtractedItem
    {
        public string Text { get; set; } = string.Empty;

        public string Comment { get; set; } = string.Empty;

        public TextSpan Place { get; set; }

        public required Document Document { get; set; }

        public bool WithFormat { get; set; }

        public static string WriteToXml(ExtractedItem[] items)
        {
            XmlWriterSettings settings = new()
            {
                Encoding = Encoding.UTF8
            };
            settings.Async = false;
            using var stream = new MemoryStream();

            // format see class TranslationDeclare
            using (var writer = XmlWriter.Create(stream, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement(nameof(TranslationItems), Xml.Namespace);

                {
                    foreach (var item in items)
                    {
                        writer.WriteStartElement(TranslationItems.TranslationItemElementName);

                        var path =
                            Path.GetRelativePath(Path.GetDirectoryName(item.Document.Project.FilePath)!,
                                item.Document.FilePath!);

                        writer.WriteComment($"At project {item.Document.Project.Name}:{path}");
                        if (item.WithFormat) writer.WriteComment("Text will be formatted");
                        writer.WriteElementString(nameof(item.Text), item.Text);
                        writer.WriteElementString(nameof(item.Comment), item.Comment);
                        writer.WriteEndElement();
                    }
                }

                writer.WriteFullEndElement();
                writer.WriteEndDocument();
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }

    public static class SyntaxInformation
    {
        public static readonly HashSet<string?> CallerClassName =
        [
            typeof(ITranslationGetter).FullName!,
            typeof(TranslationGetter).FullName!
        ];

        public static readonly HashSet<string?> CallerMethod =
        [
            nameof(ITranslationGetter.I18n)
        ];

        public static readonly HashSet<string?> CallerFormatMethod =
        [
            nameof(ITranslationGetter.I18nf)
        ];
    }

    /*
    public static void Command(CommandLineApplication configCmd)
    {
        configCmd.Description = "this subcommand will generate translation key from CSharp projects";

        var projects = configCmd.Argument
        ("Projects", "the project file path(.csproj or sln) that you want to get translation item from");
        CommandOption<string> optOpt = configCmd.Option<string>("-o|--output", "the output file", CommandOptionType.SingleValue);

        projects.MultipleValues = true;
        string defaultOpt = "extracted-translations.xml";
        optOpt.DefaultValue = defaultOpt;

        configCmd.OnExecute(() =>
        {
            // load MSBuild
            MSBuildLocator.RegisterDefaults();

            // Load projects
            string? opt = optOpt.Value();

            List<Project> loadedProjects = new();

            foreach (var project in projects.Values)
            {
                if (project is null)
                {
                    return;
                }

                if (project.EndsWith(".sln"))
                {
                    loadedProjects.AddRange(Utility.OpenSlnToProject(project));
                }
                else
                {
                    loadedProjects.Add(Utility.OpenProject(project));
                }
            }

            // compile && process
            ConcurrentBag<ExtractedItem> collected = [];
            List<Task> tasks = [];
            var compilations = Utility.GetCompilation(loadedProjects.ToArray());

            for (int index = 0; index != loadedProjects.Count; index++)
            {
                var project = loadedProjects[index];
                var compilation = compilations[index];

                foreach (var document in project.Documents)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            Logger.Info("process {file}", document.FilePath);
                            var items = await _WalkDocument(document, compilation);
                            foreach (var item in items)
                            {
                                collected.Add(item);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e, "Error when process {project} {file}", project.FilePath, document.FilePath);
                        }
                    }));
                }
            }

            Task.WhenAll(tasks).Wait();

            // Notice
            Logger.Info("Found {itemCount} translations", collected.Count);

            // write to
            var xml = ExtractedItem.WriteToXml(collected.ToArray());
            File.WriteAllText(opt!, xml);

            Logger.Info("Write to {file}", opt!);
        });

    }
    */
}