using System;
using System.IO;
using System.Text.RegularExpressions;
using TeamCherry.Localization;

namespace VoicesOfPharloom;

public static class TextDumper
{
    public static string TextDumpPath { get { return Path.Combine(BepInEx.Paths.GameRootPath, "VoP_TextDump"); } }
    
    public static void DumpAllText()
    {
        string initLang = Language.CurrentLanguage().ToString();
        foreach(var langEnum in Enum.GetValues(typeof(GlobalEnums.SupportedLanguages)))
        {
            string lang = langEnum.ToString();
            Plugin.Logger.LogInfo($"Dumping text for language code {lang}...");
            Language.SwitchLanguage(lang);

            string langDumpPath = Path.Combine(TextDumpPath, lang);
            if (!Directory.Exists(langDumpPath))
                Directory.CreateDirectory(langDumpPath);

            foreach(string sheet in Language.GetSheets())
            {
                string sheetDumpPath = Path.Combine(langDumpPath, sheet);
                if (!Directory.Exists(sheetDumpPath))
                    Directory.CreateDirectory(sheetDumpPath);
                
                foreach (var key in Language.GetKeys(sheet))
                {
                    string text = Language.Get(key, sheet);
                    string[] lines = Regex.Split(text, @"\<.?page(=.)?\>");

                    int idx = 0;
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        string filePath = Path.Combine(sheetDumpPath, $"{key}_{idx++}.txt");
                        using StreamWriter writer = new StreamWriter(filePath);
                        writer.WriteLine(line);
                        writer.Flush();
                        writer.Close();
                    }
                }
            }
        }
        Language.SwitchLanguage(initLang);
    }
}