using UnityEngine;

namespace DockIQ.Board
{
    /// <summary>
    /// Classic 2:1 isometric projection for 2D sprites (no 3D meshes).
    /// Grid +X = East (screen down-right), +Y = North (screen up-right).
    /// </summary>
    public static class IsoMath
    {
        /// <summary>Horizontal span of one diamond tile.</summary>
        public const float TileWidth = 1.0f;

        /// <summary>Vertical span of one diamond tile (half of width for 2:1 iso).</summary>
        public const float TileHeight = 0.5f;

        public static Vector3 CellToWorld(int x, int y, float z = 0f)
        {
            float wx = (x - y) * (TileWidth * 0.5f);
            float wy = (x + y) * (TileHeight * 0.5f);
            return new Vector3(wx, wy, z);
        }

        public static Vector3 CellToWorld(Vector2Int cell, float z = 0f) =>
            CellToWorld(cell.x, cell.y, z);

        public static Vector2Int WorldToCell(Vector3 world)
        {
            float a = world.x / (TileWidth * 0.5f);
            float b = world.y / (TileHeight * 0.5f);
            int x = Mathf.RoundToInt((a + b) * 0.5f);
            int y = Mathf.RoundToInt((b - a) * 0.5f);
            return new Vector2Int(x, y);
        }

        /// <summary>Screen-space unit step along a grid direction (for arrow aiming).</summary>
        public static Vector2 DirToScreen(Dir dir)
        {
            Vector2Int o = DirUtil.ToOffset(dir);
            Vector3 delta = CellToWorld(o.x, o.y) - CellToWorld(0, 0);
            return new Vector2(delta.x, delta.y).normalized;
        }

        /// <summary>Z rotation for a sprite that points "up" (+Y) by default.</summary>
        public static float DirToZDegrees(Dir dir)
        {
            Vector2 v = DirToScreen(dir);
            return Mathf.Atan2(v.x, v.y) * Mathf.Rad2Deg;
        }

        /// <summary>Painter sorting: lower on screen (smaller x+y) draws in front.</summary>
        public static int DepthOrder(int x, int y, int layer = 0) =>
            -(x + y) * 10 + layer;

        public static int DepthOrder(Vector2Int cell, int layer = 0) =>
            DepthOrder(cell.x, cell.y, layer);

        /// <summary>World AABB of the board in isometric space (for camera fit).</summary>
        public static void GetBounds(int width, int height, out Vector2 min, out Vector2 max)
        {
            Vector2 bMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 bMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

            int[,] corners =
            {
                { 0, 0 },
                { width - 1, 0 },
                { 0, height - 1 },
                { width - 1, height - 1 }
            };

            for (int i = 0; i < 4; i++)
            {
                Vector3 p = CellToWorld(corners[i, 0], corners[i, 1]);
                var v = new Vector2(p.x, p.y);
                bMin = Vector2.Min(bMin, v);
                bMax = Vector2.Max(bMax, v);
            }

            // Expand by half-tile so diamonds aren't clipped
            var pad = new Vector2(TileWidth * 0.5f, TileHeight * 0.5f);
            min = bMin - pad;
            max = bMax + pad;
        }
    }
}
