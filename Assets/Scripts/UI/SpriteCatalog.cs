using UnityEngine;

namespace DockIQ.UI
{
    /// <summary>
    /// Board art lookups via a scene-bound <see cref="BoardArtCatalog"/> (<c>Assets/UI</c>).
    /// </summary>
    public static class SpriteCatalog
    {
        private static BoardArtCatalog _boardArt;

        public static void Bind(BoardArtCatalog boardArt) => _boardArt = boardArt;

        public static Sprite TrackOrFallback() =>
            _boardArt != null && _boardArt.Track != null
                ? _boardArt.Track
                : PlaceholderArt.IsoDiamond();

        public static Sprite SpawnOrFallback() =>
            _boardArt != null && _boardArt.Spawn != null
                ? _boardArt.Spawn
                : TrackOrFallback();

        public static Sprite SwitchOrFallback() =>
            _boardArt != null && _boardArt.Switch != null
                ? _boardArt.Switch
                : PlaceholderArt.IsoDiamond();

        public static Sprite RotatorOrFallback() =>
            _boardArt != null && _boardArt.Rotator != null
                ? _boardArt.Rotator
                : PlaceholderArt.IsoDiamond();

        public static Sprite RotatorForModeOrFallback(int mode)
        {
            int normalized = ((mode % 3) + 3) % 3;
            if (_boardArt != null)
            {
                if (normalized == 1 && _boardArt.RotatorLeft != null)
                    return _boardArt.RotatorLeft;
                if (normalized == 2 && _boardArt.RotatorRight != null)
                    return _boardArt.RotatorRight;
                if (_boardArt.Rotator != null)
                    return _boardArt.Rotator;
            }

            return RotatorOrFallback();
        }

        public static Sprite BridgeOrFallback(bool open)
        {
            if (_boardArt != null)
            {
                Sprite art = open ? _boardArt.BridgeOpen : _boardArt.BridgeClosed;
                if (art != null)
                    return art;
                art = _boardArt.BridgeClosed ?? _boardArt.BridgeOpen;
                if (art != null)
                    return art;
            }

            return PlaceholderArt.IsoDiamond();
        }

        public static Sprite LiftOrFallback() =>
            _boardArt != null && _boardArt.Lift != null
                ? _boardArt.Lift
                : PlaceholderArt.IsoDiamond();

        public static Sprite ElevatorOrFallback() =>
            _boardArt != null && _boardArt.Elevator != null
                ? _boardArt.Elevator
                : PlaceholderArt.IsoDiamond();

        public static Sprite ReflectorOrFallback() =>
            _boardArt != null && _boardArt.Reflector != null
                ? _boardArt.Reflector
                : PlaceholderArt.IsoDiamond();

        public static Sprite ObstacleOrFallback() =>
            _boardArt != null && _boardArt.Obstacle != null
                ? _boardArt.Obstacle
                : PlaceholderArt.IsoDiamond();

        public static Sprite LiftableOrFallback(bool raised)
        {
            if (_boardArt != null)
            {
                Sprite art = raised ? _boardArt.LiftableUp : _boardArt.LiftableDown;
                if (art != null)
                    return art;
                art = _boardArt.LiftableDown ?? _boardArt.LiftableUp;
                if (art != null)
                    return art;
            }

            return PlaceholderArt.IsoDiamond();
        }

        public static Sprite DirectionArrowOrFallback() =>
            _boardArt != null && _boardArt.DirectionArrow != null
                ? _boardArt.DirectionArrow
                : PlaceholderArt.WhiteSquare();

        public static Sprite PathWaypointOrFallback() =>
            _boardArt != null && _boardArt.PathWaypoint != null
                ? _boardArt.PathWaypoint
                : PlaceholderArt.Circle();

        public static Sprite SelectionRingOrFallback() =>
            _boardArt != null && _boardArt.SelectionRing != null
                ? _boardArt.SelectionRing
                : PlaceholderArt.WhiteSquare();

        public static Sprite RobotOrFallback(bool rescue)
        {
            if (_boardArt != null)
            {
                Sprite art = rescue ? _boardArt.RobotRescue : _boardArt.Robot;
                if (art != null)
                    return art;
                if (_boardArt.Robot != null)
                    return _boardArt.Robot;
            }

            return PlaceholderArt.RobotBody();
        }

        public static Sprite DockOrFallback(int dockId = 1) =>
            GateForDockId(dockId) ?? PlaceholderArt.IsoDiamond();

        /// <summary>Level cargo sprite (Parcels_0 = level 1) from the bound board art catalog.</summary>
        public static Sprite ParcelForLevel(int levelId) =>
            _boardArt != null ? _boardArt.ParcelForLevel(levelId) : null;

        /// <summary>Dock gate sprite (Gates_0 = dock 1) from the bound board art catalog.</summary>
        public static Sprite GateForDockId(int dockId) =>
            _boardArt != null ? _boardArt.GateForDockId(dockId) : null;

        /// <summary>Achievement badge (Achievements_0 = first catalog entry).</summary>
        public static Sprite AchievementIcon(int index) =>
            _boardArt != null ? _boardArt.AchievementIcon(index) : null;

        /// <summary>True when the sprite is painted production art (not a procedural placeholder).</summary>
        public static bool IsProductionArt(Sprite sprite)
        {
            if (sprite == null)
                return false;
            string n = sprite.name;
            return n != "PlaceholderIsoDiamond"
                   && n != "PlaceholderWhite"
                   && n != "PlaceholderCircle"
                   && n != "PlaceholderRobot";
        }

        /// <summary>World scale so sprite width matches <paramref name="targetWorldWidth"/>.</summary>
        public static float FitWidthScale(Sprite sprite, float targetWorldWidth)
        {
            if (sprite == null || sprite.rect.width <= 0.01f)
                return 1f;
            return targetWorldWidth * sprite.pixelsPerUnit / sprite.rect.width;
        }
    }
}
