#region

using System.Xml;
using System.Xml.Serialization;

#endregion

namespace Utopia.Core.Translation;

public static class TranslationLoader
{
    /// <summary>
    ///     Load a .xml translation file,its type as <see cref="TranslationItems" />
    /// </summary>
    public static Dictionary<string, string> LoadFromFile(string file)
    {
        XmlSerializer serializer = new(typeof(TranslationItems));

        TranslationItems? declares = null;

        using (var fs = File.Open(file, FileMode.Open, FileAccess.Read))
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
    public static Dictionary<string, string> LoadFromDirectory(string directory)
    {
        Dictionary<string, string> items = [];
        foreach (var files in Directory.GetFiles(Path.GetFullPath(directory), "*", SearchOption.AllDirectories))
            if (files.EndsWith(".xml", StringComparison.Ordinal))
                items.Union(LoadFromFile(files.ToString()));

        return items;
    }
}
