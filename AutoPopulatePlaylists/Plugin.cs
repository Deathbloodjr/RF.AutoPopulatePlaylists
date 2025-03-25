using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.Configuration;
using AutoPopulatePlaylists.Plugins;
using UnityEngine;
using System.Collections;
using SaveProfileManager.Plugins;
using System.Reflection;

namespace AutoPopulatePlaylists
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, ModName, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.DB.RF.CustomPlaylists")]
    public class Plugin : BasePlugin
    {
        public const string ModName = "AutoPopulatePlaylists";

        public static Plugin Instance;
        private Harmony _harmony = null;
        public new static ManualLogSource Log;


        public ConfigEntry<bool> ConfigEnabled;
        public ConfigEntry<string> ConfigPlaylistDataPath;


        public override void Load()
        {
            Instance = this;

            Log = base.Log;

            SetupConfig(Config, Path.Combine("BepInEx", "data", ModName));
            SetupHarmony();

            var isSaveManagerLoaded = IsSaveManagerLoaded();
            if (isSaveManagerLoaded)
            {
                AddToSaveManager();
            }
        }


        // Any data that's likely to be shared between multiple profiles should use the dataFolder path
        // Any data that's likely to be specific per profile should use the saveFolder path
        private void SetupConfig(ConfigFile config, string saveFolder, bool isSaveManager = false)
        {
            var dataFolder = Path.Combine("BepInEx", "data", ModName);

            if (!isSaveManager)
            {
                ConfigEnabled = config.Bind("General",
                   "Enabled",
                   true,
                   "Enables the mod.");
            }

            ConfigPlaylistDataPath = config.Bind("General",
                "PlaylistDataPath",
                Path.Combine(saveFolder, "AutoPopulatePlaylists.json"),
                "The file path containing your Playlist Data.");
        }

        private void SetupHarmony()
        {
            // Patch methods
            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

            LoadPlugin(Instance.ConfigEnabled.Value);
        }

        public static void LoadPlugin(bool enabled)
        {
            if (enabled)
            {
                bool result = true;
                // If any PatchFile fails, result will become false
                result &= Instance.PatchFile(typeof(AutoPopulatePlaylistsPatch));
                result &= Instance.PatchFile(typeof(LanguageHook));
                DefaultJsonCreation.CreateDefaultFile();
                AutoPopulatePlaylistsPatch.InitializePlaylistData();
                AutoPopulatePlaylistsPatch.CreateCustomPlaylists();
                if (result)
                {
                    Logger.Log($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
                }
                else
                {
                    Logger.Log($"Plugin {MyPluginInfo.PLUGIN_GUID} failed to load.", LogType.Error);
                    // Unload this instance of Harmony
                    // I hope this works the way I think it does
                    Instance._harmony.UnpatchSelf();
                }
            }
            else
            {
                Logger.Log($"Plugin {MyPluginInfo.PLUGIN_NAME} is disabled.");
            }
        }

        private bool PatchFile(Type type)
        {
            if (_harmony == null)
            {
                _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            }
            try
            {
                _harmony.PatchAll(type);
#if DEBUG
                Logger.Log("File patched: " + type.FullName);
#endif
                return true;
            }
            catch (Exception e)
            {
                Logger.Log("Failed to patch file: " + type.FullName);
                Logger.Log(e.Message);
                return false;
            }
        }

        public static void UnloadPlugin()
        {
            AutoPopulatePlaylistsPatch.ResetCustomPlaylists();
            Instance._harmony.UnpatchSelf();
            Logger.Log($"Plugin {MyPluginInfo.PLUGIN_NAME} has been unpatched.");
        }

        public static void ReloadPlugin()
        {
            // Reloading will always be completely different per mod
            // You'll want to reload any config file or save data that may be specific per profile
            // If there's nothing to reload, don't put anything here, and keep it commented in AddToSaveManager

            AutoPopulatePlaylistsPatch.ResetCustomPlaylists();
            DefaultJsonCreation.CreateDefaultFile();
            AutoPopulatePlaylistsPatch.InitializePlaylistData();
            AutoPopulatePlaylistsPatch.CreateCustomPlaylists();
        }

        public void AddToSaveManager()
        {
            // Add SaveDataManager dll path to your csproj.user file
            // https://github.com/Deathbloodjr/RF.SaveProfileManager
            var plugin = new PluginSaveDataInterface(MyPluginInfo.PLUGIN_GUID);
            plugin.AssignLoadFunction(LoadPlugin);
            plugin.AssignUnloadFunction(UnloadPlugin);
            plugin.AssignReloadSaveFunction(ReloadPlugin);
            plugin.AssignConfigSetupFunction(SetupConfig);
            plugin.AddToManager(ConfigEnabled.Value);
        }

        private bool IsSaveManagerLoaded()
        {
            try
            {
                Assembly loadedAssembly = Assembly.Load("com.DB.RF.SaveProfileManager");
                return loadedAssembly != null;
            }
            catch
            {
                return false;
            }
        }

        public static MonoBehaviour GetMonoBehaviour() => TaikoSingletonMonoBehaviour<CommonObjects>.Instance;
        public void StartCoroutine(IEnumerator enumerator)
        {
            GetMonoBehaviour().StartCoroutine(enumerator);
        }
    }
}
