using System;
using System.Collections.Generic;
using UnityEngine;

namespace DockIQ.Board
{
    public sealed class GridBoard
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public float CellSize { get; private set; }
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
            var liftsByPair = new Dictionary<int, List<Vector2Int>>();

            for (int y = 0; y < Height; y++)
            {
                int rowIndex = Height - 1 - y;
                string row = rows[rowIndex];
                for (int x = 0; x < Width; x++)
                {
                    _cells[x, y] = ParseCell(row[x]);
                    if (_cells[x, y].IsLift)
                    {
                        int pair = _cells[x, y].LiftPairId;
                        if (!liftsByPair.TryGetValue(pair, out var list))
                        {
                            list = new List<Vector2Int>();
                            liftsByPair[pair] = list;
                        }

                        list.Add(new Vector2Int(x, y));
                    }
                }
            }

            foreach (var kv in liftsByPair)
            {
                var pads = kv.Value;
                if (pads.Count != 2)
                {
                    Debug.LogWarning($"Lift pair {kv.Key} has {pads.Count} pads (need 2)");
                    continue;
                }

                LinkLift(pads[0], pads[1]);
                LinkLift(pads[1], pads[0]);
            }
        }

        private void LinkLift(Vector2Int from, Vector2Int to)
        {
            var cell = Get(from);
            cell.LiftTarget = to;
            if (cell.Device is LiftDevice lift)
                lift.LinkedCell = to;
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

        /// <summary>
        /// Advance a robot one cell along tracks. Updates facing when redirected.
        /// </summary>
        /// <param name="suppressLift">If true, treat current lift pad as a normal tile (no teleport).</param>
        public bool TryStep(Vector2Int from, Dir facing, out Vector2Int next, out Dir newFacing,
            bool suppressLift = false)
        {
            next = from;
            newFacing = facing;

            if (!InBounds(from))
                return false;

            var data = Get(from);
            if (!data.IsTraversable || data.IsDock)
                return false;

            // Lift: teleport to linked pad, then immediately leave that pad
            // in the travel direction so we never bounce A↔a forever.
            if (data.IsLift && !suppressLift)
                return TryExitLift(data.LiftTarget, facing, out next, out newFacing);

            if (!data.TryResolveExit(facing, out Dir exit))
                return false; // e.g. closed bridge

            // On a lift with suppress: leave via facing, never re-teleport.
            newFacing = exit;
            next = from + DirUtil.ToOffset(exit);
            return CanEnter(next);
        }

        /// <summary>
        /// Arrive on a lift pad and continue off it (pass-through transfer).
        /// </summary>
        /// <returns>
        /// True if transfer resolved. <paramref name="landedOnPad"/> is true when
        /// the robot had to stop on the arrival pad (no valid exit).
        /// </returns>
        private bool TryExitLift(Vector2Int pad, Dir facing, out Vector2Int next, out Dir newFacing)
        {
            next = pad;
            newFacing = facing;

            if (!InBounds(pad) || !Get(pad).IsTraversable)
                return false;

            // Leave along travel facing — do not trigger the destination lift's teleport.
            Dir exit = facing;
            Vector2Int after = pad + DirUtil.ToOffset(exit);
            if (CanEnter(after) && !Get(after).IsLift)
            {
                next = after;
                newFacing = exit;
                return true;
            }

            // Stay on arrival pad; caller should suppress lift next tick.
            next = pad;
            newFacing = facing;
            return true;
        }

        private bool CanEnter(Vector2Int cell)
        {
            if (!InBounds(cell))
                return false;

            var dest = Get(cell);
            if (!dest.IsTraversable)
                return false;

            if (dest.Type == CellType.Bridge &&
                dest.Device is BridgeDevice bridge &&
                !bridge.IsOpen)
                return false;

            return true;
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
                    cell.Type = CellType.Track;
                    cell.Facing = DirUtil.FromChar(c);
                    break;

                case '+':
                    cell.Type = CellType.Switch;
                    cell.Facing = Dir.East;
                    cell.Device = new SwitchDevice(Dir.East);
                    break;

                case 'R':
                case 'r':
                    cell.Type = CellType.Rotator;
                    cell.Device = new RotatorDevice(0);
                    break;

                case 'B':
                    cell.Type = CellType.Bridge;
                    cell.Device = new BridgeDevice(startOpen: false);
                    break;

                case 'A':
                case 'a':
                    cell.Type = CellType.Lift;
                    cell.LiftPairId = 0;
                    cell.Device = new LiftDevice();
                    break;

                case 'C':
                case 'c':
                    cell.Type = CellType.Lift;
                    cell.LiftPairId = 1;
                    cell.Device = new LiftDevice();
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
