using DockIQ.Board;
using UnityEngine;

namespace DockIQ.Levels
{
    public static partial class LevelCatalog
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

        private static string[][] L0(params string[] rows) => new[] { rows };

        private static string[][] L2(string[] ground, string[] upper) => new[] { ground, upper };

        private static MovableDef Mov(char kind, int start, int mode, params Vector3Int[] path) =>
            new MovableDef
            {
                Kind = kind,
                StartIndex = start,
                RotatorMode = mode,
                Path = path
            };

        private static MovableDef Mov(char kind, int start, params Vector3Int[] path) =>
            Mov(kind, start, 0, path);

        private static Vector3Int P(int x, int y, int layer = 0) => new Vector3Int(x, y, layer);

        private static LevelDef[] Build()
        {
            var list = new LevelDef[48];
            int i = 0;
            void Add(LevelDef def) => list[i++] = def;

            // ——— 1–4: switches, docks, decoys ———
            Add(new LevelDef
            {
                Id = 1, Title = "AI Offline",
                RequestText = "Guide Robot #A13 to Tokyo Dock",
                RobotCallsign = "#A13", DockName = "Tokyo Dock", TargetDockId = 1,
                TimeLimit = 50f, TickSeconds = 0.48f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Switches,
                Layers = L0(
                    "........",
                    ".S>+....",
                    "...v....",
                    "...+>1..",
                    "........")
            });

            Add(new LevelDef
            {
                Id = 2, Title = "Wrong Bay",
                RequestText = "Guide Robot #B07 to Tokyo Dock",
                RobotCallsign = "#B07", DockName = "Tokyo Dock", TargetDockId = 2,
                TimeLimit = 45f, TickSeconds = 0.44f,
                RobotStart = new Vector2Int(1, 5), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Switches,
                Layers = L0(
                    "...........",
                    ".S>+>+.....",
                    ".....v.....",
                    ".....+>1...",
                    ".....v.....",
                    ".....+>>>2.",
                    "...........")
            });

            Add(new LevelDef
            {
                Id = 3, Title = "Busy Yard",
                RequestText = "Guide Robot #C21 to Chicago Dock",
                RobotCallsign = "#C21", DockName = "Chicago Dock", TargetDockId = 1,
                TimeLimit = 40f, TickSeconds = 0.42f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                DecoyStarts = new[] { new Vector2Int(1, 1), new Vector2Int(3, 1) },
                DecoyFacings = new[] { Dir.East, Dir.East },
                Mechanics = MechanicsMask.Switches,
                Layers = L0(
                    "...........",
                    ".S>+>+>+>1.",
                    "...........",
                    ".>>>>>>>...",
                    "...........")
            });

            Add(new LevelDef
            {
                Id = 4, Title = "Fork Choice",
                RequestText = "Guide Robot #D11 to Seoul Dock",
                RobotCallsign = "#D11", DockName = "Seoul Dock", TargetDockId = 2,
                TimeLimit = 42f, TickSeconds = 0.42f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Switches,
                Layers = L0(
                    "..........",
                    ".S>+>1....",
                    "...v......",
                    "...+>>>2..",
                    "..........")
            });

            // ——— 5–8: bridges + rotators ———
            Add(new LevelDef
            {
                Id = 5, Title = "Drawbridge",
                RequestText = "Guide Robot #D04 to Berlin Dock",
                RobotCallsign = "#D04", DockName = "Berlin Dock", TargetDockId = 1,
                TimeLimit = 32f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(0, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Switches | MechanicsMask.Bridges,
                Layers = L0(
                    "...........",
                    "S>+>B>+>1..",
                    "...........",
                    "...........",
                    "...........")
            });

            Add(new LevelDef
            {
                Id = 6, Title = "Turntable Intro",
                RequestText = "Guide Robot #E01 to Osaka Dock",
                RobotCallsign = "#E01", DockName = "Osaka Dock", TargetDockId = 1,
                TimeLimit = 40f, TickSeconds = 0.42f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Rotators | MechanicsMask.Switches,
                Layers = L0(
                    ".........",
                    ".S>>R....",
                    "....+>1..",
                    ".........",
                    ".........")
            });

            Add(new LevelDef
            {
                Id = 7, Title = "Bridge & Bend",
                RequestText = "Guide Robot #E12 to Rome Dock",
                RobotCallsign = "#E12", DockName = "Rome Dock", TargetDockId = 1,
                TimeLimit = 36f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Switches | MechanicsMask.Bridges | MechanicsMask.Rotators,
                Layers = L0(
                    "...........",
                    ".S>+>B>R...",
                    ".......+>1.",
                    "...........",
                    "...........")
            });

            Add(new LevelDef
            {
                Id = 8, Title = "Lift & Turntable",
                RequestText = "Guide Robot #E99 to Osaka Dock",
                RobotCallsign = "#E99", DockName = "Osaka Dock", TargetDockId = 2,
                TimeLimit = 38f, TickSeconds = 0.38f,
                RobotStart = new Vector2Int(1, 5), RobotFacing = Dir.East,
                DecoyStarts = new[] { new Vector2Int(2, 3) },
                DecoyFacings = new[] { Dir.East },
                Mechanics = MechanicsMask.Switches | MechanicsMask.Rotators | MechanicsMask.Lifts,
                Layers = L0(
                    "...........",
                    ".S>+>R>A...",
                    "...........",
                    ".a>+>+>1...",
                    ".....v.....",
                    ".....+>>>2.",
                    "...........")
            });

            // ——— 9–12: fixed reflectors ———
            Add(new LevelDef
            {
                Id = 9, Title = "Mirror Hall",
                RequestText = "Guide Robot #F09 to Paris Dock",
                RobotCallsign = "#F09", DockName = "Paris Dock", TargetDockId = 1,
                TimeLimit = 40f, TickSeconds = 0.42f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Switches | MechanicsMask.Reflectors,
                Layers = L0(
                    "...........",
                    ".S>+>M.....",
                    "...v.......",
                    "...+>>>1...",
                    "...........")
            });

            Add(new LevelDef
            {
                Id = 10, Title = "Bounce Back",
                RequestText = "Guide Robot #F10 to Madrid Dock",
                RobotCallsign = "#F10", DockName = "Madrid Dock", TargetDockId = 1,
                TimeLimit = 38f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 2), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Switches | MechanicsMask.Reflectors,
                Layers = L0(
                    "...........",
                    ".>>>>M.....",
                    ".S>+.......",
                    "...+>>>1...",
                    "...........")
            });

            Add(new LevelDef
            {
                Id = 11, Title = "Double Mirror",
                RequestText = "Guide Robot #F11 to Lisbon Dock",
                RobotCallsign = "#F11", DockName = "Lisbon Dock", TargetDockId = 2,
                TimeLimit = 42f, TickSeconds = 0.40f,
                RobotStart = new Vector2Int(1, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Switches | MechanicsMask.Reflectors,
                Layers = L0(
                    "...........",
                    ".S>+>M>1...",
                    "...v.......",
                    "...+>>>2...",
                    "...........")
            });

            Add(new LevelDef
            {
                Id = 12, Title = "Reflect & Bridge",
                RequestText = "Guide Robot #F12 to Vienna Dock",
                RobotCallsign = "#F12", DockName = "Vienna Dock", TargetDockId = 1,
                TimeLimit = 36f, TickSeconds = 0.38f,
                RobotStart = new Vector2Int(0, 3), RobotFacing = Dir.East,
                Mechanics = MechanicsMask.Switches | MechanicsMask.Reflectors | MechanicsMask.Bridges,
                Layers = L0(
                    "...........",
                    "S>+>M......",
                    "..v........",
                    "..+>B>>1...",
                    "...........")
            });

            AddRange13to24(Add);
            AddRange25to38(Add);
            AddRange39to48(Add);

            if (i != 48)
                Debug.LogError($"LevelCatalog expected 48 levels, built {i}");

            return list;
        }
    }
}
