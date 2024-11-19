using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataConst;

namespace AutoPopulatePlaylists.Plugins
{
    internal class LanguageHook
    {
        public static LanguageType CurrentLanguage = LanguageType.English;

        [HarmonyPatch(typeof(WordDataInterface))]
        [HarmonyPatch(nameof(WordDataInterface.ChangeLanguage))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void WordDataInterface_ChangeLanguage_Postfix(WordDataInterface __instance, string language)
        {
            CurrentLanguage = SelectLanguage.GetLanguageType(language);
        }
    }
}
