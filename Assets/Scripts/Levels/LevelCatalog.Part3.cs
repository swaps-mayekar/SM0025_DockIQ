using System;
using DockIQ.Board;
using UnityEngine;

namespace DockIQ.Levels
{
    public static partial class LevelCatalog
    {
        private static void AddRange25to38(Action<LevelDef> add)
        {
            // ——— 25–30: liftable obstacles ———
            add(new LevelDef
            {
                Id = 25, Title = "Raise the Gate",
                RequestText = "Surgical implant trays → Dock 1",
                RobotCallsign = "#I25", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 36f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Liftables,
                Layers = L0(
                    "...........",
                    ".S>>X>>1...",
                    "...........",
                    "...........",
                    "...........")
            });

            add(new LevelDef
            {
                Id = 26, Title = "Lift & Switch",
                RequestText = "Red trauma kit → Dock 1",
                RobotCallsign = "#I26", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 38f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Switches | MechanicsMask.Liftables,
                Layers = L0(
                    "...........",
                    ".S>+>X>+>1.",
                    "...........",
                    "...........",
                    "...........")
            });

            add(new LevelDef
            {
                Id = 27, Title = "Two Hoists",
                RequestText = "Pressurized oxygen canisters → Dock 1",
                RobotCallsign = "#I27", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 40f, TickSeconds = 0.38f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Liftables | MechanicsMask.Switches,
                Layers = L0(
                    "...........",
                    ".S>X>+>X>1.",
                    "...........",
                    "...........",
                    "...........")
            });

            add(new LevelDef
            {
                Id = 28, Title = "Timed Hoist",
                RequestText = "Heart-lung module → Dock 1",
                RobotCallsign = "#I28", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 32f, TickSeconds = 0.36f,
                RobotStart = new Vector2Int(0, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Bridges | MechanicsMask.Liftables,
                Layers = L0(
                    "...........",
                    "S>>B>X>>1..",
                    "...........",
                    "...........",
                    "...........")
            });

            add(new LevelDef
            {
                Id = 29, Title = "Wrong Dock Lift",
                RequestText = "Yellow quarantine sample → Dock 2",
                RobotCallsign = "#I29", DockName = "Dock 2", TargetDockId = 2,
                TimeLimit = 40f, TickSeconds = 0.38f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Switches | MechanicsMask.Liftables,
                Layers = L0(
                    "...........",
                    ".S>+>X>1...",
                    "...v.......",
                    "...+>>>2...",
                    "...........")
            });

            add(new LevelDef
            {
                Id = 30, Title = "Hoist & Bounce",
                RequestText = "Laser-alignment tools → Dock 1",
                RobotCallsign = "#I30", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 40f, TickSeconds = 0.38f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Liftables | MechanicsMask.Reflectors | MechanicsMask.Switches,
                Layers = L0(
                    "...........",
                    ".S>+>X>M...",
                    "...v.......",
                    "...+>>>1...",
                    "...........")
            });

            // ——— 31–38: elevators + 2-layer ———
            add(new LevelDef
            {
                Id = 31, Title = "Upper Deck",
                RequestText = "Upper-deck cold box → Dock 1",
                RobotCallsign = "#J31", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 45f, TickSeconds = 0.42f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Elevators,
                Layers = L2(
                    new[]
                    {
                        "...........",
                        ".S>>E......",
                        "...........",
                        "...........",
                        "..........."
                    },
                    new[]
                    {
                        "...........",
                        "....E>>1...",
                        "...........",
                        "...........",
                        "..........."
                    })
            });

            add(new LevelDef
            {
                Id = 32, Title = "Deck Switch",
                RequestText = "Satellite battery pack → Dock 1",
                RobotCallsign = "#J32", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 44f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Elevators | MechanicsMask.Switches,
                Layers = L2(
                    new[]
                    {
                        "...........",
                        ".S>>E......",
                        "...........",
                        "...........",
                        "..........."
                    },
                    new[]
                    {
                        "...........",
                        "....E>+....",
                        "......v....",
                        "......+>>1.",
                        "..........."
                    })
            });

            add(new LevelDef
            {
                Id = 33, Title = "Down to Bay",
                RequestText = "Descending priority crate → Dock 1",
                RobotCallsign = "#J33", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 42f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 3), RobotLayer = 1, RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Elevators | MechanicsMask.Switches,
                Layers = L2(
                    new[]
                    {
                        "...........",
                        "....E>+....",
                        "......v....",
                        "......+>>1.",
                        "..........."
                    },
                    new[]
                    {
                        "...........",
                        ".S>>E......",
                        "...........",
                        "...........",
                        "..........."
                    })
            });

            add(new LevelDef
            {
                Id = 34, Title = "Split Floors",
                RequestText = "VIP vault case → Dock 2",
                RobotCallsign = "#J34", DockName = "Dock 2", TargetDockId = 2,
                TimeLimit = 48f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Elevators | MechanicsMask.Switches,
                Layers = L2(
                    new[]
                    {
                        "...........",
                        ".S>+>E.....",
                        "...v.......",
                        "...+>>>2...",
                        "..........."
                    },
                    new[]
                    {
                        "...........",
                        ".....E>+>1.",
                        "...........",
                        "...........",
                        "..........."
                    })
            });

            add(new LevelDef
            {
                Id = 35, Title = "Elevator Bridge",
                RequestText = "Water-purification membranes → Dock 1",
                RobotCallsign = "#J35", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 40f, TickSeconds = 0.38f,
                RobotStart = new Vector2Int(0, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Elevators | MechanicsMask.Bridges,
                Layers = L2(
                    new[]
                    {
                        "...........",
                        "S>>B>>E....",
                        "...........",
                        "...........",
                        "..........."
                    },
                    new[]
                    {
                        "...........",
                        "......E>>1.",
                        "...........",
                        "...........",
                        "..........."
                    })
            });

            add(new LevelDef
            {
                Id = 36, Title = "Mezzanine Mirror",
                RequestText = "Fiber-optic spine → Dock 1",
                RobotCallsign = "#J36", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 44f, TickSeconds = 0.38f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Elevators | MechanicsMask.Reflectors | MechanicsMask.Switches,
                Layers = L2(
                    new[]
                    {
                        "...........",
                        ".S>>E......",
                        "...........",
                        "...........",
                        "..........."
                    },
                    new[]
                    {
                        "...........",
                        "....E>+>>M.",
                        "......v....",
                        "......+>>1.",
                        "..........."
                    })
            });

            add(new LevelDef
            {
                Id = 37, Title = "Dual Shaft",
                RequestText = "Reactor coolant sample → Dock 1",
                RobotCallsign = "#J37", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 48f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 4), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Elevators | MechanicsMask.Switches,
                Layers = L2(
                    new[]
                    {
                        "...........",
                        ".S>+>E.....",
                        "...v.......",
                        "...+>>>e...",
                        ".......v...",
                        ".......+>>1"
                    },
                    new[]
                    {
                        "...........",
                        ".....E>+...",
                        ".......v...",
                        ".......e...",
                        "...........",
                        "..........."
                    })
            });

            add(new LevelDef
            {
                Id = 38, Title = "Stacked Yard",
                RequestText = "Twin-deck medical pallet → Dock 2",
                RobotCallsign = "#J38", DockName = "Dock 2", TargetDockId = 2,
                TimeLimit = 50f, TickSeconds = 0.38f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                DecoyStarts = new[] { new Vector2Int(2, 1) },
                DecoyFacings = new[] { Dir.East },
                Mechanics = MechanicsMask.Elevators | MechanicsMask.Switches | MechanicsMask.Rotators,
                Layers = L2(
                    new[]
                    {
                        "...........",
                        ".S>+>R>E...",
                        "...........",
                        ".>>>>......",
                        "..........."
                    },
                    new[]
                    {
                        "...........",
                        ".......E>+1",
                        ".........v.",
                        ".........+2",
                        "..........."
                    })
            });
        }
    }
}
