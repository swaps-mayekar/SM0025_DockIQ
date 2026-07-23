using System;
using DockIQ.Board;
using UnityEngine;

namespace DockIQ.Levels
{
    public static partial class LevelCatalog
    {
        private static void AddRange13to24(Action<LevelDef> add)
        {
            // ——— 13–18: sliding obstacles ———
            add(new LevelDef
            {
                Id = 13, Title = "Clear the Aisle",
                RequestText = "Slide the fallen bot, then guide #G13 to Dock",
                RobotCallsign = "#G13", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 40f, TickSeconds = 0.42f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Obstacles | MechanicsMask.Movables,
                Layers = L0(
                    "...........",
                    ".S>>>>1....",
                    "...>.......",
                    "...>.......",
                    "..........."),
                Movables = new[]
                {
                    Mov('O', 0, P(3, 3), P(3, 2), P(3, 1))
                }
            });

            add(new LevelDef
            {
                Id = 14, Title = "Side Track Block",
                RequestText = "Move the obstacle off the main line",
                RobotCallsign = "#G14", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 38f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Switches | MechanicsMask.Obstacles | MechanicsMask.Movables,
                Layers = L0(
                    "...........",
                    ".S>+>>>1...",
                    "...v.......",
                    "...>.......",
                    "..........."),
                Movables = new[]
                {
                    Mov('O', 0, P(4, 3), P(3, 2), P(3, 1))
                }
            });

            add(new LevelDef
            {
                Id = 15, Title = "Two Crates",
                RequestText = "Clear both blockers for #G15",
                RobotCallsign = "#G15", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 45f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Obstacles | MechanicsMask.Movables,
                Layers = L0(
                    "...........",
                    ".S>>>>>1...",
                    "...>.......",
                    "...>.>.....",
                    "..........."),
                Movables = new[]
                {
                    Mov('O', 0, P(3, 3), P(3, 2)),
                    Mov('O', 0, P(5, 3), P(5, 1))
                }
            });

            add(new LevelDef
            {
                Id = 16, Title = "Decoy Jam",
                RequestText = "Slide scrap aside before #G16 arrives",
                RobotCallsign = "#G16", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 36f, TickSeconds = 0.38f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                DecoyStarts = new[] { new Vector2Int(1, 1) },
                DecoyFacings = new[] { Dir.East },
                Mechanics = MechanicsMask.Switches | MechanicsMask.Obstacles | MechanicsMask.Movables,
                Layers = L0(
                    "...........",
                    ".S>+>>>1...",
                    ".....>.....",
                    ".>>>>.>....",
                    "..........."),
                Movables = new[]
                {
                    Mov('O', 0, P(4, 3), P(5, 2), P(6, 1))
                }
            });

            add(new LevelDef
            {
                Id = 17, Title = "Narrow Gap",
                RequestText = "Park the obstacle on the spur",
                RobotCallsign = "#G17", DockName = "Dock 2", TargetDockId = 2,
                TimeLimit = 42f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Switches | MechanicsMask.Obstacles | MechanicsMask.Movables,
                Layers = L0(
                    "...........",
                    ".S>+>1.....",
                    "...v.......",
                    "...+>>>2...",
                    "...>.>...>."),
                Movables = new[]
                {
                    Mov('O', 0, P(5, 1), P(5, 0), P(3, 0))
                }
            });

            add(new LevelDef
            {
                Id = 18, Title = "Obstacle Timing",
                RequestText = "Open a window for #G18",
                RobotCallsign = "#G18", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 34f, TickSeconds = 0.36f,
                RobotStart = new Vector2Int(0, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Bridges | MechanicsMask.Obstacles | MechanicsMask.Movables,
                Layers = L0(
                    "...........",
                    "S>>B>>>1...",
                    "....>......",
                    "....>......",
                    "..........."),
                Movables = new[]
                {
                    Mov('O', 0, P(4, 3), P(4, 2), P(4, 1))
                }
            });

            // ——— 19–24: moving rotators ———
            add(new LevelDef
            {
                Id = 19, Title = "Sliding Turntable",
                RequestText = "Slide & set the turntable for #H19",
                RobotCallsign = "#H19", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 42f, TickSeconds = 0.42f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Rotators | MechanicsMask.Movables | MechanicsMask.Switches,
                Layers = L0(
                    "...........",
                    ".S>>.......",
                    "...+>>1....",
                    "...>.......",
                    "..........."),
                Movables = new[]
                {
                    Mov('R', 0, 2, P(3, 3), P(3, 2), P(3, 1))
                }
            });

            add(new LevelDef
            {
                Id = 20, Title = "Path Pivot",
                RequestText = "Move the rotator onto the junction",
                RobotCallsign = "#H20", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 40f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Switches | MechanicsMask.Rotators | MechanicsMask.Movables,
                Layers = L0(
                    "...........",
                    ".S>+.......",
                    "...v.......",
                    "...>>>>1...",
                    "...>......."),
                Movables = new[]
                {
                    Mov('R', 0, 1, P(3, 1), P(3, 2), P(3, 3))
                }
            });

            add(new LevelDef
            {
                Id = 21, Title = "Mobile Mirror",
                RequestText = "Slide the reflector into place",
                RobotCallsign = "#H21", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 40f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 2), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Switches | MechanicsMask.Reflectors | MechanicsMask.Movables,
                Layers = L0(
                    "...........",
                    ".>>>>......",
                    ".S>+>>.....",
                    "...+>>>1...",
                    "..........."),
                Movables = new[]
                {
                    Mov('m', 0, P(4, 3), P(4, 2), P(5, 2))
                }
            });

            add(new LevelDef
            {
                Id = 22, Title = "Rotator Relay",
                RequestText = "Cycle the turntable path for dock 2",
                RobotCallsign = "#H22", DockName = "Dock 2", TargetDockId = 2,
                TimeLimit = 44f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Rotators | MechanicsMask.Movables | MechanicsMask.Switches,
                Layers = L0(
                    "...........",
                    ".S>>>1.....",
                    "....v......",
                    "....+>>>2..",
                    "....>......"),
                Movables = new[]
                {
                    Mov('R', 0, 0, P(4, 3), P(4, 2), P(4, 1))
                }
            });

            add(new LevelDef
            {
                Id = 23, Title = "Twin Movers",
                RequestText = "Align rotator and clear scrap",
                RobotCallsign = "#H23", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 46f, TickSeconds = 0.38f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Rotators | MechanicsMask.Obstacles | MechanicsMask.Movables |
                             MechanicsMask.Switches,
                Layers = L0(
                    "...........",
                    ".S>>.......",
                    "...+>>1....",
                    "...>.>.....",
                    "..........."),
                Movables = new[]
                {
                    Mov('R', 0, 2, P(3, 3), P(3, 2), P(3, 1)),
                    Mov('O', 0, P(5, 2), P(5, 1), P(3, 1))
                }
            });

            add(new LevelDef
            {
                Id = 24, Title = "Mirror Slide",
                RequestText = "Bounce #H24 with a moving mirror",
                RobotCallsign = "#H24", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 38f, TickSeconds = 0.38f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Reflectors | MechanicsMask.Movables | MechanicsMask.Switches,
                Layers = L0(
                    "...........",
                    ".S>+>>>....",
                    "...v.......",
                    "...+>>>1...",
                    "..........."),
                Movables = new[]
                {
                    Mov('m', 0, P(6, 3), P(5, 3), P(4, 3))
                }
            });
        }
    }
}
