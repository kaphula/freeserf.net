﻿/*
 * AIStateDestroyUselessBuildings.cs - AI state to destroy useless buildings
 *
 * Copyright (C) 2019  Robert Schneckenhaus <robert.schneckenhaus@web.de>
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

using System.Linq;

namespace Freeserf.AIStates
{
    // Removes stonecutters or mines that can no longer quarry resources.
    // For mines this is called directly with a specific building index.
    // Otherwise it is a cyclic check and we check all stonecutters.
    class AIStateDestroyUselessBuildings : AIState
    {
        uint buildingIndex = uint.MaxValue;

        public AIStateDestroyUselessBuildings()
        {

        }

        public AIStateDestroyUselessBuildings(uint buildingIndex)
        {
            this.buildingIndex = buildingIndex;
        }

        public override void Update(AI ai, Game game, Player player, PlayerInfo playerInfo, int tick)
        {
            if (buildingIndex != uint.MaxValue)
            {
                // a specific mine
                var mine = game.GetBuilding(buildingIndex);

                if (mine != null)
                {
                    game.DemolishBuilding(mine.Position, player);
                }
            }
            else
            {
                var stonecutters = game.GetPlayerBuildings(player, Building.Type.Stonecutter).ToList(); // use ToList as we might change the collection below

                foreach (var stonecutter in stonecutters)
                {
                    if (game.Map.FindInArea(stonecutter.Position, 8, FindStone, 1).Count == 0)
                    {
                        game.DemolishBuilding(stonecutter.Position, player);
                    }
                }
            }

            Kill(ai);
        }

        static Map.FindData FindStone(Map map, uint pos)
        {
            return new Map.FindData()
            {
                Success = map.GetObject(pos) >= Map.Object.Stone0 && map.GetObject(pos) <= Map.Object.Stone7
            };
        }
    }
}