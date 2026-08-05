using System;
using DockIQ.Board;
using UnityEngine;

namespace DockIQ.Levels
{
    public static partial class LevelCatalog
    {
        private static void AddRange39to48(Action<LevelDef> add)
        {
            // ——— 39–48: combo hard puzzles ———
            add(new LevelDef
            {
                Id = 39, Title = "Full Toolkit",
                RequestText = "Multi-system trauma crate → Dock 1",
                RobotCallsign = "#K39", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 40f, TickSeconds = 0.36f,
                RobotStart = new Vector2Int(0, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Switches | MechanicsMask.Bridges | MechanicsMask.Liftables,
                Layers = L0(
                    "...........",
                    "S>+>B>X>+>1",
                    "...........",
                    "...........",
                    "...........")
            });

            add(new LevelDef
            {
                Id = 40, Title = "Scrap & Shaft",
                RequestText = "Elevator battery bank → Dock 1",
                RobotCallsign = "#K40", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 44f, TickSeconds = 0.38f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Elevators | MechanicsMask.Obstacles | MechanicsMask.Movables,
                Layers = L2(
                    new[]
                    {
                        "...........",
                        ".S>>>E.....",
                        "....>......",
                        "....>......",
                        "..........."
                    },
                    new[]
                    {
                        "...........",
                        ".....E>>>1.",
                        "...........",
                        "...........",
                        "..........."
                    }),
                Movables = new[]
                {
                    Mov('O', 0, P(4, 3), P(4, 2), P(4, 1))
                }
            });

            add(new LevelDef
            {
                Id = 41, Title = "Mirror Mezzanine",
                RequestText = "Sealed film archive → Dock 1",
                RobotCallsign = "#K41", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 46f, TickSeconds = 0.38f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Elevators | MechanicsMask.Reflectors | MechanicsMask.Movables |
                             MechanicsMask.Switches,
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
                        "....E>+>1..",
                        "......>....",
                        "...........",
                        "..........."
                    }),
                Movables = new[]
                {
                    Mov('m', 0, P(6, 2, 1), P(6, 3, 1), P(7, 3, 1))
                }
            });

