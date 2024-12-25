using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using UnityEngine;
using static Il2CppSystem.Linq.Expressions.Interpreter.CastInstruction.CastInstructionNoT;

namespace AutoPopulatePlaylists.Plugins
{
    class DefaultJsonCreation
    {
        public static void CreateDefaultFile()
        {
            var filePath = Plugin.Instance.ConfigPlaylistDataPath.Value;
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            if (!File.Exists(filePath))
            {
                JsonObject defaultFile = GetDefaultJson();

                JsonSerializerOptions options = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                };

                File.WriteAllText(Plugin.Instance.ConfigPlaylistDataPath.Value, defaultFile.ToJsonString(options));
            }
        }

        public static void OutputPlaylistData(List<PlaylistData> playlists)
        {
            var node = GetDefaultJson();
            var data = node["Data"].AsArray();
            for (int i = 0; i < playlists.Count; i++)
            {
                var playlist = playlists[i];
                JsonObject obj = new JsonObject()
                {
                    ["Enabled"] = playlist.IsEnabled,
                    ["CategoryPanelData"] = new JsonObject()
                    {
                        ["Name"] = playlist.Name,
                        ["BgColor"] = "#" + ColorUtility.ToHtmlStringRGB(playlist.BgColor),
                        ["FrameType"] = playlist.FrameType.ToString(),
                        ["FrameColor"] = "#" + ColorUtility.ToHtmlStringRGB(playlist.FrameColor),
                    },
                    ["Difficulties"] = new JsonArray(),
                    ["Stars"] = new JsonArray(),
                    ["Crowns"] = new JsonArray(),
                    ["Genres"] = new JsonArray(),
                    ["Sorting"] = new JsonArray(),
                };

                for (int j = 0; j < playlist.Difficulties.Count; j++)
                {
                    var value = playlist.Difficulties[j];
                    if (value == EnsoData.EnsoLevelType.Mania)
                    {
                        obj["Difficulties"].AsArray().Add("Oni");
                    }
                    else
                    {
                        obj["Difficulties"].AsArray().Add(value.ToString());
                    }
                }
                for (int j = 0; j < playlist.Stars.Count; j++)
                {
                    var value = playlist.Stars[j];
                    obj["Stars"].AsArray().Add(value);
                }
                for (int j = 0; j < playlist.Crowns.Count; j++)
                {
                    var value = playlist.Crowns[j];
                    obj["Crowns"].AsArray().Add(value.ToString());
                }
                for (int j = 0; j < playlist.Genres.Count; j++)
                {
                    var value = playlist.Genres[j];
                    obj["Genres"].AsArray().Add(value.ToString());
                }
                for (int j = 0; j < playlist.SortTypes.Count; j++)
                {
                    var value = playlist.SortTypes[j];
                    obj["Sorting"].AsArray().Add(value.ToString());
                }

                data.Add(obj);
            }


            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            File.Copy(Plugin.Instance.ConfigPlaylistDataPath.Value, Plugin.Instance.ConfigPlaylistDataPath.Value.Replace(".json", "_bak.json"), true);
            File.WriteAllText(Plugin.Instance.ConfigPlaylistDataPath.Value, node.ToJsonString(options));
        }

        static JsonObject GetDefaultJson()
        {
            return new JsonObject()
            {
                ["Example"] = new JsonObject()
                {

                },
                ["Template"] = new JsonObject()
                {
                    ["Enabled"] = true,
                    ["CategoryPanelData"] = new JsonObject()
                    {
                        ["Name"] = "",
                        ["BgColor"] = "#FFFFFF",
                        ["FrameType"] = "SingleColor",
                        ["FrameColor"] = "#FFFFFF",
                    },
                    ["Difficulties"] = new JsonArray(),
                    ["Stars"] = new JsonArray(),
                    ["Crowns"] = new JsonArray(),
                    ["Genres"] = new JsonArray(),
                    ["Sorting"] = new JsonArray(),
                },
                ["Data"] = new JsonArray(),
            };
        }
    }
}
