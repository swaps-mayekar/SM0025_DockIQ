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
                RequestText = "Lift the crate, then rush #I25 to dock",
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
                RequestText = "Raise X and set switches for #I26",
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
                RequestText = "Raise both liftables for a clear run",
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
                RequestText = "Open bridge and raise crate for #I28",
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
                RequestText = "Raise X, steer to dock 2",
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
                RequestText = "Lift crate and use the mirror",
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
                RequestText = "Ride the elevator to the upper dock",
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
                        ".E>>>1.....",
                        "...........",
                        "...........",
                        "..........."
                    })
            });

            add(new LevelDef
            {
                Id = 32, Title = "Deck Switch",
                RequestText = "Elevator up, then switch to dock",
                RobotCallsign = "#J32", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 44f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Elevators | MechanicsMask.Switches,
                Layers = L2(
                    new[]
                    {
                        "...........",
                        ".S>+>E.....",
                        "...........",
                        "...........",
                        "..........."
                    },
                    new[]
                    {
                        "...........",
                        ".E>+>>>1...",
                        "...........",
                        "...........",
                        "..........."
                    })
            });

            add(new LevelDef
            {
                Id = 33, Title = "Down to Bay",
                RequestText = "Start upstairs — descend to dock 1",
                RobotCallsign = "#J33", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 42f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 3), RobotLayer = 1, RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Elevators | MechanicsMask.Switches,
                Layers = L2(
                    new[]
                    {
                        "...........",
                        ".E>+>>>1...",
                        "...........",
                        "...........",
                        "..........."
                    },
                    new[]
                    {
                        "...........",
                        ".S>+>E.....",
                        "...........",
                        "...........",
                        "..........."
                    })
            });

            add(new LevelDef
            {
                Id = 34, Title = "Split Floors",
                RequestText = "Choose the upper path to dock 2",
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
                        "...+>>>1...",
                        "..........."
                    },
                    new[]
                    {
                        "...........",
                        ".E>+>>>2...",
                        "...........",
                        "...........",
                        "..........."
                    })
            });

            add(new LevelDef
            {
                Id = 35, Title = "Elevator Bridge",
                RequestText = "Open bridge, ride up to dock",
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
                        ".E>>>>1....",
                        "...........",
                        "...........",
                        "..........."
                    })
            });

            add(new LevelDef
            {
                Id = 36, Title = "Mezzanine Mirror",
                RequestText = "Upper mirror sends #J36 home",
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
                        ".E>+>>>1...",
                        ".....M.....",
                        "...........",
                        "..........."
                    })
            });

            add(new LevelDef
            {
                Id = 37, Title = "Dual Shaft",
                RequestText = "Use the correct elevator pair",
                RobotCallsign = "#J37", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 48f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Elevators | MechanicsMask.Switches,
                Layers = L2(
                    new[]
                    {
                        "...........",
                        ".S>+>E.....",
                        "...v.......",
                        "...+>e>1...",
                        "..........."
                    },
                    new[]
                    {
                        "...........",
                        ".E>+.......",
                        "...v.......",
                        "...+>e.....",
                        "..........."
                    })
            });

            add(new LevelDef
            {
                Id = 38, Title = "Stacked Yard",
                RequestText = "Navigate both decks to dock 2",
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
                        ".E>+>>>2...",
                        "...v.......",
                        "...+>>>1...",
                        "..........."
                    })
            });
        }
    }
}