            add(new LevelDef
            {
                Id = 42, Title = "Rotator Upstairs",
                RequestText = "High-bay vaccine tray → Dock 2",
                RobotCallsign = "#K42", DockName = "Dock 2", TargetDockId = 2,
                TimeLimit = 48f, TickSeconds = 0.38f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Elevators | MechanicsMask.Rotators | MechanicsMask.Movables |
                             MechanicsMask.Switches,
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
                        ".....E>>>2.",
                        "...........",
                        "...........",
                        "..........."
                    }),
                Movables = new[]
                {
                    // Cycles on upper approach tiles — never covers the elevator shaft.
                    Mov('R', 0, 2, P(8, 3, 1), P(7, 3, 1), P(6, 3, 1))
                }
            });

            add(new LevelDef
            {
                Id = 43, Title = "Pressure Cooker",
                RequestText = "Live-tissue cooler → Dock 1",
                RobotCallsign = "#K43", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 30f, TickSeconds = 0.34f,
                RobotStart = new Vector2Int(0, 3), RobotFacing = Dir.East,
                DecoyStarts = new[] { new Vector2Int(1, 1) },
                DecoyFacings = new[] { Dir.East },
                Mechanics = MechanicsMask.Bridges | MechanicsMask.Liftables | MechanicsMask.Switches,
                Layers = L0(
                    "...........",
                    "S>B>X>+>1..",
                    "...........",
                    ".>>>>......",
                    "...........")
            });

            add(new LevelDef
            {
                Id = 44, Title = "Triple Threat",
                RequestText = "Flight-recorder black box → Dock 2",
                RobotCallsign = "#K44", DockName = "Dock 2", TargetDockId = 2,
                TimeLimit = 48f, TickSeconds = 0.36f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Obstacles | MechanicsMask.Rotators | MechanicsMask.Movables |
                             MechanicsMask.Switches,
                Layers = L0(
                    "...........",
                    ".S>+>>>1...",
                    "...v.......",
                    "...+>>>2...",
                    "...>>>....."),
                Movables = new[]
                {
                    Mov('O', 0, P(4, 3), P(5, 1), P(4, 1)),
                    Mov('R', 0, 2, P(5, 0), P(4, 0), P(3, 0))
                }
            });

            add(new LevelDef
            {
                Id = 45, Title = "Cross Deck Bounce",
                RequestText = "Quantum sensor array → Dock 1",
                RobotCallsign = "#K45", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 46f, TickSeconds = 0.36f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Elevators | MechanicsMask.Reflectors | MechanicsMask.Liftables |
                             MechanicsMask.Switches,
                Layers = L2(
                    new[]
                    {
                        "...........",
                        ".S>X>E.....",
                        "...........",
                        "...........",
                        "..........."
                    },
                    new[]
                    {
                        "...........",
                        ".....E>+>1.",
                        ".......M...",
                        "...........",
                        "..........."
                    })
            });

            add(new LevelDef
            {
                Id = 46, Title = "Yard Symphony",
                RequestText = "Hospital generator coil → Dock 2",
                RobotCallsign = "#K46", DockName = "Dock 2", TargetDockId = 2,
                TimeLimit = 50f, TickSeconds = 0.36f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Switches | MechanicsMask.Bridges | MechanicsMask.Rotators |
                             MechanicsMask.Liftables | MechanicsMask.Reflectors,
                Layers = L0(
                    "...........",
                    ".S>+>B>R...",
                    "...v.......",
                    "...X>M>1...",
                    "...R>>>>>2.")
            });

            add(new LevelDef
            {
                Id = 47, Title = "Sky Bridge",
                RequestText = "Climate core → Dock 1",
                RobotCallsign = "#K47", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 52f, TickSeconds = 0.36f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Elevators | MechanicsMask.Obstacles | MechanicsMask.Movables |
                             MechanicsMask.Switches,
                Layers = L2(
                    new[]
                    {
                        "...........",
                        ".S>+>E.....",
                        "...v.......",
                        "...+>e.....",
                        "...>......."
                    },
                    new[]
                    {
                        "...........",
                        ".....E>....",
                        "...........",
                        ".....e>1...",
                        "....>.>...."
                    }),
                Movables = new[]
                {
                    // Blocks approach to dock; slide onto the spur — never covers elevators.
                    Mov('O', 0, P(6, 1, 1), P(6, 0, 1), P(5, 0, 1))
                }
            });

            add(new LevelDef
            {
                Id = 48, Title = "Final Dispatch",
                RequestText = "Master restore key → Tokyo Dock",
                RobotCallsign = "#K48", DockName = "Tokyo Dock", TargetDockId = 2,
                TimeLimit = 55f, TickSeconds = 0.34f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                DecoyStarts = new[] { new Vector2Int(1, 0), new Vector2Int(3, 0) },
                DecoyFacings = new[] { Dir.East, Dir.East },
                Mechanics = MechanicsMask.Elevators | MechanicsMask.Switches | MechanicsMask.Rotators |
                             MechanicsMask.Liftables | MechanicsMask.Movables | MechanicsMask.Obstacles |
                             MechanicsMask.Bridges,
                Layers = L2(
                    new[]
                    {
                        "...........",
                        ".S>+>B>E...",
                        "...v.......",
                        "...X>+>1...",
                        ".>>>>......"
                    },
                    new[]
                    {
                        "...........",
                        ".......E>>2",
                        ".........>.",
                        ".........>.",
                        "..........."
                    }),
                Movables = new[]
                {
                    Mov('O', 0, P(9, 2, 1), P(9, 1, 1), P(9, 0, 1)),
                    Mov('R', 0, 1, P(9, 3, 1), P(8, 3, 1))
                }
            });
        }
    }
}
