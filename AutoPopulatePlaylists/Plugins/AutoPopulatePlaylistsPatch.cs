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
    internal class AutoPopulatePlaylistsPatch
    {
        static List<PlaylistData> AllPlaylistData = new List<PlaylistData>();
        static List<CategoryPanelData> CreatedPanels = new List<CategoryPanelData>();

        const int StartingGenreNo = 100;
        static int currentGenreNo = StartingGenreNo;

        public static void InitializePlaylistData()
        {
            AllPlaylistData.Clear();
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

            if (node["Data"] is not null)
            {
                var data = node["Data"].AsArray();
                for (int i = 0; i < data.Count; i++)
                {
                    AllPlaylistData.Add(new PlaylistData(data[i]));
                }
            }
            else
            {
                // I'd like to remove this
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

                    panel.InitializeCallback(delegate { return GetFilteredList(playlist); });
                    panel.AddToManager();
                    CreatedPanels.Add(panel);
                }
            }
        }

        public static void ResetCustomPlaylists()
        {
            for (int i = 0; i < CreatedPanels.Count; i++)
            {
                CreatedPanels[i].RemoveFromManager();
            }
            currentGenreNo = StartingGenreNo;
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
                        SongData data = SongData.GetSongData(songList[i]);
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

      
    }
}
