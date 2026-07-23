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
        public string[] Rows;

        /// <summary>Grid cell where the rescue robot starts (y=0 at bottom).</summary>
        public Vector2Int RobotStart;

        /// <summary>Initial travel facing for the rescue robot.</summary>
        public Dir RobotFacing = Dir.East;

        public Vector2Int[] DecoyStarts = Array.Empty<Vector2Int>();
        public Dir[] DecoyFacings = Array.Empty<Dir>();

        public int VipCount = 1;
        public MechanicsMask Mechanics = MechanicsMask.Switches;
    }
}
