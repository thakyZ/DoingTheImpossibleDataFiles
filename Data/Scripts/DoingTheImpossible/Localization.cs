using Sandbox.ModAPI;
using SpaceEquipmentLtd.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using VRage;
using VRage.Utils;

namespace SpaceEquipmentLtd.Localization
{
  public static class LocalizationHelper
  {
    public static Dictionary<string, string> GetTexts(MyLanguagesEnum language, Dictionary<MyLanguagesEnum, Dictionary<string, string>> translations, Logging log = null)
    {
      Dictionary<string, string> texts = new Dictionary<string, string>();

      Dictionary<string, string> fallbackTranslation = translations[MyLanguagesEnum.English]; //Should be a complete set (all possible entries)
      Dictionary<string, string> requestedTranslation;
      if (language == MyLanguagesEnum.English || !translations.TryGetValue(language, out requestedTranslation))
      {
        requestedTranslation = null;
      }

      foreach (KeyValuePair<string, string> kv in fallbackTranslation)
      {
        string translation;
        if (requestedTranslation == null || !requestedTranslation.TryGetValue(kv.Key, out translation))
        {
          if (language != MyLanguagesEnum.English && log != null && log.ShouldLog(Logging.Level.Error))
          {
            log.Write(Logging.Level.Error, "Missing translation in language={0} for key={1}", language, kv.Key);
          }

          translation = kv.Value;
        }
        texts.Add(kv.Key, translation);
      }
      return texts;
    }

    public static MyStringId GetStringId(Dictionary<string, string> texts, string key)
    {
      try
      {
        return MyStringId.GetOrCompute(texts[key]);
      }
      catch (Exception ex)
      {
        throw new Exception($"GetStringId Failed for Key={key}", ex);
      }
    }

    public static void ExportDictionary(string destFileName, Dictionary<string, string> texts)
    {
      using (System.IO.TextWriter writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(destFileName, typeof(LocalizationHelper)))
      {
        foreach (KeyValuePair<string, string> kv in texts)
        {
          writer.WriteLine(string.Format("\"{0}\",\"{1}\"", kv.Key, kv.Value));
        }
      }
    }

    public static void ImportDictionary(string srcFileName, Dictionary<string, string> texts)
    {
      using (System.IO.TextReader reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(srcFileName, typeof(LocalizationHelper)))
      {
        Regex regexObj = new Regex(@"""[^""]*");
        string line;
        while (!string.IsNullOrEmpty(line = reader.ReadLine()))
        {
          MatchCollection matchResults = regexObj.Matches(line);
          if (matchResults != null && matchResults.Count == 2)
          {
            texts.Add(matchResults[0].Value.Replace("\"", ""), matchResults[1].Value.Replace("\"", ""));
          }
        }
      }
    }
  }
}
