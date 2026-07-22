using System;
using UnityEngine;

namespace DockIQ.Board
{
    public sealed class GridBoard
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public float CellSize { get; private set; }

        /// <summary>World offset so the isometric board is centered at origin.</summary>
        public Vector3 Origin { get; private set; }

        private CellData[,] _cells;

        public CellData Get(int x, int y) => _cells[x, y];

        public CellData Get(Vector2Int p) => _cells[p.x, p.y];

        public bool InBounds(int x, int y) =>
            x >= 0 && y >= 0 && x < Width && y < Height;

        public bool InBounds(Vector2Int p) => InBounds(p.x, p.y);

        public void Build(string[] rows, float cellSize = 1f)
        {
            if (rows == null || rows.Length == 0)
                throw new ArgumentException("Level rows required");

            Height = rows.Length;
            Width = rows[0].Length;
            for (int r = 0; r < rows.Length; r++)
            {
                if (rows[r].Length != Width)
                    throw new ArgumentException($"Row {r} width mismatch");
            }

            CellSize = cellSize;

            IsoMath.GetBounds(Width, Height, out Vector2 min, out Vector2 max);
            Vector2 center = (min + max) * 0.5f;
            Origin = new Vector3(-center.x, -center.y, 0f);

            _cells = new CellData[Width, Height];
            for (int y = 0; y < Height; y++)
            {
                int rowIndex = Height - 1 - y;
                string row = rows[rowIndex];
                for (int x = 0; x < Width; x++)
                    _cells[x, y] = ParseCell(row[x]);
            }
        }

        public Vector3 CellToWorld(Vector2Int cell, float z = 0f)
        {
            Vector3 local = IsoMath.CellToWorld(cell.x, cell.y, z);
            return Origin + local;
        }

        public bool TryWorldToCell(Vector3 world, out Vector2Int cell)
        {
            Vector3 local = world - Origin;
            cell = IsoMath.WorldToCell(local);
            return InBounds(cell);
        }

        public Vector2Int? Step(Vector2Int from)
        {
            if (!InBounds(from))
                return null;

            var data = Get(from);
            if (!data.IsTraversable || data.IsDock)
                return null;

            Vector2Int next = from + DirUtil.ToOffset(data.GetExitDir());
            if (!InBounds(next) || !Get(next).IsTraversable)
                return null;

            return next;
        }

        private static CellData ParseCell(char c)
        {
            var cell = new CellData { Type = CellType.Empty, Facing = Dir.East };

            switch (c)
            {
                case '.':
                case ' ':
                    break;

                case '^':
                case '>':
                case 'v':
                case '<':
                    cell.Type = CellType.Belt;
                    cell.Facing = DirUtil.FromChar(c);
                    break;

                case '+':
                    cell.Type = CellType.Switch;
                    cell.Facing = Dir.East;
                    cell.Device = new SwitchDevice(Dir.East);
                    break;

                case '*':
                    cell.Type = CellType.Splitter;
                    cell.Facing = Dir.East;
                    cell.Device = new SplitterDevice(Dir.East);
                    break;

                case 'S':
                case 's':
                    cell.Type = CellType.Spawn;
                    cell.Facing = Dir.East;
                    break;

                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    cell.Type = CellType.Dock;
                    cell.DockId = c - '0';
                    break;

                default:
                    Debug.LogWarning($"Unknown cell char '{c}', treating as empty");
                    break;
            }

            return cell;
        }
    }
}
