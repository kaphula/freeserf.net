﻿/*
 * Data.cs - Game resources file functions
 *
 * Copyright (C) 2014-2017  Wicked_Digger <wicked_digger@mail.ru>
 * Copyright (C) 2018       Robert Schneckenhaus <robert.schneckenhaus@web.de>
 *
 * This file is part of freeserf.net. freeserf.net is based on freeserf.
 *
 * freeserf.net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * freeserf.net is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with freeserf.net. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Freeserf
{
    public struct DataResource
    {
        public Data.Resource Asset;
        public Data.Type Type;
        public uint Count;
        public string Name;

        internal DataResource(Data.Resource asset, Data.Type type, uint count, string name)
        {
            Asset = asset;
            Type = type;
            Count = count;
            Name = name;
        }
    }

    public class Data
    {
        public static readonly DataResource[] dataResources = new DataResource[] {
            new DataResource ( Data.Resource.None,         Data.Type.Unknown,   0,   "error"         ),
            new DataResource ( Data.Resource.ArtLandscape, Data.Type.Sprite,    1,   "art_landscape" ),
            new DataResource ( Data.Resource.Animation,    Data.Type.Animation, 200, "animation"     ),
            new DataResource ( Data.Resource.SerfShadow,   Data.Type.Sprite,    1,   "serf_shadow"   ),
            new DataResource ( Data.Resource.DottedLines,  Data.Type.Sprite,    7,   "dotted_lines"  ),
            new DataResource ( Data.Resource.ArtFlag,      Data.Type.Sprite,    7,   "art_flag"      ),
            new DataResource ( Data.Resource.ArtBox,       Data.Type.Sprite,    14,  "art_box"       ),
            new DataResource ( Data.Resource.CreditsBg,    Data.Type.Sprite,    1,   "credits_bg"    ),
            new DataResource ( Data.Resource.Logo,         Data.Type.Sprite,    1,   "logo"          ),
            new DataResource ( Data.Resource.Symbol,       Data.Type.Sprite,    16,  "symbol"        ),
            new DataResource ( Data.Resource.MapMaskUp,    Data.Type.Sprite,    81,  "map_mask_up"   ),
            new DataResource ( Data.Resource.MapMaskDown,  Data.Type.Sprite,    81,  "map_mask_down" ),
            new DataResource ( Data.Resource.PathMask,     Data.Type.Sprite,    27,  "path_mask"     ),
            new DataResource ( Data.Resource.MapGround,    Data.Type.Sprite,    33,  "map_ground"    ),
            new DataResource ( Data.Resource.PathGround,   Data.Type.Sprite,    10,  "path_ground"   ),
            new DataResource ( Data.Resource.GameObject,   Data.Type.Sprite,    279, "game_object"   ),
            new DataResource ( Data.Resource.FrameTop,     Data.Type.Sprite,    4,   "frame_top"     ),
            new DataResource ( Data.Resource.MapBorder,    Data.Type.Sprite,    10,  "map_border"    ),
            new DataResource ( Data.Resource.MapWaves,     Data.Type.Sprite,    16,  "map_waves"     ),
            new DataResource ( Data.Resource.FramePopup,   Data.Type.Sprite,    4,   "frame_popup"   ),
            new DataResource ( Data.Resource.Indicator,    Data.Type.Sprite,    8,   "indicator"     ),
            new DataResource ( Data.Resource.Font,         Data.Type.Sprite,    44,  "font"          ),
            new DataResource ( Data.Resource.FontShadow,   Data.Type.Sprite,    44,  "font_shadow"   ),
            new DataResource ( Data.Resource.Icon,         Data.Type.Sprite,    318, "icon"          ),
            new DataResource ( Data.Resource.MapObject,    Data.Type.Sprite,    194, "map_object"    ),
            new DataResource ( Data.Resource.MapShadow,    Data.Type.Sprite,    194, "map_shadow"    ),
            new DataResource ( Data.Resource.PanelButton,  Data.Type.Sprite,    25,  "panel_button"  ),
            new DataResource ( Data.Resource.FrameBottom,  Data.Type.Sprite,    26,  "frame_bottom"  ),
            new DataResource ( Data.Resource.SerfTorso,    Data.Type.Sprite,    541, "serf_torso"    ),
            new DataResource ( Data.Resource.SerfHead,     Data.Type.Sprite,    630, "serf_head"     ),
            new DataResource ( Data.Resource.FrameSplit,   Data.Type.Sprite,    3,   "frame_split"   ),
            new DataResource ( Data.Resource.Sound,        Data.Type.Sound,     90,  "sound"         ),
            new DataResource ( Data.Resource.Music,        Data.Type.Music,     7,   "music"         ),
            new DataResource ( Data.Resource.Cursor,       Data.Type.Sprite,    1,   "cursor"        )
        };

        public enum Type
        {
            Unknown,
            Sprite,
            Animation,
            Sound,
            Music
        }

        public enum Resource
        {
            None,
            ArtLandscape,
            Animation,
            SerfShadow,
            DottedLines,
            ArtFlag,
            ArtBox,
            CreditsBg,
            Logo,
            Symbol,
            MapMaskUp,
            MapMaskDown,
            PathMask,
            MapGround,
            PathGround,
            GameObject,
            FrameTop,
            MapBorder,
            MapWaves,
            FramePopup,
            Indicator,
            Font,
            FontShadow,
            Icon,
            MapObject,
            MapShadow,
            PanelButton,
            FrameBottom,
            SerfTorso,
            SerfHead,
            FrameSplit,
            Sound,
            Music,
            Cursor
        }

        protected DataSource dataSource = null;
        static Data instance = new Data();

        protected Data()
        {

        }

        public static Data GetInstance()
        {
            return instance;
        }

        // Try to load data file from given path or standard paths.
        //
        // Return true if successful. Standard paths will be searched only if the
        // given path is an empty string.
        public bool Load(string path)
        {
            // If it is possible, prefer DOS game data.
            List<Func<string, DataSource>> sourceFactories = new List<Func<string, DataSource>>();

            sourceFactories.Add((string p) =>
            {
                return new DataSourceCustom(path);
            });
            sourceFactories.Add((string p) =>
            {
                return new DataSourceDOS(path);
            });
            sourceFactories.Add((string p) =>
            {
                return new DataSourceAmiga(path);
            });

            List<string> searchPaths = new List<string>();

            if (string.IsNullOrWhiteSpace(path))
            {
                searchPaths = GetStandardSearchPaths();
            }
            else
            {
                searchPaths.Add(path);
            }

            // Use each data source to try to find the data files in the search paths.
            foreach (var factory in sourceFactories)
            {
                foreach (var searchPath in searchPaths)
                {
                    DataSource source = factory(path);

                    if (source.Check())
                    {
                        Log.Info.Write("data", $"Game data found in '{source.Path}'...");

                        if (source.Load())
                        {
                            dataSource = source;
                            break;
                        }
                    }
                }

                if (dataSource != null)
                {
                    break;
                }
            }

            return dataSource != null;
        }

        public DataSource GetDataSource()
        {
            return dataSource;
        }

        public static Type GetAssetType(Resource asset)
        {
            return dataResources[(int)asset].Type;
        }
        public static uint GetAssetCount(Resource asset)
        {
            return dataResources[(int)asset].Count;
        }

        public static string GetAssetName(Resource asset)
        {
            return dataResources[(int)asset].Name;
        }

        // Return standard game data search paths for current platform.
        protected List<string> GetStandardSearchPaths()
        {
            // Data files are searched for in some common directories, some of which are
            // specific to the platform we're running on.
            //
            // On platforms where the XDG specification applies, the data file is
            // searched for in the directories specified by the
            // XDG Base Directory Specification
            // <http://standards.freedesktop.org/basedir-spec/basedir-spec-latest.html>.
            //
            // On Windows platforms the %localappdata% is used in place of
            // XDG_DATA_HOME.

            List<string> paths = new List<string>();

            // Add path where base is obtained from an environment variable and can
            // be null or empty.
            Action<string, string> addEnvPath = (string root, string suffix) =>
            {
                if (root != null)
                {
                    if (!string.IsNullOrWhiteSpace(root))
                        paths.Add(root + "/" + suffix);
                }
            };

            // Look in current directory
            paths.Add(".");

            // Look in data directories under the home directory
            addEnvPath(Environment.GetEnvironmentVariable("XDG_DATA_HOME"), "freeserf");
            addEnvPath(Environment.GetEnvironmentVariable("HOME"), ".local/share/freeserf");
            addEnvPath(Environment.GetEnvironmentVariable("HOME"), ".local/share/freeserf/custom");

            if (Environment.OSVersion.Platform == PlatformID.Win32Windows)
            {
                // Look in the same directory as the freeserf.exe app.
                addEnvPath(Assembly.GetEntryAssembly().Location, "../");

                // Look in windows XDG_DATA_HOME equivalents.
                addEnvPath(Environment.GetEnvironmentVariable("userprofile"), ".local/share/freeserf");
                addEnvPath(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "freeserf");
            }
            else
            {
                paths.Add("/usr/local/share/freeserf");
                paths.Add("/usr/share/freeserf");
            }

            return paths;
        }
    }
}
