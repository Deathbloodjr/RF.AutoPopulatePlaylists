using BepInEx.Configuration;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
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
        Pops,
        Anime,
        Vocaloid,
        Variety,
        Classical,
        GameMusic,
        NamcoOriginal,
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
            string playlistDataFilePath = Plugin.Instance.ConfigPlaylistDataPath.Value;
            if (!Directory.Exists(playlistDataFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(playlistDataFilePath));
            }
            if (!File.Exists(playlistDataFilePath))
            {
                return;
            }
            var node = JsonNode.Parse(File.ReadAllText(playlistDataFilePath));
            var pops = new PlaylistData(node["Pops"]);
            var anime = new PlaylistData(node["Anime"]);
            var vocaloid = new PlaylistData(node["Vocaloid"]);
            var variety = new PlaylistData(node["Variety"]);
            var classical = new PlaylistData(node["Classical"]);
            var gamemusic = new PlaylistData(node["GameMusic"]);
            var namco = new PlaylistData(node["NamcoOriginal"]);
            var playlist1 = new PlaylistData(node["Playlist 1"]);
            var playlist2 = new PlaylistData(node["Playlist 2"]);
            var playlist3 = new PlaylistData(node["Playlist 3"]);
            var playlist4 = new PlaylistData(node["Playlist 4"]);
            var playlist5 = new PlaylistData(node["Playlist 5"]);


            TryAddPlaylistData(Playlist.Pops, pops);
            TryAddPlaylistData(Playlist.Anime, anime);
            TryAddPlaylistData(Playlist.Vocaloid, vocaloid);
            TryAddPlaylistData(Playlist.Variety, variety);
            TryAddPlaylistData(Playlist.Classical, classical);
            TryAddPlaylistData(Playlist.GameMusic, gamemusic);
            TryAddPlaylistData(Playlist.NamcoOriginal, namco);
            TryAddPlaylistData(Playlist.Playlist1, playlist1);
            TryAddPlaylistData(Playlist.Playlist2, playlist2);
            TryAddPlaylistData(Playlist.Playlist3, playlist3);
            TryAddPlaylistData(Playlist.Playlist4, playlist4);
            TryAddPlaylistData(Playlist.Playlist5, playlist5);

            // This can't be done this way for genres
            // It will rename the genre everywhere it appears, rather than just in the filter list
            //TryAddKeyReplacement("genre_pops", pops);
            //TryAddKeyReplacement("genre_anime", anime);
            //TryAddKeyReplacement("genre_vocalo", vocaloid);
            //TryAddKeyReplacement("genre_", playlist1);
            //TryAddKeyReplacement("genre_", playlist1);
            //TryAddKeyReplacement("genre_", playlist1);
            //TryAddKeyReplacement("genre_", playlist1);
            TryAddKeyReplacement("category_playlist_1", playlist1);
            TryAddKeyReplacement("category_playlist_2", playlist2);
            TryAddKeyReplacement("category_playlist_3", playlist3);
            TryAddKeyReplacement("category_playlist_4", playlist4);
            TryAddKeyReplacement("category_playlist_5", playlist5);
        }

        private static void TryAddPlaylistData(Playlist playlist, PlaylistData playlistData)
        {
            if (!PlaylistData.TryAdd(playlist, playlistData))
            {
                PlaylistData[playlist] = playlistData;
            }
        }

        private static void TryAddKeyReplacement(string key, PlaylistData playlistData)
        {
            if (playlistData.IsEnabled)
            {
                if (!KeyReplacements.TryAdd(key, playlistData.Name))
                {
                    KeyReplacements[key] = playlistData.Name;
                }
            }
        }

        static List<MusicDataInterface.MusicInfoAccesser> GetFilteredList(Playlist playlist)
        {
            List<MusicDataInterface.MusicInfoAccesser> result = new List<MusicInfoAccesser>();

            var songList = SingletonMonoBehaviour<CommonObjects>.Instance.MyDataManager.MusicData.MusicInfoAccesserList;
            Il2CppReferenceArray<Scripts.UserData.MusicInfoEx> downloadedList = SingletonMonoBehaviour<CommonObjects>.Instance.MusicData.Datas;

            if (PlaylistData.ContainsKey(playlist))
            {
                var playlistData = PlaylistData[playlist];
                if (playlistData.IsEnabled)
                {
                    for (int i = 0; i < songList.Count; i++)
                    {
                        SongData data = new SongData(songList[i], downloadedList);
                        if (data.IsValidWithFilter(playlistData))
                        {
                            result.Add(songList[i]);
                        }
                    }
                }
            }

            return result;
        }


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
                case FilterTypes.Pops: currentPlaylist = Playlist.Pops; break;
                case FilterTypes.Anime: currentPlaylist = Playlist.Anime; break;
                case FilterTypes.Vocalo: currentPlaylist = Playlist.Vocaloid; break;
                case FilterTypes.Variety: currentPlaylist = Playlist.Variety; break;
                case FilterTypes.Classic: currentPlaylist = Playlist.Classical; break;
                case FilterTypes.Game: currentPlaylist = Playlist.GameMusic; break;
                case FilterTypes.Namco: currentPlaylist = Playlist.NamcoOriginal; break;
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
