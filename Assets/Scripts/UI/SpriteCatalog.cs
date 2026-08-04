using UnityEngine;

namespace DockIQ.UI
{
    /// <summary>
    /// Board art lookups. Prefers a scene-bound <see cref="BoardArtCatalog"/>, then Resources.
    /// </summary>
    public static class SpriteCatalog
    {
        private static BoardArtCatalog _boardArt;

        public static void Bind(BoardArtCatalog boardArt) => _boardArt = boardArt;

        public static Sprite Load(string resourcesPath)
        {
            if (string.IsNullOrEmpty(resourcesPath))
                return null;
            return Resources.Load<Sprite>(resourcesPath);
        }

        public static Sprite TrackOrFallback() =>
            _boardArt != null && _boardArt.Track != null
                ? _boardArt.Track
                : Load("Sprites/Belts/belt_straight") ?? Load("Sprites/Tracks/track") ?? PlaceholderArt.IsoDiamond();

        public static Sprite SpawnOrFallback() =>
            _boardArt != null && _boardArt.Spawn != null
                ? _boardArt.Spawn
                : Load("Sprites/Belts/spawn_pad") ?? TrackOrFallback();

        public static Sprite SwitchOrFallback() =>
            _boardArt != null && _boardArt.Switch != null
                ? _boardArt.Switch
                : Load("Sprites/Devices/switch") ?? PlaceholderArt.IsoDiamond();

        public static Sprite RotatorOrFallback() =>
            _boardArt != null && _boardArt.Rotator != null
                ? _boardArt.Rotator
                : Load("Sprites/Devices/rotator") ?? PlaceholderArt.IsoDiamond();

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

            return normalized switch
            {
                1 => Load("Sprites/Devices/rotator_left"),
                2 => Load("Sprites/Devices/rotator_right"),
                _ => Load("Sprites/Devices/rotator_straight")
            } ?? RotatorOrFallback();
        }

        public static Sprite BridgeOrFallback(bool open)
        {
            if (_boardArt != null)
            {
                Sprite art = open ? _boardArt.BridgeOpen : _boardArt.BridgeClosed;
                if (art != null)
                    return art;
                // Prefer closed as generic bridge if only one state is missing.
                art = _boardArt.BridgeClosed ?? _boardArt.BridgeOpen;
                if (art != null)
                    return art;
            }

            return Load(open ? "Sprites/Devices/bridge_open" : "Sprites/Devices/bridge_closed")
                   ?? Load("Sprites/Devices/bridge")
                   ?? PlaceholderArt.IsoDiamond();
        }

        public static Sprite LiftOrFallback() =>
            _boardArt != null && _boardArt.Lift != null
                ? _boardArt.Lift
                : Load("Sprites/Devices/lift") ?? PlaceholderArt.IsoDiamond();

        public static Sprite ElevatorOrFallback() =>
            _boardArt != null && _boardArt.Elevator != null
                ? _boardArt.Elevator
                : Load("Sprites/Devices/elevator") ?? PlaceholderArt.IsoDiamond();

        public static Sprite ReflectorOrFallback() =>
            _boardArt != null && _boardArt.Reflector != null
                ? _boardArt.Reflector
                : Load("Sprites/Devices/reflector") ?? PlaceholderArt.IsoDiamond();

        public static Sprite ObstacleOrFallback() =>
            _boardArt != null && _boardArt.Obstacle != null
                ? _boardArt.Obstacle
                : Load("Sprites/Devices/obstacle") ?? PlaceholderArt.IsoDiamond();

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

            return Load(raised ? "Sprites/Devices/liftable_up" : "Sprites/Devices/liftable_down")
                   ?? Load("Sprites/Devices/liftable")
                   ?? PlaceholderArt.IsoDiamond();
        }

        public static Sprite DirectionArrowOrFallback() =>
            _boardArt != null && _boardArt.DirectionArrow != null
                ? _boardArt.DirectionArrow
                : Load("Sprites/Belts/direction_arrow") ?? PlaceholderArt.WhiteSquare();

        public static Sprite PathWaypointOrFallback() =>
            _boardArt != null && _boardArt.PathWaypoint != null
                ? _boardArt.PathWaypoint
                : Load("Sprites/Belts/path_waypoint") ?? PlaceholderArt.Circle();

        public static Sprite SelectionRingOrFallback() =>
            _boardArt != null && _boardArt.SelectionRing != null
                ? _boardArt.SelectionRing
                : Load("Sprites/Robots/selection_ring") ?? PlaceholderArt.WhiteSquare();

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

            return Load(rescue ? "Sprites/Parcels/parcel_vip" : "Sprites/Parcels/parcel")
                   ?? Load(rescue ? "Sprites/Robots/robot_rescue" : "Sprites/Robots/robot")
                   ?? PlaceholderArt.RobotBody();
        }

        public static Sprite DockOrFallback(int dockId = 1) =>
            GateForDockId(dockId)
            ?? Load("Sprites/Docks/dock")
            ?? PlaceholderArt.IsoDiamond();

        /// <summary>Level cargo sprite (Parcels_0 = level 1) from the bound board art catalog.</summary>
        public static Sprite ParcelForLevel(int levelId) =>
            _boardArt != null ? _boardArt.ParcelForLevel(levelId) : null;

        /// <summary>Dock gate sprite (Gates_0 = dock 1) from the bound board art catalog.</summary>
        public static Sprite GateForDockId(int dockId) =>
            _boardArt != null ? _boardArt.GateForDockId(dockId) : null;

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
