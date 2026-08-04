using System;
using DockIQ.Board;
using UnityEngine;

namespace DockIQ.Levels
{
    [Serializable]
    public sealed class LevelDef
    {
        public int Id;
        public string Title;
        public string RequestText;
        public string RobotCallsign = "#A13";
        public string DockName = "Dock 1";
        public int TargetDockId = 1;
        public float TimeLimit = 40f;
        public float TickSeconds = 0.45f;

        /// <summary>Legacy single-layer rows. Used when <see cref="Layers"/> is null.</summary>
        public string[] Rows;

        /// <summary>Multi-layer ASCII grids. Layer 0 = ground. Prefer this over Rows.</summary>
        public string[][] Layers;

        /// <summary>Path-cycled movables (rotators, reflectors, obstacles).</summary>
        public MovableDef[] Movables = Array.Empty<MovableDef>();

        /// <summary>Grid cell where the rescue robot starts (y=0 at bottom).</summary>
        public Vector2Int RobotStart;

        /// <summary>Floor layer for the rescue robot (default ground).</summary>
        public int RobotLayer;

        /// <summary>Initial travel facing for the rescue robot.</summary>
        public Dir RobotFacing = Dir.East;

        public Vector2Int[] DecoyStarts = Array.Empty<Vector2Int>();
        public Dir[] DecoyFacings = Array.Empty<Dir>();
        public int[] DecoyLayers;

        public int VipCount = 1;
        public MechanicsMask Mechanics = MechanicsMask.Switches;

        [NonSerialized] private bool _openingRunwayApplied;

        /// <summary>
        /// Inserts normal track cells immediately after the rescue spawn and shifts the
        /// remainder of the level consistently. This guarantees players can observe the
        /// parcel moving before it reaches the first mechanic.
        /// </summary>
        public void EnsureOpeningRunway(int normalTileCount = 5)
        {
            if (_openingRunwayApplied || normalTileCount <= 0)
                return;
            _openingRunwayApplied = true;

            string[][] source = ResolveLayers();
            int insertAfterX = RobotStart.x;
            int rescueRow = source[0].Length - 1 - RobotStart.y;
            string runway = new string('>', normalTileCount);
            string emptyGap = new string('.', normalTileCount);
            var expanded = new string[source.Length][];

            for (int layer = 0; layer < source.Length; layer++)
            {
                expanded[layer] = new string[source[layer].Length];
                for (int row = 0; row < source[layer].Length; row++)
                {
                    string original = source[layer][row];
                    int insertIndex = Mathf.Clamp(insertAfterX + 1, 0, original.Length);
                    bool rescuePath = layer == RobotLayer && row == rescueRow;
                    bool crossesInsertion = insertIndex > 0 && insertIndex < original.Length &&
                                            IsRouteGlyph(original[insertIndex - 1]) &&
                                            IsRouteGlyph(original[insertIndex]);
                    string inserted = rescuePath || crossesInsertion ? runway : emptyGap;
                    expanded[layer][row] = original.Insert(insertIndex, inserted);
                }
            }

            Layers = expanded;
            Rows = null;

            if (DecoyStarts != null)
            {
                for (int i = 0; i < DecoyStarts.Length; i++)
                {
                    Vector2Int start = DecoyStarts[i];
                    if (start.x > insertAfterX)
                        DecoyStarts[i] = new Vector2Int(start.x + normalTileCount, start.y);
                }
            }

            if (Movables != null)
            {
                for (int i = 0; i < Movables.Length; i++)
                {
                    Vector3Int[] path = Movables[i]?.Path;
                    if (path == null)
                        continue;
                    for (int p = 0; p < path.Length; p++)
                    {
                        Vector3Int point = path[p];
                        if (point.x > insertAfterX)
                            path[p] = new Vector3Int(point.x + normalTileCount, point.y, point.z);
                    }
                }
            }

            // Preserve the original decision-time budget after adding five travel ticks.
            TimeLimit += normalTileCount * TickSeconds;
        }

        private static bool IsRouteGlyph(char c) =>
            c == '^' || c == '>' || c == 'v' || c == '<' ||
            c == '+' || c == 'R' || c == 'B' ||
            c == 'A' || c == 'a' || c == 'C' || c == 'c' ||
            c == 'E' || c == 'e' || c == 'M' || c == 'X' ||
            c == 'O' || c == 'S' || c == 's' ||
            (c >= '1' && c <= '9');

        public string[][] ResolveLayers()
        {
            if (Layers != null && Layers.Length > 0)
                return Layers;
            if (Rows != null && Rows.Length > 0)
                return new[] { Rows };
            throw new InvalidOperationException($"Level {Id} has no Layers or Rows");
        }

        public CellCoord RobotCoord => CellCoord.From(RobotStart, RobotLayer);

        public CellCoord DecoyCoord(int i)
        {
            int layer = DecoyLayers != null && i < DecoyLayers.Length ? DecoyLayers[i] : 0;
            return CellCoord.From(DecoyStarts[i], layer);
        }
    }
}
