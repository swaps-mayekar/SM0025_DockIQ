using System.Collections.Generic;
using DockIQ.Board;
using DockIQ.Core;
using DockIQ.Levels;
using DockIQ.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DockIQ.Gameplay
{
    public sealed class LevelController : MonoBehaviour
    {
        private LevelDef _level;
        private GridBoard _board;
        private BoardView _view;
        private GameHud _hud;
        private readonly List<RobotActor> _robots = new List<RobotActor>();

        private float _tickTimer;
        private float _timeLeft;
        private bool _running;
        private bool _ended;
        private Camera _cam;

        public void Begin(LevelDef level, GameHud hud)
        {
            _level = level;
            _hud = hud;
            _ended = false;
            _running = true;
            _timeLeft = level.TimeLimit;
            _tickTimer = level.TickSeconds;
            _cam = Camera.main;

            _board = new GridBoard();
            _board.Build(level.Rows, GameConstants.DefaultCellSize);

            if (_view == null)
                _view = gameObject.AddComponent<BoardView>();
            _view.Build(_board);

            ClearRobots();
            SpawnRobot(level.RobotStart, level.RobotFacing, true, level.RobotCallsign);
            if (level.DecoyStarts != null)
            {
                for (int i = 0; i < level.DecoyStarts.Length; i++)
                {
                    Dir face = level.DecoyFacings != null && i < level.DecoyFacings.Length
                        ? level.DecoyFacings[i]
                        : Dir.East;
                    SpawnRobot(level.DecoyStarts[i], face, false, $"#X{i + 1}");
                }
            }

            FitCamera();
            _hud.ShowRequest(level.RequestText, level.Title);
            _hud.SetTimer(_timeLeft);
            _hud.HideResult();
        }

        private void Update()
        {
            if (_ended)
                return;

            HandleTap();

            for (int i = 0; i < _robots.Count; i++)
                _robots[i].TickVisual(_level.TickSeconds);

            if (!_running)
                return;

            _timeLeft -= Time.deltaTime;
            _hud.SetTimer(_timeLeft);
            if (_timeLeft <= 0f)
            {
                Fail("Departure window closed!");
                return;
            }

            _tickTimer -= Time.deltaTime;
            if (_tickTimer <= 0f)
            {
                _tickTimer += _level.TickSeconds;
                StepSimulation();
            }
        }

        private void HandleTap()
        {
            if (!TryGetTapWorld(out Vector3 world))
                return;
            if (!_board.TryWorldToCell(world, out Vector2Int cell))
                return;

            var data = _board.Get(cell);
            if (!data.IsInteractive)
                return;

            data.OnTap();
            _view.RefreshDevices();
        }

        private bool TryGetTapWorld(out Vector3 world)
        {
            world = default;
            if (_cam == null)
                _cam = Camera.main;
            if (_cam == null)
                return false;

            bool pressed = false;
            Vector2 screen = default;

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                pressed = true;
                screen = mouse.position.ReadValue();
            }

            var touch = Touchscreen.current;
            if (!pressed && touch != null && touch.primaryTouch.press.wasPressedThisFrame)
            {
                pressed = true;
                screen = touch.primaryTouch.position.ReadValue();
            }

            if (!pressed)
                return false;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return false;

            world = _cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -_cam.transform.position.z));
            world.z = 0f;
            return true;
        }

        private void StepSimulation()
        {
            _robots.Sort((a, b) => b.IsRescue.CompareTo(a.IsRescue));

            for (int i = 0; i < _robots.Count; i++)
            {
                var robot = _robots[i];
                if (robot.Arrived)
                    continue;

                var cell = _board.Get(robot.Cell);
                if (cell.IsDock)
                {
                    ResolveDock(robot, cell);
                    continue;
                }

                if (!_board.TryStep(robot.Cell, robot.Facing, out Vector2Int next, out Dir newFacing,
                        robot.SuppressLift))
                {
                    // Rescue robot stuck on closed bridge / dead end — keep waiting
                    continue;
                }

                bool stayedOnPad = next == robot.Cell;
                bool usedLift = _board.Get(robot.Cell).IsLift && !robot.SuppressLift;

                if (stayedOnPad)
                {
                    // Landed on arrival lift with no exit — don't re-teleport next tick.
                    if (usedLift)
                        robot.SuppressLift = true;
                    continue;
                }

                // Left a lift pad (or any cell) — clear suppress.
                robot.SuppressLift = false;
                robot.BeginMove(next, newFacing, _board.CellToWorld(next, -0.1f));

                var nextCell = _board.Get(next);
                if (nextCell.IsDock)
                    ResolveDock(robot, nextCell);
            }
        }

        private void ResolveDock(RobotActor robot, CellData dock)
        {
            if (!robot.IsRescue)
            {
                robot.MarkArrived();
                return;
            }

            robot.MarkArrived();
            if (dock.DockId == _level.TargetDockId)
                Win();
            else
                Fail($"Wrong gate — needed {_level.DockName}!");
        }

        private void Win()
        {
            if (_ended)
                return;
            _ended = true;
            _running = false;
            ProgressStore.MarkLevelCompleted(_level.Id);
            _hud.ShowResult(true, $"{_level.RobotCallsign} reached {_level.DockName}!", _level.Id < LevelCatalog.Count);
        }

        private void Fail(string reason)
        {
            if (_ended)
                return;
            _ended = true;
            _running = false;
            _hud.ShowResult(false, reason, false);
        }

        private void SpawnRobot(Vector2Int cell, Dir facing, bool rescue, string callsign)
        {
            if (!_board.InBounds(cell) || !_board.Get(cell).IsTraversable)
            {
                Debug.LogWarning($"Invalid robot start {cell} on level {_level.Id}");
                return;
            }

            // Prefer authored facing on spawn/track cells
            var data = _board.Get(cell);
            if (data.Type == CellType.Spawn || data.Type == CellType.Track)
                facing = data.Facing;

            var go = new GameObject(rescue ? $"Robot_{callsign}" : $"Decoy_{callsign}");
            go.transform.SetParent(transform, false);
            var actor = go.AddComponent<RobotActor>();
            actor.Init(cell, facing, rescue, callsign, _board.CellToWorld(cell, -0.1f));
            _robots.Add(actor);
        }

        private void ClearRobots()
        {
            for (int i = 0; i < _robots.Count; i++)
            {
                if (_robots[i] != null)
                    Destroy(_robots[i].gameObject);
            }

            _robots.Clear();
        }

        private void FitCamera()
        {
            if (_cam == null)
                return;

            IsoMath.GetBounds(_board.Width, _board.Height, out Vector2 min, out Vector2 max);
            float spanX = (max.x - min.x) * 0.5f + 0.6f;
            float spanY = (max.y - min.y) * 0.5f + 1.4f;

            _cam.orthographic = true;
            _cam.orthographicSize = Mathf.Max(spanY, spanX / Mathf.Max(0.1f, _cam.aspect));
            _cam.backgroundColor = PlaceholderArt.Navy;
            _cam.transform.position = new Vector3(0f, -0.15f, -10f);
        }
    }
}
