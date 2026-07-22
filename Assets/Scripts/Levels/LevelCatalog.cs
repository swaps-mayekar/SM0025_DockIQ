using DockIQ.Board;
using UnityEngine;

namespace DockIQ.Levels
{
    public static class LevelCatalog
    {
        private static LevelDef[] _levels;

        public static int Count => GetAll().Length;

        public static LevelDef Get(int id)
        {
            var all = GetAll();
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].Id == id)
                    return all[i];
            }

            return all[0];
        }

        public static LevelDef[] GetAll() => _levels ??= Build();

        private static LevelDef[] Build()
        {
            return new[]
            {
                // Level 1 — one VIP, switches must be rotated (defaults face east into empty)
                new LevelDef
                {
                    Id = 1,
                    Title = "First Routing",
                    RequestText = "Urgent Medical Supply → Dock 1",
                    TargetDockId = 1,
                    TimeLimit = 50f,
                    TickSeconds = 0.5f,
                    VipStart = new Vector2Int(1, 3),
                    Mechanics = MechanicsMask.Switches,
                    Rows = new[]
                    {
                        "........",
                        ".S>+....",
                        "...v....",
                        "...+>1..",
                        "........"
                    }
                },

                // Level 2 — wrong dock; must rotate switches south then east to Dock 2
                new LevelDef
                {
                    Id = 2,
                    Title = "Wrong Bay",
                    RequestText = "Priority Vaccine Pack → Dock 2",
                    TargetDockId = 2,
                    TimeLimit = 45f,
                    TickSeconds = 0.45f,
                    VipStart = new Vector2Int(1, 5),
                    Mechanics = MechanicsMask.Switches,
                    Rows = new[]
                    {
                        "...........",
                        ".S>+>+.....",
                        ".....v.....",
                        ".....+>1...",
                        ".....v.....",
                        ".....+>>>2.",
                        "..........."
                    }
                },

                // Level 3 — decoy parcels on a parallel belt
                new LevelDef
                {
                    Id = 3,
                    Title = "Busy Belt",
                    RequestText = "Chicago Express Crate → Dock 1",
                    TargetDockId = 1,
                    TimeLimit = 40f,
                    TickSeconds = 0.42f,
                    VipStart = new Vector2Int(1, 3),
                    DecoyStarts = new[]
                    {
                        new Vector2Int(1, 1),
                        new Vector2Int(3, 1)
                    },
                    Mechanics = MechanicsMask.Switches,
                    Rows = new[]
                    {
                        "...........",
                        ".S>+>+>+>1.",
                        "...........",
                        ".>>>>>>>...",
                        "..........."
                    }
                },

                // Level 4 — tighter timer + splitter (must tap splitter south; default east is dead-end)
                new LevelDef
                {
                    Id = 4,
                    Title = "Truck Departing",
                    RequestText = "Organ Cooler → Dock 1",
                    TargetDockId = 1,
                    TimeLimit = 28f,
                    TickSeconds = 0.38f,
                    VipStart = new Vector2Int(0, 4),
                    DecoyStarts = new[] { new Vector2Int(5, 2) },
                    Mechanics = MechanicsMask.Switches | MechanicsMask.Splitters,
                    Rows = new[]
                    {
                        "...........",
                        "S>+>*......",
                        "....v......",
                        "....+>>>1..",
                        "...........",
                        "..........."
                    }
                },

                // Level 5 — dual docks, express pace
                new LevelDef
                {
                    Id = 5,
                    Title = "Dual Dock Chaos",
                    RequestText = "VIP Art Shipment → Dock 2",
                    TargetDockId = 2,
                    TimeLimit = 32f,
                    TickSeconds = 0.36f,
                    VipStart = new Vector2Int(1, 4),
                    DecoyStarts = new[]
                    {
                        new Vector2Int(1, 2),
                        new Vector2Int(2, 2)
                    },
                    Mechanics = MechanicsMask.Switches | MechanicsMask.Splitters,
                    Rows = new[]
                    {
                        "...........",
                        ".S>+>*.....",
                        ".....v.....",
                        ".>>>1+>>>2.",
                        "...........",
                        "..........."
                    }
                }
            };
        }
    }
}
