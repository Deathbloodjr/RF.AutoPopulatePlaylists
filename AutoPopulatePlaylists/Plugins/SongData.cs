using Il2CppInterop.Runtime.InteropTypes.Arrays;
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
           
            if (musicInfo.Debug)
            {
                return;
            }
            if (musicInfo.Session != "")
            {
                return;
            }

            DifficultyData.Add(new SongDifficultyData(musicInfo, EnsoData.EnsoLevelType.Easy));
            DifficultyData.Add(new SongDifficultyData(musicInfo, EnsoData.EnsoLevelType.Normal));
            DifficultyData.Add(new SongDifficultyData(musicInfo, EnsoData.EnsoLevelType.Hard));
            DifficultyData.Add(new SongDifficultyData(musicInfo, EnsoData.EnsoLevelType.Mania));
            if (musicInfo.Stars[(int)EnsoData.EnsoLevelType.Ura] != 0)
            {
                DifficultyData.Add(new SongDifficultyData(musicInfo, EnsoData.EnsoLevelType.Ura));
            }
        }

        public List<SongDifficultyData> GetValidSongDifficulties(PlaylistData playlistData)
        {
            List<SongDifficultyData> validDifficulties = new List<SongDifficultyData>();
            for (int i = 0; i < DifficultyData.Count; i++)
            {
                if (DifficultyData[i].IsValidWithFilter(playlistData))
                {
                    validDifficulties.Add(DifficultyData[i]);
                }
            }
            return validDifficulties;
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
            public MusicDataInterface.MusicInfoAccesser MusicInfo { get; set; } = null;
            public EnsoData.EnsoLevelType EnsoLevelType { get; set; } = EnsoData.EnsoLevelType.Num;
            public EnsoRecordInfo Record { get; set; }
            public EnsoData.SongGenre Genre
            {
                get
                {
                    if (MusicInfo != null)
                    {
                        return (EnsoData.SongGenre)MusicInfo.GenreNo;
                    }
                    else
                    {
                        return EnsoData.SongGenre.Namco;
                    }
                }
            }
            public int Star { 
                get
                {
                    if (MusicInfo != null && EnsoLevelType != EnsoData.EnsoLevelType.Num)
                    {
                        return MusicInfo.Stars[(int)EnsoLevelType];
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            public DataConst.CrownType Crown
            {
                get
                {
                    return Record.crown;
                }
            }
            public int Score
            {
                get
                {
                    return Record.normalHiScore.score;
                }
            }
            public int ScoreShinuchi
            {
                get
                {
                    return Record.shinuchiHiScore.score;
                }
            }
            int NormalNoteCount
            {
                get
                {
                    return Record.normalHiScore.excellent + Record.normalHiScore.good + Record.normalHiScore.bad;
                }
            }
            int ShinuchiNoteCount
            {
                get
                {
                    return Record.shinuchiHiScore.excellent + Record.shinuchiHiScore.good + Record.shinuchiHiScore.bad;
                }
            }
            public int Goods
            {
                get
                {
                    // This isn't really needed for Goods, but it is necessary for OKs and Bads
                    return Math.Max(Record.normalHiScore.excellent, Record.shinuchiHiScore.excellent);
                }
            }
            public int Oks
            {
                get
                {
                    if (NormalNoteCount != 0 && ShinuchiNoteCount != 0)
                    {
                        return Math.Min(Record.normalHiScore.good, Record.shinuchiHiScore.good);
                    }
                    else if (NormalNoteCount != 0)
                    {
                        return Record.normalHiScore.good;
                    }
                    else if (ShinuchiNoteCount != 0)
                    {
                        return Record.shinuchiHiScore.good;
                    }
                    else
                    {
                        return int.MaxValue;
                    }
                }
            }
            public int Bads
            {
                get
                {
                    if (NormalNoteCount != 0 && ShinuchiNoteCount != 0)
                    {
                        return Math.Min(Record.normalHiScore.bad, Record.shinuchiHiScore.bad);
                    }
                    else if (NormalNoteCount != 0)
                    {
                        return Record.normalHiScore.bad;
                    }
                    else if (ShinuchiNoteCount != 0)
                    {
                        return Record.shinuchiHiScore.bad;
                    }
                    else
                    {
                        return int.MaxValue;
                    }
                }
            }
            public int Drumroll
            {
                get
                {
                    return Math.Max(Record.normalHiScore.renda, Record.shinuchiHiScore.renda);
                }
            }
            public int Combo
            {
                get
                {
                    return Math.Max(Record.normalHiScore.combo, Record.shinuchiHiScore.combo);
                }
            }
            public int PlayCount
            {
                get
                {
                    return Record.playCount;
                }
            }
            /// <summary>
            /// Float from 0.0 to 1.0
            /// </summary>
            public float Accuracy
            {
                get
                {
                    return MathF.Max(NormalAccuracy, ShinuchiAccuracy);
                }
            }
            float NormalAccuracy
            {
                get
                {
                    int numNotes = Record.normalHiScore.excellent + Record.normalHiScore.good + Record.normalHiScore.bad;
                    if (numNotes == 0)
                    {
                        return 0f;
                    }
                    return (Record.normalHiScore.excellent + (Record.normalHiScore.good / 2f)) / numNotes;
                }
            }
            float ShinuchiAccuracy
            {
                get
                {
                    int numNotes = Record.shinuchiHiScore.excellent + Record.shinuchiHiScore.good + Record.shinuchiHiScore.bad;
                    if (numNotes == 0)
                    {
                        return 0f;
                    }
                    return (Record.shinuchiHiScore.excellent + (Record.shinuchiHiScore.good / 2f)) / numNotes;
                }
            }
            public SongDifficultyData(MusicDataInterface.MusicInfoAccesser musicInfo, EnsoData.EnsoLevelType level)
            {
                IsEnabled = true;
                MusicInfo = musicInfo;
                EnsoLevelType = level;
                //Star = musicInfo.Stars[(int)EnsoLevelType];
                //Genre = (EnsoData.SongGenre)musicInfo.GenreNo;
                if (Star != 0)
                {
                    MusicDataUtility.GetNormalRecordInfo(0, musicInfo.UniqueId, level, out var result);
                    Record = result;
                    //Crown = result.crown;
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
