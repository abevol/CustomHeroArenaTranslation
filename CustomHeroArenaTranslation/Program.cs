using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValveKeyValue;

namespace CustomHeroArenaTranslation
{
    public static class Extensions
    {
        public static bool IsDefault<T>(this T value) where T : struct
        {
            bool isDefault = value.Equals(default(T));

            return isDefault;
        }
    }

    internal class Program
    {
        private static string ReferencesPath = @"D:\workspace\games\dota2\works\CustomHeroArenaTranslation\TranslationData\References";
        private static string GeneratedPath = @"D:\workspace\games\dota2\works\CustomHeroArenaTranslation\TranslationData\Generated";
        private static string DotalocalizationPath = @"D:\workspace\projects\dota2\mods\dota\resource\localization";

        private static List<string> _customKeys = new() { "_custom", "_lua" };
        private static Dictionary<string, string> _customValueTags = new()
        {
            {" (Heroic)", "【英雄】"},
            {" (Darkness)", "【黑暗】"},
            {"(Unremovable)", "【不可遗忘】" },
            { "(Frosthaven)", "【凌霜圣地】" }
        };

        private class AddonLang
        {
            public string Language { get; set; }
            public Dictionary<string, string> Tokens { get; set; } = new();
        }

        private static string GetOriKey(string key)
        {
            if (!key.StartsWith("DOTA_Tooltip"))
                return key;

            foreach (var customKey in _customKeys)
            {
                if (key.Contains(customKey))
                {
                    return key.Replace(customKey, "");
                }
            }

            return key;
        }

        private static string GetOriValue(string value, out string trTag)
        {
            foreach (var customValueTag in _customValueTags)
            {
                if (value.EndsWith(customValueTag.Key))
                {
                    trTag = customValueTag.Value;
                    return value.Replace(customValueTag.Key, "");
                }
            }

            trTag = "";
            return value;
        }

        static void Main(string[] args)
        {
            var serializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);

            KVObject arenaRussian = serializer.Deserialize(File.OpenRead($"{ReferencesPath}\\arena\\addon_russian.txt"), 
                new KVSerializerOptions{ HasEscapeSequences = true }).First(x => x.Name == "Tokens");
            KVObject chaosRussian = serializer.Deserialize(File.OpenRead($"{ReferencesPath}\\chaos\\addon_russian.txt"),
                new KVSerializerOptions { HasEscapeSequences = true }).First(x => x.Name == "Tokens");
            KVObject chaosSChinese = serializer.Deserialize(File.OpenRead($"{ReferencesPath}\\chaos\\addon_schinese.txt"),
                new KVSerializerOptions { HasEscapeSequences = true }).First(x => x.Name == "Tokens");
            KVObject dotaRussian = serializer.Deserialize(File.OpenRead($"{DotalocalizationPath}\\abilities_russian.txt"),
                new KVSerializerOptions { HasEscapeSequences = true }).First(x => x.Name == "Tokens");
            KVObject dotaSChinese = serializer.Deserialize(File.OpenRead($"{DotalocalizationPath}\\abilities_schinese.txt"),
                new KVSerializerOptions { HasEscapeSequences = true }).First(x => x.Name == "Tokens");

            var arenaSChinese = new AddonLang { Language = "SChinese" };
            var cachedCredibleTr = new Dictionary<string, string>();
            var cachedNotCredibleTr = new Dictionary<string, string>();
            foreach (var kv in arenaRussian)
            {
                HashSet<string> tags = new HashSet<string>();
                string keyName = kv.Name;
                string keyValue = kv.Value?.ToString() ?? "";
                string tr = keyValue;

                var crkv = chaosRussian.FirstOrDefault(x => x.Name == keyName);
                if (crkv == null)
                    tags.Add("no_cr");
                var csckv = chaosSChinese.FirstOrDefault(x => x.Name == keyName);
                if (csckv == null)
                    tags.Add("no_tr");

                if (crkv != null)
                {
                    if (keyValue != crkv.Value?.ToString())
                        tags.Add("nm_cr");
                }

                if (csckv != null)
                {
                    tr = csckv.Value?.ToString() ?? "";
                    if (tags.Count == 0 && !string.IsNullOrEmpty(tr))
                        cachedCredibleTr.TryAdd(keyValue, tr);
                }

                if (tags.Count > 0)
                {
                    var dKey = GetOriKey(keyName);
                    var dValue = GetOriValue(keyValue, out string trTag);

                    var drkv = dotaRussian.FirstOrDefault(x => x.Name == dKey);
                    var dsckv = dotaSChinese.FirstOrDefault(x => x.Name == dKey);

                    if (drkv != null)
                    {
                        if (dsckv == null)
                            Console.WriteLine($"sdckv is null: {dKey}");
                        else
                        {
                            if (dValue == drkv.Value?.ToString())
                            {
                                tags.Clear();
                                tr = dsckv.Value?.ToString() + trTag;
                                Console.WriteLine($"set tr from dota: dKey: {dKey}, tr: {tr}");
                                if (!string.IsNullOrEmpty(tr))
                                    cachedCredibleTr.TryAdd(keyValue, tr);
                            }
                            else if (tags.Contains("no_tr"))
                            {
                                tags.Add("nm_dr");
                                tags.Remove("no_tr");
                                tr = dsckv.Value?.ToString() + trTag;
                                Console.WriteLine($"set tr from dota: dKey: {dKey}, tr: {tr}");
                            }
                        }
                    }
                }

                // if (tags.Count > 0)
                // {
                //     var cachedTr = cachedCredibleTr.FirstOrDefault(x => x.Key == keyValue);
                //     if (!cachedTr.IsDefault())
                //     {
                //         tags.Clear();
                //         tr = cachedTr.Value;
                //         Console.WriteLine($"set tr from cachedTr: keyName: {keyName}, tr: {tr}");
                //     }
                // }

                string ts = tags.Any() ? "[tags(" + string.Join(",", tags) + ")] " : "";
                string v = ts + tr;

                if (arenaSChinese.Tokens.ContainsKey(keyName))
                {
                    Console.WriteLine($"Key exist: {keyName}");
                    continue;
                }
                arenaSChinese.Tokens.Add(keyName, v);

                if (tags.Any())
                {
                    cachedNotCredibleTr.TryAdd(keyName, keyValue);
                }
            }

            foreach (var notCredibleTr in cachedNotCredibleTr)
            {
                var credibleTr = cachedCredibleTr.FirstOrDefault(x => x.Key == notCredibleTr.Value);
                if (!credibleTr.IsDefault())
                {
                    arenaSChinese.Tokens[notCredibleTr.Key] = credibleTr.Value;
                    Console.WriteLine($"reset tr from cachedTr: keyName: {notCredibleTr.Key}, tr: {credibleTr.Value}");
                }
            }

            using var ws = File.OpenWrite($"{GeneratedPath}\\arena\\addon_schinese.txt");
            serializer.Serialize(ws, arenaSChinese, "lang", new KVSerializerOptions { HasEscapeSequences = true });
        }
    }
}