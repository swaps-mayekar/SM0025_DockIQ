using System;
using UnityEngine;

namespace DockIQ.Board
{
    /// <summary>Grid position including floor layer (0 = ground).</summary>
    [Serializable]
    public struct CellCoord : IEquatable<CellCoord>
    {
        public int X;
        public int Y;
        public int Layer;

        public CellCoord(int x, int y, int layer = 0)
        {
            X = x;
            Y = y;
            Layer = layer;
        }

        public Vector2Int XY => new Vector2Int(X, Y);

        public static CellCoord From(Vector2Int p, int layer = 0) => new CellCoord(p.x, p.y, layer);

        public static CellCoord From(Vector3Int p) => new CellCoord(p.x, p.y, p.z);

        public Vector3Int ToVector3Int() => new Vector3Int(X, Y, Layer);

        public CellCoord WithOffset(Vector2Int offset) => new CellCoord(X + offset.x, Y + offset.y, Layer);

        public bool Equals(CellCoord other) => X == other.X && Y == other.Y && Layer == other.Layer;

        public override bool Equals(object obj) => obj is CellCoord other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + X;
                hash = hash * 31 + Y;
                hash = hash * 31 + Layer;
                return hash;
            }
        }

        public override string ToString() => $"({X},{Y},L{Layer})";

        public static bool operator ==(CellCoord a, CellCoord b) => a.Equals(b);

        public static bool operator !=(CellCoord a, CellCoord b) => !a.Equals(b);
    }
}
