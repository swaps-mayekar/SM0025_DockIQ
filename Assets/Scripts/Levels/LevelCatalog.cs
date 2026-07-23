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
                // L1 — railway switch: must turn south then east to Tokyo Dock
                new LevelDef
                {
                    Id = 1,
                    Title = "AI Offline",
                    RequestText = "Guide Robot #A13 to Tokyo Dock",
                    RobotCallsign = "#A13",
                    DockName = "Tokyo Dock",
                    TargetDockId = 1,
                    TimeLimit = 50f,
                    TickSeconds = 0.48f,
                    RobotStart = new Vector2Int(1, 3),
                    RobotFacing = Dir.East,
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

                // L2 — wrong gate: Osaka(1) vs Tokyo(2)
                new LevelDef
                {
                    Id = 2,
                    Title = "Wrong Bay",
                    RequestText = "Guide Robot #B07 to Tokyo Dock",
                    RobotCallsign = "#B07",
                    DockName = "Tokyo Dock",
                    TargetDockId = 2,
                    TimeLimit = 45f,
                    TickSeconds = 0.44f,
                    RobotStart = new Vector2Int(1, 5),
                    RobotFacing = Dir.East,
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

                // L3 — busy yard: decoy robots + switches
                new LevelDef
                {
                    Id = 3,
                    Title = "Busy Yard",
                    RequestText = "Guide Robot #C21 to Chicago Dock",
                    RobotCallsign = "#C21",
                    DockName = "Chicago Dock",
                    TargetDockId = 1,
                    TimeLimit = 40f,
                    TickSeconds = 0.42f,
                    RobotStart = new Vector2Int(1, 3),
                    RobotFacing = Dir.East,
                    DecoyStarts = new[]
                    {
                        new Vector2Int(1, 1),
                        new Vector2Int(3, 1)
                    },
                    DecoyFacings = new[] { Dir.East, Dir.East },
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

                // L4 — drawbridge: starts CLOSED — open it before the robot arrives
                new LevelDef
                {
                    Id = 4,
                    Title = "Drawbridge",
                    RequestText = "Guide Robot #D04 to Berlin Dock",
                    RobotCallsign = "#D04",
                    DockName = "Berlin Dock",
                    TargetDockId = 1,
                    TimeLimit = 32f,
                    TickSeconds = 0.40f,
                    RobotStart = new Vector2Int(0, 3),
                    RobotFacing = Dir.East,
                    Mechanics = MechanicsMask.Switches | MechanicsMask.Bridges,
                    Rows = new[]
                    {
                        "...........",
                        "S>+>B>+>1..",
                        "...........",
                        "...........",
                        "..........."
                    }
                },

                // L5 — rotator + lift + dual docks
                new LevelDef
                {
                    Id = 5,
                    Title = "Lift & Turntable",
                    RequestText = "Guide Robot #E99 to Osaka Dock",
                    RobotCallsign = "#E99",
                    DockName = "Osaka Dock",
                    TargetDockId = 2,
                    TimeLimit = 38f,
                    TickSeconds = 0.38f,
                    RobotStart = new Vector2Int(1, 5),
                    RobotFacing = Dir.East,
                    DecoyStarts = new[] { new Vector2Int(2, 3) },
                    DecoyFacings = new[] { Dir.East },
                    Mechanics = MechanicsMask.Switches | MechanicsMask.Rotators | MechanicsMask.Lifts,
                    Rows = new[]
                    {
                        "...........",
                        ".S>+>R>A...",
                        "...........",
                        ".a>+>+>1...",
                        ".....v.....",
                        ".....+>>>2.",
                        "..........."
                    }
                }
            };
        }
    }
}
