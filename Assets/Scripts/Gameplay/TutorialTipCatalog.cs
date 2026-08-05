using System.Collections.Generic;
using DockIQ.Board;
using DockIQ.Core;
using DockIQ.Levels;

namespace DockIQ.Gameplay
{
    public readonly struct TutorialTip
    {
        public readonly string Id;
        public readonly string Title;
        public readonly string Body;

        public TutorialTip(string id, string title, string body)
        {
            Id = id;
            Title = title;
            Body = body;
        }
    }

    public static class TutorialTipCatalog
    {
        public const string MissionBasics = "mission_basics";
        public const string WrongDock = "wrong_dock";
        public const string Decoys = "decoys";
        public const string Switches = "mech_switches";
        public const string Bridges = "mech_bridges";
        /// <summary>Legacy id — turntables now use <see cref="Switches"/>.</summary>
        public const string Rotators = "mech_rotators";
        public const string Lifts = "mech_lifts";
        public const string Reflectors = "mech_reflectors";
        public const string Obstacles = "mech_obstacles";
        public const string Movables = "mech_movables";
        public const string Liftables = "mech_liftables";
        public const string Elevators = "mech_elevators";

        public static IReadOnlyList<TutorialTip> AllTips { get; } = new[]
        {
            new TutorialTip(MissionBasics, "Manual Control",
                "Robots drive themselves. Tap turntables to route the highlighted rescue robot to the named dock before time runs out."),
            new TutorialTip(WrongDock, "Pick the Right Gate",
                "Only the named dock counts — the wrong bay fails."),
            new TutorialTip(Decoys, "Decoy Traffic",
                "Other robots ignore your mission. Collisions fail instantly."),
            new TutorialTip(Switches, "Turntables",
                "Tap a turntable to cycle Straight, Left, and Right."),
            new TutorialTip(Bridges, "Drawbridges",
                "Tap to open or close. Closed bridges block robots."),
            new TutorialTip(Lifts, "Freight Lifts",
                "Matching pads teleport a robot across the same floor."),
            new TutorialTip(Reflectors, "Reflectors",
                "Mirrors reverse a robot's travel direction."),
            new TutorialTip(Obstacles, "Scrap & Obstacles",
                "Hitting scrap fails. Slide or clear blockers off the track."),
            new TutorialTip(Movables, "Sliding Pieces",
                "Tap path pieces to slide them and open or redirect lanes."),
            new TutorialTip(Liftables, "Liftable Crates",
                "Tap to raise. Raised crates can be passed; lowered ones block."),
            new TutorialTip(Elevators, "Elevators",
                "Move robots between floors. Decys and docks may be upstairs.")
        };

        public static List<TutorialTip> GetPendingTips(LevelDef level)
        {
            var tips = new List<TutorialTip>(4);

            if (level.Id == 1)
                TryAdd(tips, MissionBasics);

            if (level.Id == 2)
                TryAdd(tips, WrongDock);

            if (level.DecoyStarts != null && level.DecoyStarts.Length > 0)
                TryAdd(tips, Decoys);

            var m = level.Mechanics;
            bool hasTurntable = (m & MechanicsMask.Switches) != 0 || (m & MechanicsMask.Rotators) != 0;
            if (hasTurntable && level.Id != 1)
                TryAddTurntable(tips);

            if ((m & MechanicsMask.Bridges) != 0)
                TryAdd(tips, Bridges);

            if ((m & MechanicsMask.Lifts) != 0)
                TryAdd(tips, Lifts);

            if ((m & MechanicsMask.Reflectors) != 0)
                TryAdd(tips, Reflectors);

            if ((m & MechanicsMask.Obstacles) != 0)
                TryAdd(tips, Obstacles);

            if ((m & MechanicsMask.Movables) != 0)
                TryAdd(tips, Movables);

            if ((m & MechanicsMask.Liftables) != 0)
                TryAdd(tips, Liftables);

            if ((m & MechanicsMask.Elevators) != 0)
                TryAdd(tips, Elevators);

            return tips;
        }

        private static TutorialTip FindTip(string id)
        {
            for (int i = 0; i < AllTips.Count; i++)
            {
                if (AllTips[i].Id == id)
                    return AllTips[i];
            }

            return default;
        }

        /// <summary>Extra tip ids to mark seen when a tip is dismissed (avoids duplicate turntable tips).</summary>
        public static void MarkDismissed(string tipId)
        {
            ProgressStore.MarkTutorialTipSeen(tipId);
            if (tipId == MissionBasics || tipId == Switches || tipId == Rotators)
                MarkTurntableSeen();
        }

        private static void MarkTurntableSeen()
        {
            ProgressStore.MarkTutorialTipSeen(Switches);
            ProgressStore.MarkTutorialTipSeen(Rotators);
        }

        private static bool HasSeenTurntableTip() =>
            ProgressStore.HasSeenTutorialTip(Switches) || ProgressStore.HasSeenTutorialTip(Rotators);

        private static void TryAddTurntable(List<TutorialTip> tips)
        {
            if (HasSeenTurntableTip())
                return;
            tips.Add(FindTip(Switches));
        }

        private static void TryAdd(List<TutorialTip> tips, string id)
        {
            if (ProgressStore.HasSeenTutorialTip(id))
                return;
            tips.Add(FindTip(id));
        }
    }
}
