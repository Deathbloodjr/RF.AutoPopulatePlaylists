﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutoPopulatePlaylists.Plugins.SongData;

namespace AutoPopulatePlaylists.Plugins
{
    public enum SortType
    {
        Default, // Is default even needed? maybe just to break out?
        Order,
        Genre,
        Difficulty,
        AlphabeticalTitle,
        AlphabeticalSubtitle,
        AlphabeticalSongId,
        UniqueId, // I don't know why anyone would ever want to sort by UniqueId, but go ahead
        Score,
        ScoreShinuchi,
        Accuracy,
        Goods,
        Oks,
        Bads,
        DrumrollCount,
        MaxCombo,
        PlayCount,
    }

    internal class SongListSorter
    {
        static Dictionary<SortType, Func<SongDifficultyData, string>> SortStringFunctions = new Dictionary<SortType, Func<SongDifficultyData, string>>();
        static Dictionary<SortType, Func<SongDifficultyData, int>> SortIntFunctions = new Dictionary<SortType, Func<SongDifficultyData, int>>();
        static Dictionary<SortType, Func<SongDifficultyData, float>> SortFloatFunctions = new Dictionary<SortType, Func<SongDifficultyData, float>>();

        static List<SortType> RemoveDuplicateSorts = new List<SortType>()
        {
            SortType.AlphabeticalTitle,
            SortType.AlphabeticalSubtitle,
            SortType.AlphabeticalSongId,
            SortType.UniqueId,
            SortType.Order,
        };

        static void InitializeSortFunctions()
        {
            AddOrReplaceSortFunction(SortType.Order, (x) => x.MusicInfo.Order);
            AddOrReplaceSortFunction(SortType.Genre, (x) => (int)x.Genre);
            AddOrReplaceSortFunction(SortType.Difficulty, (x) => x.Star);
            AddOrReplaceSortFunction(SortType.AlphabeticalTitle, (x) => x.MusicInfo.SongNames[(int)LanguageHook.CurrentLanguage]);
            AddOrReplaceSortFunction(SortType.AlphabeticalSubtitle, (x) => x.MusicInfo.SongSubs[(int)LanguageHook.CurrentLanguage]);
            AddOrReplaceSortFunction(SortType.AlphabeticalSongId, (x) => x.MusicInfo.Id);
            AddOrReplaceSortFunction(SortType.UniqueId, (x) => x.MusicInfo.UniqueId);
            AddOrReplaceSortFunction(SortType.Score, (x) => x.Score);
            AddOrReplaceSortFunction(SortType.ScoreShinuchi, (x) => x.ScoreShinuchi);
            AddOrReplaceSortFunction(SortType.Accuracy, (x) => x.Accuracy);
            AddOrReplaceSortFunction(SortType.Goods, (x) => x.Goods);
            AddOrReplaceSortFunction(SortType.Oks, (x) => x.Oks);
            AddOrReplaceSortFunction(SortType.Bads, (x) => x.Bads);
            AddOrReplaceSortFunction(SortType.DrumrollCount, (x) => x.Drumroll);
            AddOrReplaceSortFunction(SortType.MaxCombo, (x) => x.Combo);
            AddOrReplaceSortFunction(SortType.PlayCount, (x) => x.PlayCount);
        }

        static void AddOrReplaceSortFunction(SortType type, Func<SongDifficultyData, string> func)
        {
            if (!SortStringFunctions.ContainsKey(type))
            {
                SortStringFunctions.Add(type, func);
            }
        }

        static void AddOrReplaceSortFunction(SortType type, Func<SongDifficultyData, int> func)
        {
            if (!SortIntFunctions.ContainsKey(type))
            {
                SortIntFunctions.Add(type, func);
            }
        }

        static void AddOrReplaceSortFunction(SortType type, Func<SongDifficultyData, float> func)
        {
            if (!SortFloatFunctions.ContainsKey(type))
            {
                SortFloatFunctions.Add(type, func);
            }
        }

        static IOrderedEnumerable<SongDifficultyData> SortSongs(List<SongDifficultyData> songs, SortType sortType)
        {
            if (SortStringFunctions.ContainsKey(sortType))
            {
                return songs.OrderBy(SortStringFunctions[sortType]);
            }
            else if (SortIntFunctions.ContainsKey(sortType))
            {
                return songs.OrderBy(SortIntFunctions[sortType]);
            }
            else if (SortFloatFunctions.ContainsKey(sortType))
            {
                return songs.OrderBy(SortFloatFunctions[sortType]);
            }
            else
            {
                Logger.Log("Issue attempting to sort by " + sortType.ToString());
                return null;
            }
        }

        static IOrderedEnumerable<SongDifficultyData> SortSongs(IOrderedEnumerable<SongDifficultyData> songs, SortType sortType)
        {
            if (SortStringFunctions.ContainsKey(sortType))
            {
                return songs.ThenBy(SortStringFunctions[sortType]);
            }
            else if (SortIntFunctions.ContainsKey(sortType))
            {
                return songs.ThenBy(SortIntFunctions[sortType]);
            }
            else if (SortFloatFunctions.ContainsKey(sortType))
            {
                return songs.ThenBy(SortFloatFunctions[sortType]);
            }
            else
            {
                Logger.Log("Issue attempting to sort by " + sortType.ToString());
                return null;
            }
        }

        public static List<SongDifficultyData> SortSongs(List<SongDifficultyData> songs, List<SortType> sortTypes)
        {
            if (sortTypes.Count == 0)
            {
                return songs;
            }

            InitializeSortFunctions();

            var sortedList = SortSongs(songs, sortTypes[0]);
            for (int i = 1; i < sortTypes.Count; i++)
            {
                sortedList = SortSongs(sortedList, sortTypes[i]);
            }

            return sortedList.ToList();
        }

        public static bool RemoveDuplicates(List<SortType> sortTypes)
        {
            if (sortTypes.Count == 0)
            {
                return true;
            }
            if (RemoveDuplicateSorts.Contains(sortTypes[0]))
            {
                return true;
            }
            return false;
        }
    }
}