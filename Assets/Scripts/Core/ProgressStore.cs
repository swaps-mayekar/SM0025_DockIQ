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
    }
}
