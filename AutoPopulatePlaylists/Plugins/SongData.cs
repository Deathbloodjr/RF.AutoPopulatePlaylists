using Scripts.UserData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPopulatePlaylists.Plugins
{
    internal class SongData
    {
        List<SongDifficultyData> DifficultyData { get; set; } = new List<SongDifficultyData>();

        public SongData(MusicDataInterface.MusicInfoAccesser musicInfo)
        {
            if (!BlockSongIdConstants.SongIds.Contains(musicInfo.Id))
            {
                DifficultyData.Add(new SongDifficultyData(musicInfo, EnsoData.EnsoLevelType.Easy));
                DifficultyData.Add(new SongDifficultyData(musicInfo, EnsoData.EnsoLevelType.Normal));
                DifficultyData.Add(new SongDifficultyData(musicInfo, EnsoData.EnsoLevelType.Hard));
                DifficultyData.Add(new SongDifficultyData(musicInfo, EnsoData.EnsoLevelType.Mania));
                DifficultyData.Add(new SongDifficultyData(musicInfo, EnsoData.EnsoLevelType.Ura));
            }
        }

        public bool IsValidWithFilter(PlaylistData playlistData)
        {
            for (int i = 0; i < DifficultyData.Count; i++)
            {
                if (DifficultyData[i].IsValidWithFilter(playlistData))
                {
                    return true;
                }
            }

            return false;
        }

        internal class SongDifficultyData
        {
            public bool IsEnabled { get; set; }
            public EnsoData.EnsoLevelType EnsoLevelType { get; set; }
            public int Star { get; set; }
            public DataConst.CrownType Crown { get; set; }
            public EnsoData.SongGenre Genre { get; set; }

            public SongDifficultyData(MusicDataInterface.MusicInfoAccesser musicInfo, EnsoData.EnsoLevelType level)
            {
                IsEnabled = true;
                if (musicInfo.Debug)
                {
                    IsEnabled = false;
                }
                EnsoLevelType = level;
                Star = musicInfo.Stars[(int)EnsoLevelType];
                Genre = (EnsoData.SongGenre)musicInfo.GenreNo;
                if (Star != 0)
                {
                    MusicDataUtility.GetNormalRecordInfo(0, musicInfo.UniqueId, level, out var result);
                    Crown = result.crown;
                }
                else
                {
                    IsEnabled = false;
                }
            }

            public bool IsValidWithFilter(PlaylistData playlistData)
            {
                if (!IsEnabled)
                {
                    return false;
                }
                if (playlistData.Difficulties.Count != 0 &&
                    !playlistData.Difficulties.Contains(EnsoLevelType))
                {
                    return false;
                }
                if (playlistData.Stars.Count != 0 &&
                    !playlistData.Stars.Contains(Star))
                {
                    return false;
                }
                if (playlistData.Crowns.Count != 0 &&
                    !playlistData.Crowns.Contains(Crown))
                {
                    return false;
                }
                if (playlistData.Genres.Count != 0 &&
                    !playlistData.Genres.Contains(Genre))
                {
                    return false;
                }
                return true;
            }
        }
    }
}
