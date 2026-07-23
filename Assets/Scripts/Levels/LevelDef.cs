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
