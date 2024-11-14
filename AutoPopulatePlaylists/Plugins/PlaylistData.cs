using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using UnityEngine.Android;

namespace AutoPopulatePlaylists.Plugins
{
    class PlaylistData
    {
        public bool IsEnabled { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<EnsoData.EnsoLevelType> Difficulties { get; set; } = new List<EnsoData.EnsoLevelType>();
        public List<int> Stars { get; set; } = new List<int>();
        public List<DataConst.CrownType> Crowns { get; set; } = new List<DataConst.CrownType>();
        public List<EnsoData.SongGenre> Genres { get; set; } = new List<EnsoData.SongGenre>();

        // I don't know if System.Text.Json is going to work properly within a mod, but I guess we'll give it a shot
        // Worst case, it isn't too rough to convert over to the Lightweight Json library
        // I forgot about this concern, it's been working perfectly for me, but the real test will be when others start using this mod
        // Another real test would be when other mods begin using System.Text.Json. Even then, it was working for me in TDMX, but not for others
        public PlaylistData(JsonNode node)
        {
            if (node == null)
            {
                IsEnabled = false;
            }
            else
            {
                IsEnabled = true;
                if (node["Name"] != null)
                {
                    Name = node["Name"].GetValue<string>();
                }
                if (node["Difficulties"] != null)
                {
                    var diffs = node["Difficulties"].AsArray();
                    //if (diffs.Count == 0)
                    //{
                    //    Difficulties.Add(EnsoData.EnsoLevelType.Easy);
                    //    Difficulties.Add(EnsoData.EnsoLevelType.Normal);
                    //    Difficulties.Add(EnsoData.EnsoLevelType.Hard);
                    //    Difficulties.Add(EnsoData.EnsoLevelType.Mania);
                    //    Difficulties.Add(EnsoData.EnsoLevelType.Ura);
                    //}
                    for (int i = 0; i < diffs.Count; i++)
                    {
                        var diff = diffs[i].GetValue<string>();
                        diff = diff.Trim().ToLower();
                        switch (diff)
                        {
                            case "easy":
                            case "kantan":
                            case "かんたん":
                                Difficulties.Add(EnsoData.EnsoLevelType.Easy);
                                break;
                            case "medium":
                            case "normal":
                            case "futsu":
                            case "futsuu":
                            case "ふつう":
                                Difficulties.Add(EnsoData.EnsoLevelType.Normal);
                                break;
                            case "hard":
                            case "muzukashi":
                            case "muzukashii":
                            case "むずかしい":
                                Difficulties.Add(EnsoData.EnsoLevelType.Hard);
                                break;
                            case "oni":
                            case "mania":
                            case "otome":
                            case "ex":
                            case "extreme":
                            case "おに":
                                Difficulties.Add(EnsoData.EnsoLevelType.Mania);
                                break;
                            case "ura":
                            case "uraoni":
                            case "ura oni":
                            case "extraextreme":
                            case "extra extreme":
                            case "おに (裏)":
                                Difficulties.Add(EnsoData.EnsoLevelType.Ura);
                                break;
                            default:
                                Logger.Log("Couldn't parse Difficulty value: " + diff);
                                break;
                        }
                    }
                }
                if (node["Stars"] != null)
                {
                    var stars = node["Stars"].AsArray();
                    //if (stars.Count == 0)
                    //{
                    //    // Don't judge...
                    //    Stars.Add(1);
                    //    Stars.Add(2);
                    //    Stars.Add(3);
                    //    Stars.Add(4);
                    //    Stars.Add(5);
                    //    Stars.Add(6);
                    //    Stars.Add(7);
                    //    Stars.Add(8);
                    //    Stars.Add(9);
                    //    Stars.Add(10);
                    //}
                    for (int i = 0; i < stars.Count; i++)
                    {
                        var star = stars[i].GetValue<int>();
                        if (star >= 1 && star <= 10)
                        {
                            Stars.Add(star);
                        }
                        else
                        {
                            Logger.Log("Couldn't add Star value: " + star);
                        }
                    }
                }
                if (node["Crowns"] != null)
                {
                    var crowns = node["Crowns"].AsArray();
                    //if (crowns.Count == 0)
                    //{
                    //    Crowns.Add(DataConst.CrownType.None);
                    //    Crowns.Add(DataConst.CrownType.Silver);
                    //    Crowns.Add(DataConst.CrownType.Gold);
                    //    Crowns.Add(DataConst.CrownType.Rainbow);
                    //}
                    for (int i = 0; i < crowns.Count; i++)
                    {
                        var crown = crowns[i].GetValue<string>();
                        crown = crown.Trim().ToLower();
                        switch (crown)
                        {
                            case "none":
                            case "not cleared":
                            case "off":
                                Crowns.Add(DataConst.CrownType.None);
                                break;
                            case "clear":
                            case "cleared":
                            case "silver":
                                Crowns.Add(DataConst.CrownType.Silver);
                                break;
                            case "fc":
                            case "fullcombo":
                            case "full combo":
                            case "gold":
                                Crowns.Add(DataConst.CrownType.Gold);
                                break;
                            case "dfc":
                            case "donderful combo":
                            case "rainbow":
                                Crowns.Add(DataConst.CrownType.Rainbow);
                                break;
                            default:
                                Logger.Log("Couldn't parse Crown value: " + crown);
                                break;
                        }
                    }
                }
                if (node["Genres"] != null)
                {
                    var genres = node["Genres"].AsArray();
                    //if (genres.Count == 0)
                    //{
                    //    Genres.Add(EnsoData.SongGenre.Pops);
                    //    Genres.Add(EnsoData.SongGenre.Anime);
                    //    Genres.Add(EnsoData.SongGenre.Vocalo);
                    //    Genres.Add(EnsoData.SongGenre.Variety);
                    //    Genres.Add(EnsoData.SongGenre.Classic);
                    //    Genres.Add(EnsoData.SongGenre.Game);
                    //    Genres.Add(EnsoData.SongGenre.Namco);
                    //}
                    for (int i = 0; i < genres.Count; i++)
                    {
                        var genre = genres[i].GetValue<string>();
                        genre = genre.Trim().ToLower();
                        switch (genre)
                        {
                            case "pop":
                            case "jpop":
                            case "pops":
                                Genres.Add(EnsoData.SongGenre.Pops);
                                break;
                            case "anime":
                                Genres.Add(EnsoData.SongGenre.Anime);
                                break;
                            case "vocalo":
                            case "vocaloid":
                                Genres.Add(EnsoData.SongGenre.Vocalo);
                                break;
                            case "variety":
                                Genres.Add(EnsoData.SongGenre.Variety);
                                break;
                            case "classic":
                            case "classical":
                                Genres.Add(EnsoData.SongGenre.Classic);
                                break;
                            case "game":
                            case "gamemusic":
                            case "game music":
                                Genres.Add(EnsoData.SongGenre.Game);
                                break;
                            case "namco":
                            case "namcooriginal":
                            case "namco original":
                                Genres.Add(EnsoData.SongGenre.Namco);
                                break;
                            default:
                                Logger.Log("Couldn't parse Genre value: " + genre);
                                break;
                        }
                    }
                }
            }
        }
    }
}
