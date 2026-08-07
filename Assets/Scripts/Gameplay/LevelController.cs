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
        [SerializeField] private BoardArtCatalog _boardArt;
        [SerializeField] private UiChromeCatalog _uiChrome;

        private LevelDef _level;
        private GridBoard _board;
        private BoardView _view;
        private GameHud _hud;
        private readonly List<RobotActor> _robots = new List<RobotActor>();
        private readonly Queue<TutorialTip> _pendingTips = new Queue<TutorialTip>();

        private float _tickTimer;
        private float _timeLeft;
        private bool _running;
        private bool _ended;
        private bool _paused;
        private bool _tutorialActive;
        private Camera _cam;

        public void Begin(LevelDef level, GameHud hud)
        {
            _level = level;
            _level.EnsureOpeningRunway(5);
            _hud = hud;
            _ended = false;
            _running = true;
            _paused = false;
            _tutorialActive = false;
            _timeLeft = level.TimeLimit;
            _tickTimer = level.TickSeconds;
            _cam = Camera.main;
            _pendingTips.Clear();

            SpriteCatalog.Bind(_boardArt);
            if (_uiChrome != null)
                UiChrome.Bind(_uiChrome);

            _board = new GridBoard();
            _board.Build(level.ResolveLayers(), level.Movables, GameConstants.DefaultCellSize);

            if (_view == null)
                _view = gameObject.AddComponent<BoardView>();
            _view.Build(_board);

            ClearRobots();
            SpawnRobot(level.RobotCoord, level.RobotFacing, true, level.RobotCallsign);
            if (level.DecoyStarts != null)
            {
                for (int i = 0; i < level.DecoyStarts.Length; i++)
                {
                    Dir face = level.DecoyFacings != null && i < level.DecoyFacings.Length
                        ? level.DecoyFacings[i]
                        : Dir.East;
                    SpawnRobot(level.DecoyCoord(i), face, false, $"#X{i + 1}");
                }
            }

            FitCamera();
            _hud.ShowRequest(level.RequestText, level.Title);
            _hud.SetTimer(_timeLeft);
            _hud.HideResult();
            _hud.ConfigurePause(OnPaused, OnResumed, SceneRouter.ReloadGame, SceneRouter.LoadMenu);
            BeginTutorials(level);
        }

        private void Update()
        {
            if (_ended)
                return;

            if (_paused || _tutorialActive)
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
            if (!_board.TryWorldToCell(world, out CellCoord cell))
                return;

            var data = _board.Get(cell);

            if (data.MovableId >= 0)
            {
                if (_board.TryAdvanceMovable(data.MovableId, IsRobotOn))
                    _view.Build(_board);
                else
                    _view.RefreshDevices();
                return;
            }

            if (!data.IsInteractive)
                return;

            data.OnTap();
            _view.RefreshDevices();
        }

        private bool IsRobotOn(CellCoord c)
        {
            for (int i = 0; i < _robots.Count; i++)
            {
                var r = _robots[i];
                if (!r.Arrived && r.Coord == c)
                    return true;
            }

            return false;
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

                if (robot.PadHoldTicks > 0)
                {
                    robot.PadHoldTicks--;
                    continue;
                }

                var cell = _board.Get(robot.Coord);
                if (cell.IsDock)
                {
                    ResolveDock(robot, cell);
                    continue;
                }

                if (!_board.TryStep(robot.Coord, robot.Facing, out CellCoord next, out Dir newFacing,
                        out bool clash, robot.SuppressLift))
                {
                    continue;
                }

                if (clash)
                {
                    Fail("Collision in the yard!");
                    return;
                }

                bool usedTransfer = (cell.IsLift || cell.IsElevator) && !robot.SuppressLift;

                if (next == robot.Coord)
                {
                    if (usedTransfer)
                        robot.SuppressLift = true;
                    continue;
                }

                if (usedTransfer)
                {
                    // Linger on the far pad so the teleport reads clearly (esp. elevators).
                    robot.SuppressLift = true;
                    robot.PadHoldTicks = cell.IsElevator ? 1 : 0;
                }
                else
                {
                    robot.SuppressLift = false;
                }

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
            _paused = false;
            _tutorialActive = false;
            _pendingTips.Clear();
            if (GameSession.IsStory)
                ProgressStore.MarkLevelCompleted(_level.Id);

            AchievementStore.EvaluateOnWin(_level, _timeLeft, GameSession.Mode);

            bool hasNext = GameSession.IsStory
                ? _level.Id < LevelCatalog.Count
                : _level.Id < LevelCatalog.Count && ProgressStore.IsUnlocked(_level.Id + 1);
            _hud.ShowResult(true, $"{_level.RobotCallsign} reached {_level.DockName}!", hasNext);
        }

        private void Fail(string reason)
        {
            if (_ended)
                return;
            _ended = true;
            _running = false;
            _paused = false;
            _tutorialActive = false;
            _pendingTips.Clear();
            _hud.ShowResult(false, reason, false);
        }

        private void BeginTutorials(LevelDef level)
        {
            _pendingTips.Clear();

            if (GameSession.IsStory)
            {
                var intro = StoryBriefingCatalog.TryGetCampaignIntro();
                if (intro.HasValue)
                    _pendingTips.Enqueue(intro.Value);

                _pendingTips.Enqueue(StoryBriefingCatalog.GetLevelBriefing(level));
            }

            var tips = TutorialTipCatalog.GetPendingTips(level);
            for (int i = 0; i < tips.Count; i++)
                _pendingTips.Enqueue(tips[i]);

            if (_pendingTips.Count > 0)
                ShowNextTutorial();
        }

        private void ShowNextTutorial()
        {
            if (_ended || _pendingTips.Count == 0)
            {
                EndTutorialFlow();
                return;
            }

            var tip = _pendingTips.Dequeue();
            _tutorialActive = true;
            if (!_hud.ShowTutorial(tip.Title, tip.Body, tip.Id, () => OnTutorialDismissed(tip.Id)))
                EndTutorialFlow();
        }

        private void OnTutorialDismissed(string tipId)
        {
            // Level briefings re-show every Story start; campaign intro & mechanic tips persist.
            if (!StoryBriefingCatalog.IsBriefingTip(tipId))
                TutorialTipCatalog.MarkDismissed(tipId);

            if (_ended)
            {
                EndTutorialFlow();
                return;
            }

            if (_pendingTips.Count > 0)
                ShowNextTutorial();
            else
                EndTutorialFlow();
        }

        private void EndTutorialFlow()
        {
            _tutorialActive = false;
            _pendingTips.Clear();
            if (!_ended)
                _hud.HideTutorial();
        }

        private void OnPaused()
        {
            if (_ended || _tutorialActive)
                return;
            _paused = true;
        }

        private void OnResumed()
        {
            if (_ended || _tutorialActive)
                return;
            _paused = false;
        }

        private void SpawnRobot(CellCoord cell, Dir facing, bool rescue, string callsign)
        {
            if (!_board.InBounds(cell) || !_board.Get(cell).IsTraversable)
            {
                Debug.LogWarning($"Invalid robot start {cell} on level {_level.Id}");
                return;
            }

            var data = _board.Get(cell);
            if (data.Type == CellType.Spawn || data.Type == CellType.Track)
                facing = data.Facing;

            var go = new GameObject(rescue ? $"Robot_{callsign}" : $"Decoy_{callsign}");
            go.transform.SetParent(transform, false);
            var actor = go.AddComponent<RobotActor>();
            actor.Init(cell, facing, rescue, callsign, _board.CellToWorld(cell, -0.1f),
                rescue ? _level.Id : 0);
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

            IsoMath.GetBounds(_board.Width, _board.Height, _board.LayerCount, out Vector2 min, out Vector2 max);
            float spanX = (max.x - min.x) * 0.5f + 0.6f;
            float spanY = (max.y - min.y) * 0.5f + 1.4f;

            _cam.orthographic = true;
            _cam.orthographicSize = Mathf.Max(spanY, spanX / Mathf.Max(0.1f, _cam.aspect));
            _cam.backgroundColor = PlaceholderArt.Navy;
            _cam.transform.position = new Vector3(0f, -0.15f, -10f);
        }
    }
}
