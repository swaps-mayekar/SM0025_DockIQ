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
        public const string Rotators = "mech_rotators";
        public const string Lifts = "mech_lifts";
        public const string Reflectors = "mech_reflectors";
        public const string Obstacles = "mech_obstacles";
        public const string Movables = "mech_movables";
        public const string Liftables = "mech_liftables";
        public const string Elevators = "mech_elevators";

        public static List<TutorialTip> GetPendingTips(LevelDef level)
        {
            var tips = new List<TutorialTip>(4);

            if (level.Id == 1)
                TryAdd(tips, MissionBasics, "Warehouse Rescue",
                    "Robots drive the rails on their own. Tap switches to reroute the highlighted rescue robot to the correct dock before time runs out.");

            if (level.Id == 2)
                TryAdd(tips, WrongDock, "Pick the Right Gate",
                    "Multiple docks are open. Only the named dock counts — the wrong bay fails the rescue.");

            if (level.DecoyStarts != null && level.DecoyStarts.Length > 0)
                TryAdd(tips, Decoys, "Decoy Traffic",
                    "Other robots share the yard. They ignore your mission. Collisions with scrap or blocked paths fail instantly.");

            var m = level.Mechanics;
            if ((m & MechanicsMask.Switches) != 0 && level.Id != 1)
                TryAdd(tips, Switches, "Switches",
                    "Tap a switch (+) to flip which way the track forks.");

            if ((m & MechanicsMask.Bridges) != 0)
                TryAdd(tips, Bridges, "Drawbridges",
                    "Tap a bridge (B) to open or close it. Closed bridges block robots.");

            if ((m & MechanicsMask.Rotators) != 0)
                TryAdd(tips, Rotators, "Turntables",
                    "Tap a turntable (R) to rotate the intersection and change which exit the robot takes.");

            if ((m & MechanicsMask.Lifts) != 0)
                TryAdd(tips, Lifts, "Freight Lifts",
                    "Matching lift pads (A/a) teleport a robot across the same floor. Time the transfer so your rescue robot lands on the right path.");

            if ((m & MechanicsMask.Reflectors) != 0)
                TryAdd(tips, Reflectors, "Reflectors",
                    "Mirrors (M) reverse a robot's travel direction when it hits them.");

            if ((m & MechanicsMask.Obstacles) != 0)
                TryAdd(tips, Obstacles, "Scrap & Obstacles",
                    "Obstacles block the track. Hitting scrap fails the level — clear or slide them out of the way.");

            if ((m & MechanicsMask.Movables) != 0)
                TryAdd(tips, Movables, "Sliding Pieces",
                    "Tap path pieces to slide them along their route. Use them to open lanes or redirect traffic.");

            if ((m & MechanicsMask.Liftables) != 0)
                TryAdd(tips, Liftables, "Liftable Crates",
                    "Tap a liftable (X) to raise it. Raised crates can be passed under; lowered ones block and clash.");

            if ((m & MechanicsMask.Elevators) != 0)
                TryAdd(tips, Elevators, "Elevators",
                    "Elevators (E/e) move robots between floors. Watch both decks — decoys and docks may be upstairs.");

            return tips;
        }

        /// <summary>Extra tip ids to mark seen when a tip is dismissed (avoids duplicate switch tip after level 1).</summary>
        public static void MarkDismissed(string tipId)
        {
            ProgressStore.MarkTutorialTipSeen(tipId);
            if (tipId == MissionBasics)
                ProgressStore.MarkTutorialTipSeen(Switches);
        }

        private static void TryAdd(List<TutorialTip> tips, string id, string title, string body)
        {
            if (ProgressStore.HasSeenTutorialTip(id))
                return;
            tips.Add(new TutorialTip(id, title, body));
        }
    }
}
