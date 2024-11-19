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
using static AutoPopulatePlaylists.Plugins.SongData;

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
            var text = File.ReadAllText(playlistDataFilePath);
            var node = JsonNode.Parse(text);
            var pops = new PlaylistData(node["Pops"]);
            var anime = new PlaylistData(node["Anime"]);
            var vocaloid = new PlaylistData(node["Vocaloid"]);
            var variety = new PlaylistData(node["Variety"]);
            var classical = new PlaylistData(node["Classical"]);
            var gamemusic = new PlaylistData(node["Game Music"]);
            var namco = new PlaylistData(node["Namco Original"]);
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
        }

        private static void TryAddPlaylistData(Playlist playlist, PlaylistData playlistData)
        {
            if (playlistData.IsEnabled)
            {
                // Add PlaylistData
                if (!PlaylistData.TryAdd(playlist, playlistData))
                {
                    PlaylistData[playlist] = playlistData;
                }
            }
            else
            {
                // Remove PlaylistData if it exists
                if (PlaylistData.ContainsKey(playlist))
                {
                    PlaylistData.Remove(playlist);
                }
            }
            
        }

        static List<MusicDataInterface.MusicInfoAccesser> GetFilteredList(Playlist playlist)
        {
            List<MusicDataInterface.MusicInfoAccesser> result = new List<MusicDataInterface.MusicInfoAccesser>();

            var songList = SingletonMonoBehaviour<CommonObjects>.Instance.MyDataManager.MusicData.MusicInfoAccesserList;
            var musicPassSongList = SingletonMonoBehaviour<CommonObjects>.Instance.ServerDataCache.SonglistDetails.ary_release_song;
            var grouping = SingletonMonoBehaviour<CommonObjects>.Instance.ServerDataCache.GroupingDetails.ary_grouping;
            List<int> validUniqueIds = new List<int>();
            for (int i = 0; i < musicPassSongList.Count; i++)
            {
                validUniqueIds.Add(musicPassSongList[i].song_uid);
            }

            if (PlaylistData.ContainsKey(playlist))
            {
                var playlistData = PlaylistData[playlist];
                if (playlistData.IsEnabled)
                {
                    List<SongDifficultyData> songDataList = new List<SongDifficultyData>();
                    for (int i = 0; i < songList.Count; i++)
                    {
                        if (songList[i].Debug)
                        {
                            continue;
                        }
                        var uniqueId = songList[i].UniqueId;
                        if ((validUniqueIds.Contains(uniqueId) && 
                            SingletonMonoBehaviour<CommonObjects>.Instance.ServerDataCache.IsAvailableSong(songList[i])) || 
                            songList[i].IsDefault)
                        {
                            SongData data = new SongData(songList[i]);
                            songDataList.AddRange(data.GetValidSongDifficulties(playlistData));
                            //if (data.IsValidWithFilter(playlistData))
                            //{
                            //    songDataList.Add()
                            //    result.Add(songList[i]);
                            //}
                        }
                    }

                    List<SortType> sorts = new List<SortType>()
                    {
                        SortType.AlphabeticalTitle
                    };

                    songDataList = SongListSorter.SortSongs(songDataList, sorts);

                    bool removeDuplicates = SongListSorter.RemoveDuplicates(sorts);
                    for (int i = 0; i < songDataList.Count; i++)
                    {
                        if (i >= 1)
                        {
                            if (removeDuplicates && songDataList[i].MusicInfo == result[result.Count - 1])
                            {
                                continue;
                            }
                        }
                        result.Add(songDataList[i].MusicInfo);
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

            Playlist currentPlaylist = GetPlaylistFromFilterType(__instance.filter);

            if (currentPlaylist != Playlist.None)
            {
                if (PlaylistData.ContainsKey(currentPlaylist) &&
                    PlaylistData[currentPlaylist].IsEnabled)
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
        }

        public static Playlist GetPlaylistFromFilterType(FilterTypes filterType)
        {
            switch (filterType)
            {
                case FilterTypes.Pops: return Playlist.Pops;
                case FilterTypes.Anime: return Playlist.Anime;
                case FilterTypes.Vocalo: return Playlist.Vocaloid;
                case FilterTypes.Variety: return Playlist.Variety;
                case FilterTypes.Classic: return Playlist.Classical;
                case FilterTypes.Game: return Playlist.GameMusic;
                case FilterTypes.Namco: return Playlist.NamcoOriginal;
                case FilterTypes.Playlist1: return Playlist.Playlist1;
                case FilterTypes.Playlist2: return Playlist.Playlist2;
                case FilterTypes.Playlist3: return Playlist.Playlist3;
                case FilterTypes.Playlist4: return Playlist.Playlist4;
                case FilterTypes.Playlist5: return Playlist.Playlist5;
                default: return Playlist.None;
            }
        }


        [HarmonyPatch(typeof(UiFilterButton))]
        [HarmonyPatch(nameof(UiFilterButton.SetPanel))]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPostfix]
        public static void UiFilterButton_SetPanel_Postfix(UiFilterButton __instance)
        {
            var playlist = GetPlaylistFromFilterType(__instance.filter);
            if (PlaylistData.ContainsKey(playlist))
            {
                var playlistData = PlaylistData[playlist];
                if (playlistData.Name != "")
                {
                    __instance.textOn.SetTextRawOnly(playlistData.Name);
                    __instance.textOff.SetTextRawOnly(playlistData.Name);
                }
            }
        }
    }
}
