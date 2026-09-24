#if DEBUG
using System.IO;
using System.Reflection;
using System.Text;

namespace PvpAutoLb.Core.Localization;

internal static class LocAudit
{
    private const int ReportedKeyLimit = 20;

    public static void Run(string directory)
    {
        var keys = CollectKeys();
        for (var index = 0; index < Languages.All.Length; index++)
        {
            var language = Languages.All[index];
            if (ReferenceEquals(language, Languages.English))
            {
                continue;
            }

            var catalog = StringCatalog.Load(Path.Combine(directory, string.Concat(language.Code, ".json")));
            if (catalog.Count == 0)
            {
                RunLog.Warning($"'{language.Code}.json' missing or empty.");
                continue;
            }

            var missing = new StringBuilder();
            var missingCount = 0;
            for (var keyIndex = 0; keyIndex < keys.Count; keyIndex++)
            {
                if (catalog.Contains(keys[keyIndex]))
                {
                    continue;
                }

                missingCount++;
                if (missingCount <= ReportedKeyLimit)
                {
                    missing.Append(keys[keyIndex]).Append(", ");
                }
            }

            if (missingCount == 0)
            {
                RunLog.Info($"'{language.Code}.json' complete ({keys.Count} keys).");
                continue;
            }

            RunLog.Warning($"'{language.Code}.json' missing {missingCount}/{keys.Count} keys: {missing}…");
        }
    }

    private static List<string> CollectKeys()
    {
        var keys = new List<string>();
        var groups = typeof(L).GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
        for (var groupIndex = 0; groupIndex < groups.Length; groupIndex++)
        {
            var fields = groups[groupIndex].GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            for (var fieldIndex = 0; fieldIndex < fields.Length; fieldIndex++)
            {
                var field = fields[fieldIndex];
                if (field.FieldType == typeof(LocString))
                {
                    keys.Add(((LocString)field.GetValue(null)!).Key);
                }
                else if (field.FieldType == typeof(LocPlural))
                {
                    var keyBase = ((LocPlural)field.GetValue(null)!).KeyBase;
                    keys.Add(string.Concat(keyBase, ".one"));
                    keys.Add(string.Concat(keyBase, ".other"));
                }
                else if (field.FieldType == typeof(LocString[]))
                {
                    var entries = (LocString[])field.GetValue(null)!;
                    for (var entryIndex = 0; entryIndex < entries.Length; entryIndex++)
                    {
                        keys.Add(entries[entryIndex].Key);
                    }
                }
            }
        }

        return keys;
    }
}
#endif
