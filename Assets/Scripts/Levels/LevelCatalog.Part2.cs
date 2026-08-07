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
                RequestText = "Yellow biohazard cooler → Dock 1",
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
                RequestText = "City-grid failover drives → Dock 1",
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
                RequestText = "Fire-suppressant canisters → Dock 1",
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
                RequestText = "Priority Red evidence pouch → Dock 1",
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
                RequestText = "Organ-preservation pouch → Dock 2",
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
                RequestText = "Live coral research sample → Dock 1",
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
                RequestText = "Defibrillator units → Dock 1",
                RobotCallsign = "#H19", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 42f, TickSeconds = 0.42f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Rotators | MechanicsMask.Movables | MechanicsMask.Switches,
                Layers = L0(
                    "...........",
                    ".S>>>>>>...",
                    "....+>>1...",
                    "...........",
                    "..........."),
                Movables = new[]
                {
                    // Horizontal path only — does not cover the switch.
                    Mov('R', 0, 2, P(6, 3), P(5, 3), P(4, 3))
                }
            });

            add(new LevelDef
            {
                Id = 20, Title = "Path Pivot",
                RequestText = "Cryogenic stem-cell case → Dock 1",
                RobotCallsign = "#H20", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 40f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Switches | MechanicsMask.Rotators | MechanicsMask.Movables,
                Layers = L0(
                    "...........",
                    ".S>>>>>>...",
                    "...v.......",
                    "...+>>>1...",
                    "..........."),
                Movables = new[]
                {
                    // Slide onto (3,3); Right mode drops south. Switch below turns east.
                    Mov('R', 0, 2, P(6, 3), P(5, 3), P(3, 3))
                }
            });

            add(new LevelDef
            {
                Id = 21, Title = "Mobile Mirror",
                RequestText = "Night-vision optics → Dock 1",
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
                RequestText = "Sealed antidote drum → Dock 2",
                RobotCallsign = "#H22", DockName = "Dock 2", TargetDockId = 2,
                TimeLimit = 48f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 5), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Rotators | MechanicsMask.Movables | MechanicsMask.Switches,
                Layers = L0(
                    "...........",
                    ".S>>>>>>...",
                    ".....v.....",
                    ".....+>1...",
                    ".....v.....",
                    ".....+>>>2.",
                    "..........."),
                Movables = new[]
                {
                    // Slide onto (5,5) to drop south. First junction defaults to wrong Dock 1 —
                    // flip it south, then ride the lower switch east to Dock 2.
                    Mov('R', 0, 2, P(7, 5), P(6, 5), P(5, 5))
                }
            });

            add(new LevelDef
            {
                Id = 23, Title = "Twin Movers",
                RequestText = "Emergency radio kits → Dock 1",
                RobotCallsign = "#H23", DockName = "Dock 1", TargetDockId = 1,
                TimeLimit = 46f, TickSeconds = 0.38f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Rotators | MechanicsMask.Obstacles | MechanicsMask.Movables |
                             MechanicsMask.Switches,
                Layers = L0(
                    "...........",
                    ".S>>>>>>...",
                    "....+>>1...",
                    "......>....",
                    "..........."),
                Movables = new[]
                {
                    Mov('R', 0, 2, P(6, 3), P(5, 3), P(4, 3)),
                    Mov('O', 0, P(6, 2), P(6, 1), P(5, 1))
                }
            });

            add(new LevelDef
            {
                Id = 24, Title = "Mirror Slide",
                RequestText = "Diplomatic gold pouch → Dock 1",
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
