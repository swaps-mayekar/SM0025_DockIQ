using System;

namespace DockIQ.Levels
{
    /// <summary>Path-cycled gadget placed on track cells (Chip-style movable piece).</summary>
    [Serializable]
    public sealed class MovableDef
    {
        /// <summary>'R' moving rotator, 'm' moving reflector, 'O' sliding obstacle.</summary>
        public char Kind = 'O';

        /// <summary>Path slots as (x, y, layer). Length must be >= 2 (or 1 for rotate-only rotator).</summary>
        public UnityEngine.Vector3Int[] Path;

        public int StartIndex;

        /// <summary>Initial rotator mode when Kind == 'R'.</summary>
        public int RotatorMode;
    }
}
