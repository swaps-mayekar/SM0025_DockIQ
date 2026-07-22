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
        public int TargetDockId = 1;
        public float TimeLimit = 40f;
        public float TickSeconds = 0.45f;
        public string[] Rows;

        /// <summary>Grid cell where VIP starts (x,y with y=0 at bottom).</summary>
        public Vector2Int VipStart;

        public Vector2Int[] DecoyStarts = Array.Empty<Vector2Int>();

        public int VipCount = 1;
        public MechanicsMask Mechanics = MechanicsMask.Switches;
    }
}
