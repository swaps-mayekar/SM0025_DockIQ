using System.Collections.Generic;
using DockIQ.Board;
using DockIQ.Levels;

namespace DockIQ.Core
{
    public readonly struct AchievementDef
    {
        public readonly string Id;
        public readonly string Title;
        public readonly string Description;
        public readonly string LockedHint;

        public AchievementDef(string id, string title, string description, string lockedHint)
        {
            Id = id;
            Title = title;
            Description = description;
            LockedHint = lockedHint;
        }
    }

    public static class AchievementCatalog
    {
        public const string FirstRescue = "first_rescue";
        public const string Story12 = "story_12";
        public const string Story24 = "story_24";
        public const string Story48 = "story_48";
        public const string DecoyDodge = "decoy_dodge";
        public const string CloseCall = "close_call";
        public const string Leisurely = "leisurely";
        public const string MultiFloor = "multi_floor";
        public const string FreeReplay = "free_replay";

        public static IReadOnlyList<AchievementDef> All { get; } = new[]
        {
            new AchievementDef(FirstRescue, "First Rescue",
                "Cleared your first warehouse level.",
                "Clear any level."),
            new AchievementDef(Story12, "Bay Cleared",
                "Completed Story through level 12.",
                "Reach Story progress 12/48."),
            new AchievementDef(Story24, "Half Yard",
                "Completed Story through level 24.",
                "Reach Story progress 24/48."),
            new AchievementDef(Story48, "Full Yard Mastery",
                "Completed every Story rescue in the yard.",
                "Clear all 48 Story levels."),
            new AchievementDef(DecoyDodge, "Decoy Dodge",
                "Cleared a level with decoy traffic.",
                "Clear a level that has decoys."),
            new AchievementDef(CloseCall, "Close Call",
                "Won with under 5 seconds left on the clock.",
                "Win with under 5 seconds remaining."),
            new AchievementDef(Leisurely, "Leisurely Run",
                "Won with at least half the timer remaining.",
                "Win with 50% or more time left."),
            new AchievementDef(MultiFloor, "Dual Deck",
                "Cleared a multi-floor elevator level.",
                "Clear a level that uses elevators."),
            new AchievementDef(FreeReplay, "Free Replay",
                "Cleared a level in Free Play.",
                "Clear any unlocked level in Free Play.")
        };
    }

    public static class AchievementStore
    {
        public static bool IsUnlocked(string id) => ProgressStore.HasAchievement(id);

        public static bool Unlock(string id) => ProgressStore.MarkAchievement(id);

        /// <summary>
        /// Evaluates win-based badges and unlocks any newly earned ones.
        /// Call after Story progress is updated so milestone checks see current lastCompleted.
        /// </summary>
        public static List<AchievementDef> EvaluateOnWin(LevelDef level, float timeLeft, GameMode mode)
        {
            var newlyUnlocked = new List<AchievementDef>(4);
            if (level == null)
                return newlyUnlocked;

            TryUnlock(newlyUnlocked, AchievementCatalog.FirstRescue);

            EvaluateStoryMilestones(newlyUnlocked);

            if (level.DecoyStarts != null && level.DecoyStarts.Length > 0)
                TryUnlock(newlyUnlocked, AchievementCatalog.DecoyDodge);

            if (timeLeft < 5f)
                TryUnlock(newlyUnlocked, AchievementCatalog.CloseCall);

            if (level.TimeLimit > 0f && timeLeft >= level.TimeLimit * 0.5f)
                TryUnlock(newlyUnlocked, AchievementCatalog.Leisurely);

            if ((level.Mechanics & MechanicsMask.Elevators) != 0)
                TryUnlock(newlyUnlocked, AchievementCatalog.MultiFloor);

            if (mode == GameMode.FreePlay)
                TryUnlock(newlyUnlocked, AchievementCatalog.FreeReplay);

            return newlyUnlocked;
        }

        /// <summary>
        /// Grants Story milestone badges from saved progress (e.g. after updating the game).
        /// </summary>
        public static void EvaluateFromProgress()
        {
            var newlyUnlocked = new List<AchievementDef>(4);
            if (ProgressStore.Current.lastCompleted > 0)
                TryUnlock(newlyUnlocked, AchievementCatalog.FirstRescue);
            EvaluateStoryMilestones(newlyUnlocked);
        }

        private static void EvaluateStoryMilestones(List<AchievementDef> newlyUnlocked)
        {
            int lastCompleted = ProgressStore.Current.lastCompleted;
            if (lastCompleted >= 12)
                TryUnlock(newlyUnlocked, AchievementCatalog.Story12);
            if (lastCompleted >= 24)
                TryUnlock(newlyUnlocked, AchievementCatalog.Story24);
            if (lastCompleted >= GameConstants.TotalLevels)
                TryUnlock(newlyUnlocked, AchievementCatalog.Story48);
        }

        private static void TryUnlock(List<AchievementDef> newlyUnlocked, string id)
        {
            if (!Unlock(id))
                return;

            for (int i = 0; i < AchievementCatalog.All.Count; i++)
            {
                if (AchievementCatalog.All[i].Id == id)
                {
                    newlyUnlocked.Add(AchievementCatalog.All[i]);
                    return;
                }
            }
        }
    }
}
