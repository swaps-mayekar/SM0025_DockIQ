using UnityEngine;

namespace DockIQ.UI
{
    /// <summary>
    /// Direct references to board sprites (parcels, gates, tracks, devices, robots).
    /// </summary>
    [CreateAssetMenu(fileName = "BoardArtCatalog", menuName = "DockIQ/Board Art Catalog")]
    public sealed class BoardArtCatalog : ScriptableObject
    {
        [Header("Cargo & docks")]
        [SerializeField] private Sprite[] _parcels;
        [SerializeField] private Sprite[] _gates;
        [SerializeField] private Sprite[] _achievements;

        [Header("Tracks")]
        [SerializeField] private Sprite _track;
        [SerializeField] private Sprite _spawn;
        [SerializeField] private Sprite _directionArrow;
        [SerializeField] private Sprite _pathWaypoint;

        [Header("Devices")]
        [SerializeField] private Sprite _switch;
        [SerializeField] private Sprite _rotator;
        [SerializeField] private Sprite _rotatorLeft;
        [SerializeField] private Sprite _rotatorRight;
        [SerializeField] private Sprite _bridgeOpen;
        [SerializeField] private Sprite _bridgeClosed;
        [SerializeField] private Sprite _lift;
        [SerializeField] private Sprite _elevator;
        [SerializeField] private Sprite _reflector;
        [SerializeField] private Sprite _obstacle;
        [SerializeField] private Sprite _liftableDown;
        [SerializeField] private Sprite _liftableUp;

        [Header("Robots")]
        [SerializeField] private Sprite _robot;
        [SerializeField] private Sprite _robotRescue;
        [SerializeField] private Sprite _selectionRing;

        public Sprite ParcelForLevel(int levelId) => Slice(_parcels, levelId - 1);

        public Sprite GateForDockId(int dockId) => Slice(_gates, dockId - 1);

        /// <summary>Achievement badge (Achievements_0 = first catalog entry).</summary>
        public Sprite AchievementIcon(int index) => Slice(_achievements, index);

        public Sprite Track => _track;
        public Sprite Spawn => _spawn;
        public Sprite DirectionArrow => _directionArrow;
        public Sprite PathWaypoint => _pathWaypoint;
        public Sprite Switch => _switch;
        public Sprite Rotator => _rotator;
        public Sprite RotatorLeft => _rotatorLeft;
        public Sprite RotatorRight => _rotatorRight;
        public Sprite BridgeOpen => _bridgeOpen;
        public Sprite BridgeClosed => _bridgeClosed;
        public Sprite Lift => _lift;
        public Sprite Elevator => _elevator;
        public Sprite Reflector => _reflector;
        public Sprite Obstacle => _obstacle;
        public Sprite LiftableDown => _liftableDown;
        public Sprite LiftableUp => _liftableUp;
        public Sprite Robot => _robot;
        public Sprite RobotRescue => _robotRescue;
        public Sprite SelectionRing => _selectionRing;

        private static Sprite Slice(Sprite[] sprites, int index)
        {
            if (sprites == null || index < 0 || index >= sprites.Length)
                return null;
            return sprites[index];
        }
    }
}
