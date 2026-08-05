using System;
using UnityEngine;

namespace DockIQ.Core
{
    [Serializable]
    public sealed class PlayerProfile
    {
        public int version = 1;
        public int highestUnlocked = 1;
        public int lastCompleted;
        /// <summary>Comma-separated tutorial tip ids the player has already seen.</summary>
        public string seenTutorialTips = "";
        /// <summary>Comma-separated achievement ids the player has unlocked.</summary>
        public string unlockedAchievements = "";
    }

    public static class ProgressStore
    {
        private static PlayerProfile _current;

        public static event Action Changed;

        public static PlayerProfile Current => _current ??= Load();

        public static PlayerProfile Load()
        {
            if (!PlayerPrefs.HasKey(GameConstants.PrefProfile))
                return new PlayerProfile();

            try
            {
                var loaded = JsonUtility.FromJson<PlayerProfile>(
                    PlayerPrefs.GetString(GameConstants.PrefProfile));
                return loaded ?? new PlayerProfile();
            }
            catch
            {
                return new PlayerProfile();
            }
        }

        public static void Save()
        {
            PlayerPrefs.SetString(GameConstants.PrefProfile, JsonUtility.ToJson(Current));
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public static bool IsUnlocked(int levelId) =>
            levelId >= 1 && levelId <= Current.highestUnlocked;

        public static void MarkLevelCompleted(int levelId)
        {
            if (levelId > Current.lastCompleted)
                Current.lastCompleted = levelId;

            int next = levelId + 1;
            if (next > Current.highestUnlocked && next <= GameConstants.TotalLevels)
                Current.highestUnlocked = next;

            Save();
        }

        public static int GetSelectedLevel() =>
            Mathf.Clamp(PlayerPrefs.GetInt(GameConstants.PrefSelectedLevel, 1), 1, GameConstants.TotalLevels);

        public static void SetSelectedLevel(int levelId)
        {
            PlayerPrefs.SetInt(GameConstants.PrefSelectedLevel, Mathf.Clamp(levelId, 1, GameConstants.TotalLevels));
            PlayerPrefs.Save();
        }

        public static bool HasSeenTutorialTip(string tipId)
        {
            if (string.IsNullOrEmpty(tipId))
                return true;

            string seen = Current.seenTutorialTips ?? "";
            if (string.IsNullOrEmpty(seen))
                return false;

            var parts = seen.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == tipId)
                    return true;
            }

            return false;
        }

        public static void MarkTutorialTipSeen(string tipId)
        {
            if (string.IsNullOrEmpty(tipId) || HasSeenTutorialTip(tipId))
                return;

            Current.seenTutorialTips = string.IsNullOrEmpty(Current.seenTutorialTips)
                ? tipId
                : Current.seenTutorialTips + "," + tipId;
            Save();
        }

        public static void ClearTutorialTips()
        {
            Current.seenTutorialTips = "";
            Save();
        }

        public static bool HasAchievement(string achievementId)
        {
            if (string.IsNullOrEmpty(achievementId))
                return false;

            string unlocked = Current.unlockedAchievements ?? "";
            if (string.IsNullOrEmpty(unlocked))
                return false;

            var parts = unlocked.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == achievementId)
                    return true;
            }

            return false;
        }

        /// <summary>Unlocks an achievement if new. Returns true when newly unlocked.</summary>
        public static bool MarkAchievement(string achievementId)
        {
            if (string.IsNullOrEmpty(achievementId) || HasAchievement(achievementId))
                return false;

            Current.unlockedAchievements = string.IsNullOrEmpty(Current.unlockedAchievements)
                ? achievementId
                : Current.unlockedAchievements + "," + achievementId;
            Save();
            return true;
        }
    }
}
