using BepInEx.Configuration;
using CustomPlaylists.Plugins;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Runtime.Serialization;
using Scripts.GameSystem;
using Scripts.OutGame.SongSelect;
using Scripts.UserData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using UnityEngine;
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

        static List<PlaylistData> AllPlaylistData = new List<PlaylistData>();

        static int currentGenreNo = 100;

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

            if (node["Data"] != null)
            {
                var data = node["Data"].AsArray();
                for (int i = 0; i < data.Count; i++)
                {
                    AllPlaylistData.Add(new PlaylistData(data[i]));
                }
            }
            else
            {
                var pops = new PlaylistData(node["Pops"], true);
                var anime = new PlaylistData(node["Anime"], true);
                var vocaloid = new PlaylistData(node["Vocaloid"], true);
                var variety = new PlaylistData(node["Variety"], true);
                var classical = new PlaylistData(node["Classical"], true);
                var gamemusic = new PlaylistData(node["Game Music"], true);
                var namco = new PlaylistData(node["Namco Original"], true);
                var playlist1 = new PlaylistData(node["Playlist 1"], true);
                var playlist2 = new PlaylistData(node["Playlist 2"], true);
                var playlist3 = new PlaylistData(node["Playlist 3"], true);
                var playlist4 = new PlaylistData(node["Playlist 4"], true);
                var playlist5 = new PlaylistData(node["Playlist 5"], true);

                AllPlaylistData.Add(pops);
                AllPlaylistData.Add(anime);
                AllPlaylistData.Add(vocaloid);
                AllPlaylistData.Add(variety);
                AllPlaylistData.Add(classical);
                AllPlaylistData.Add(gamemusic);
                AllPlaylistData.Add(namco);
                AllPlaylistData.Add(playlist1);
                AllPlaylistData.Add(playlist2);
                AllPlaylistData.Add(playlist3);
                AllPlaylistData.Add(playlist4);
                AllPlaylistData.Add(playlist5);

                DefaultJsonCreation.OutputPlaylistData(AllPlaylistData);
            }

        }

        public static void CreateCustomPlaylists()
        {
            for (int i = 0; i < AllPlaylistData.Count; i++)
            {
                var playlist = AllPlaylistData[i];
                if (playlist.IsEnabled)
                {
                    CategoryPanelData panel = new CategoryPanelData()
                    {
                        Name = playlist.Name,
                        GenreId = currentGenreNo++,
                        BgColor = playlist.BgColor,
                        FrameType = playlist.FrameType,
                        FrameColor = playlist.FrameColor,
                    };
                    //if (playlist.Crowns.Count == 1)
                    //{
                    //    if (playlist.Crowns[0] == DataConst.CrownType.Silver)
                    //    {
                    //        panel.FrameColor = Color.gray;
                    //    }
                    //    else if (playlist.Crowns[0] == DataConst.CrownType.Gold)
                    //    {
                    //        panel.FrameColor = Color.yellow;
                    //    }
                    //    else if (playlist.Crowns[0] == DataConst.CrownType.Rainbow)
                    //    {
                    //        panel.FrameType = FrameType.Gradient;
                    //        panel.FrameColor = Color.white;
                    //        //panel.FrameColor = Color.yellow;
                    //    }
                    //}
                    panel.InitializeCallback(delegate { return GetFilteredList(playlist); });
                    CategoryPanelManager.AddCategoryPanel(panel);
                }
            }
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

        static List<MusicDataInterface.MusicInfoAccesser> GetFilteredList(PlaylistData playlist)
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

            if (playlist.IsEnabled)
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
                        (songList[i].IsDefault || songList[i].InPackage == MusicDataInterface.InPackageType.HasSongAndFumen))
                    {
                        SongData data = new SongData(songList[i]);
                        songDataList.AddRange(data.GetValidSongDifficulties(playlist));
                        //if (data.IsValidWithFilter(playlistData))
                        //{
                        //    songDataList.Add()
                        //    result.Add(songList[i]);
                        //}
                    }
                }

                bool removeDuplicates = false;
                if (playlist.SortTypes.Count > 0)
                {
                    songDataList = SongListSorter.SortSongs(songDataList, playlist);
                    removeDuplicates = SongListSorter.RemoveDuplicates(playlist);
                }

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

            return result;
        }

        // Just for testing that SongId functionality works
        static List<string> GetFilteredListSongId(PlaylistData playlist)
        {
            List<string> result = new List<string>();

            var songList = SingletonMonoBehaviour<CommonObjects>.Instance.MyDataManager.MusicData.MusicInfoAccesserList;
            var musicPassSongList = SingletonMonoBehaviour<CommonObjects>.Instance.ServerDataCache.SonglistDetails.ary_release_song;
            var grouping = SingletonMonoBehaviour<CommonObjects>.Instance.ServerDataCache.GroupingDetails.ary_grouping;
            List<int> validUniqueIds = new List<int>();
            for (int i = 0; i < musicPassSongList.Count; i++)
            {
                validUniqueIds.Add(musicPassSongList[i].song_uid);
            }

            if (playlist.IsEnabled)
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
                        (songList[i].IsDefault || songList[i].InPackage == MusicDataInterface.InPackageType.HasSongAndFumen))
                    {
                        SongData data = new SongData(songList[i]);
                        songDataList.AddRange(data.GetValidSongDifficulties(playlist));
                        //if (data.IsValidWithFilter(playlistData))
                        //{
                        //    songDataList.Add()
                        //    result.Add(songList[i]);
                        //}
                    }
                }

                bool removeDuplicates = false;
                if (playlist.SortTypes.Count > 0)
                {
                    songDataList = SongListSorter.SortSongs(songDataList, playlist);
                    removeDuplicates = SongListSorter.RemoveDuplicates(playlist);
                }

                for (int i = 0; i < songDataList.Count; i++)
                {
                    if (i >= 1)
                    {
                        if (removeDuplicates && songDataList[i].MusicInfo.Id == result[result.Count - 1])
                        {
                            continue;
                        }
                    }
                    result.Add(songDataList[i].MusicInfo.Id);
                }
            }

            return result;
        }

        // Just for testing that UniqueId functionality works
        static List<int> GetFilteredListUniqueId(PlaylistData playlist)
        {
            List<int> result = new List<int>();

            var songList = SingletonMonoBehaviour<CommonObjects>.Instance.MyDataManager.MusicData.MusicInfoAccesserList;
            var musicPassSongList = SingletonMonoBehaviour<CommonObjects>.Instance.ServerDataCache.SonglistDetails.ary_release_song;
            var grouping = SingletonMonoBehaviour<CommonObjects>.Instance.ServerDataCache.GroupingDetails.ary_grouping;
            List<int> validUniqueIds = new List<int>();
            for (int i = 0; i < musicPassSongList.Count; i++)
            {
                validUniqueIds.Add(musicPassSongList[i].song_uid);
            }

            if (playlist.IsEnabled)
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
                        (songList[i].IsDefault || songList[i].InPackage == MusicDataInterface.InPackageType.HasSongAndFumen))
                    {
                        SongData data = new SongData(songList[i]);
                        songDataList.AddRange(data.GetValidSongDifficulties(playlist));
                        //if (data.IsValidWithFilter(playlistData))
                        //{
                        //    songDataList.Add()
                        //    result.Add(songList[i]);
                        //}
                    }
                }

                bool removeDuplicates = false;
                if (playlist.SortTypes.Count > 0)
                {
                    songDataList = SongListSorter.SortSongs(songDataList, playlist);
                    removeDuplicates = SongListSorter.RemoveDuplicates(playlist);
                }

                for (int i = 0; i < songDataList.Count; i++)
                {
                    if (i >= 1)
                    {
                        if (removeDuplicates && songDataList[i].MusicInfo.UniqueId == result[result.Count - 1])
                        {
                            continue;
                        }
                    }
                    result.Add(songDataList[i].MusicInfo.UniqueId);
                }
            }

            return result;
        }

        static List<MusicDataInterface.MusicInfoAccesser> GetFilteredList(Playlist playlist)
        {
            if (PlaylistData.ContainsKey(playlist))
            {
                return GetFilteredList(PlaylistData[playlist]);
            }
            else
            {
                return new List<MusicDataInterface.MusicInfoAccesser>();
            }
        }


        //[HarmonyPatch(typeof(SongScroller))]
        //[HarmonyPatch(nameof(SongScroller.CreateItemList))]
        //[HarmonyPatch(MethodType.Normal)]
        //[HarmonyPrefix]
        public static void SongScroller_CreateItemList_Prefix(SongScroller __instance, Il2CppSystem.Collections.Generic.List<MusicDataInterface.MusicInfoAccesser> list)
        {
            // This was initally here to reinitialize playlist data without restarting the game
            // I think it was an unnecessary feature, and with CustomPlaylists, it'd be much harder to do properly
            //InitializePlaylistData();

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


        //[HarmonyPatch(typeof(UiFilterButton))]
        //[HarmonyPatch(nameof(UiFilterButton.SetPanel))]
        //[HarmonyPatch(MethodType.Normal)]
        //[HarmonyPostfix]
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
