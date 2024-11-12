using BepInEx.Configuration;
using HarmonyLib;
using Scripts.GameSystem;
using Scripts.OutGame.SongSelect;
using Scripts.UserData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static MusicDataInterface;

namespace AutoPopulatePlaylists.Plugins
{
    public enum Playlist
    {
        None,
        Playlist1,
        Playlist2,
        Playlist3,
        Playlist4,
        Playlist5,
    }

    internal class AutoPopulatePlaylistsPatch
    {
        static Dictionary<Playlist, PlaylistData> PlaylistData = new Dictionary<Playlist, PlaylistData>();

        static Dictionary<string, string> KeyReplacements = new Dictionary<string, string>();

        public static void InitializePlaylistData()
        {
            string HardcodedFilePath = @"D:\Workspace\AutoPopulatePlaylists.json";
            var node = JsonNode.Parse(File.ReadAllText(HardcodedFilePath));
            var playlist1 = new PlaylistData(node["Playlist 1"]);
            var playlist2 = new PlaylistData(node["Playlist 2"]);
            var playlist3 = new PlaylistData(node["Playlist 3"]);
            var playlist4 = new PlaylistData(node["Playlist 4"]);
            var playlist5 = new PlaylistData(node["Playlist 5"]);

            if (!PlaylistData.TryAdd(Playlist.Playlist1, playlist1))
            {
                PlaylistData[Playlist.Playlist1] = playlist1;
            }
            if (!PlaylistData.TryAdd(Playlist.Playlist2, playlist2))
            {
                PlaylistData[Playlist.Playlist2] = playlist2;
            }
            if (!PlaylistData.TryAdd(Playlist.Playlist3, playlist3))
            {
                PlaylistData[Playlist.Playlist3] = playlist3;
            }
            if (!PlaylistData.TryAdd(Playlist.Playlist4, playlist4))
            {
                PlaylistData[Playlist.Playlist4] = playlist4;
            }
            if (!PlaylistData.TryAdd(Playlist.Playlist5, playlist5))
            {
                PlaylistData[Playlist.Playlist5] = playlist5;
            }

            if (playlist1.IsEnabled)
            {
                if (!KeyReplacements.TryAdd("category_playlist_1", playlist1.Name))
                {
                    KeyReplacements["category_playlist_1"] = playlist1.Name;
                }
            }
            if (playlist2.IsEnabled)
            {
                if (!KeyReplacements.TryAdd("category_playlist_2", playlist2.Name))
                {
                    KeyReplacements["category_playlist_2"] = playlist2.Name;
                }
            }
            if (playlist3.IsEnabled)
            {
                if (!KeyReplacements.TryAdd("category_playlist_3", playlist3.Name))
                {
                    KeyReplacements["category_playlist_3"] = playlist3.Name;
                }
            }
            if (playlist4.IsEnabled)
            {
                if (!KeyReplacements.TryAdd("category_playlist_4", playlist4.Name))
                {
                    KeyReplacements["category_playlist_4"] = playlist4.Name;
                }
            }
            if (playlist5.IsEnabled)
            {
                if (!KeyReplacements.TryAdd("category_playlist_5", playlist5.Name))
                {
                    KeyReplacements["category_playlist_5"] = playlist5.Name;
                }
            }
        }

        static List<MusicDataInterface.MusicInfoAccesser> GetFilteredList(Playlist playlist)
        {
            List<MusicDataInterface.MusicInfoAccesser> result = new List<MusicInfoAccesser>();

            var songList = SingletonMonoBehaviour<CommonObjects>.Instance.MyDataManager.MusicData.MusicInfoAccesserList;

            var playlistData = PlaylistData[playlist];
            if (playlistData.IsEnabled)
            {
                for (int i = 0; i < songList.Count; i++)
                {
                    SongData data = new SongData(songList[i]);
                    if (data.IsValidWithFilter(playlistData))
                    {
                        result.Add(songList[i]);
                    }
                }

                //var count = result.Count;
                //for (int j = 0; j < 400; j++)
                //{
                //    for (int i = 0; i < count; i++)
                //    {
                //        result.Add(result[i]);
                //    }
                //}
            }

            return result;
        }

        //[HarmonyPatch(typeof(UiSongScroller))]
        //[HarmonyPatch(nameof(UiSongScroller.Setup))]
        //[HarmonyPatch(MethodType.Normal)]
        //[HarmonyPrefix]
        //public static void UiSongScroller_Setup_Prefix(UiSongScroller __instance)
        //{
        //    InitializePlaylistData();
        //}



        //[HarmonyPatch(typeof(SongScroller))]
        //[HarmonyPatch(nameof(SongScroller.IsSameList))]
        //[HarmonyPatch(MethodType.Normal)]
        //[HarmonyPrefix]
        //public static void SongScroller_IsSameList_Prefix(SongScroller __instance, FilterTypes filter)
        //{
        //    Logger.Log("SongScroller_IsSameList_Prefix");
        //    Logger.Log("filter: " + filter.ToString());
        //}


        [HarmonyPatch(typeof(SongScroller))]
        [HarmonyPatch(nameof(SongScroller.CreateItemList))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPrefix]
        public static void SongScroller_CreateItemList_Prefix(SongScroller __instance, Il2CppSystem.Collections.Generic.List<MusicDataInterface.MusicInfoAccesser> list)
        {
            InitializePlaylistData();

            Playlist currentPlaylist = Playlist.None;
            switch (__instance.filter)
            {
                case FilterTypes.Playlist1: currentPlaylist = Playlist.Playlist1; break;
                case FilterTypes.Playlist2: currentPlaylist = Playlist.Playlist2; break;
                case FilterTypes.Playlist3: currentPlaylist = Playlist.Playlist3; break;
                case FilterTypes.Playlist4: currentPlaylist = Playlist.Playlist4; break;
                case FilterTypes.Playlist5: currentPlaylist = Playlist.Playlist5; break;
            }
            if (currentPlaylist != Playlist.None)
            {
                var newList = GetFilteredList(currentPlaylist);
                if (newList.Count > 0)
                {
                    list.Clear();
                    for (int i = 0; i < newList.Count; i++)
                    {
                        list.Add(newList[i]);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(WordDataManager))]
        [HarmonyPatch(nameof(WordDataManager.GetWordListInfo))]
        [HarmonyPatch(new Type[] { typeof(string) })]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void WordDataManager_GetWordListInfo_Postfix(WordDataManager __instance, ref WordDataManager.WordListKeysInfo __result, string key)
        {
            if (key != null)
            {
                if (KeyReplacements.ContainsKey(key))
                {
                    __result.Text = KeyReplacements[key];
                }
            }
        }
    }
}
