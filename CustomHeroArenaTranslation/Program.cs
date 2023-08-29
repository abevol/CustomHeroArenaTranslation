using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValveKeyValue;

namespace CustomHeroArenaTranslation
{
    internal class Program
    {
        private static string ReferencesPath = @"D:\workspace\games\dota2\works\CustomHeroArenaTranslation\TranslationData\References";
        private static string GeneratedPath = @"D:\workspace\games\dota2\works\CustomHeroArenaTranslation\TranslationData\Generated";

        private class AddonLang
        {
            public string Language { get; set; }
            public Dictionary<string, string> Tokens { get; set; } = new();
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

            var arenaSChinese = new AddonLang { Language = "SChinese" };

            foreach (var kv in arenaRussian)
            {
                Console.WriteLine(kv.Name + ": " + kv.Value);
            }

            using var ws = File.OpenWrite($"{GeneratedPath}\\arena\\addon_schinese.txt");
            serializer.Serialize(ws, arenaSChinese, "lang", new KVSerializerOptions { HasEscapeSequences = true });
        }
    }
}