#region

using System.Xml;
using System.Xml.Serialization;
using Zio;

#endregion

namespace Utopia.Core.Translation;

public static class TranslationLoader
{
    /// <summary>
    ///     Load a .xml translation file,its type as <see cref="TranslationItems" />
    /// </summary>
    public static Dictionary<string, string> LoadFromFile(string file, IFileSystem fileSystem)
    {
        XmlSerializer serializer = new(typeof(TranslationItems));

        TranslationItems? declares = null;

        using (var fs = fileSystem.OpenFile(file, FileMode.Open, FileAccess.Read))
        {
            declares = (TranslationItems?)serializer.Deserialize(fs)
                       ?? throw new XmlException("XmlSerializer.Deserialize return null");
        }

        Dictionary<string, string> items = [];

        foreach (var item in declares.Translations) items.Add(item.Text, item.Translated);

        return items;
    }

    /// <summary>
    ///     Load all .xml files and union them into one.
    ///     It use <see cref="LoadFromFile(string)" /> to read from file.
    /// </summary>
    public static Dictionary<string, string> LoadFromDirectory(string directory, IFileSystem fileSystem)
    {
        Dictionary<string, string> items = [];
        foreach (var files in fileSystem.EnumerateDirectoryEntries(new UPath(directory).ToAbsolute(), "*", SearchOption.AllDirectories))
            if (files.Name.EndsWith(".xml", StringComparison.Ordinal))
                items.Union(LoadFromFile(files.ToString(), fileSystem));

        return items;
    }
}
