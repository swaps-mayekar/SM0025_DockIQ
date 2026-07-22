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
        private readonly List<ParcelActor> _parcels = new List<ParcelActor>();

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

            ClearParcels();
            SpawnParcel(level.VipStart, true);
            if (level.DecoyStarts != null)
            {
                foreach (var d in level.DecoyStarts)
                    SpawnParcel(d, false);
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

            for (int i = 0; i < _parcels.Count; i++)
                _parcels[i].TickVisual(_level.TickSeconds);

            if (!_running)
                return;

            _timeLeft -= Time.deltaTime;
            _hud.SetTimer(_timeLeft);
            if (_timeLeft <= 0f)
            {
                Fail("Truck departed!");
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
            // Resolve moves — VIP evaluated first for dock outcomes
            _parcels.Sort((a, b) => b.IsVip.CompareTo(a.IsVip));

            for (int i = 0; i < _parcels.Count; i++)
            {
                var parcel = _parcels[i];
                if (parcel.Arrived)
                    continue;

                var cell = _board.Get(parcel.Cell);
                if (cell.IsDock)
                {
                    ResolveDock(parcel, cell);
                    continue;
                }

                Vector2Int? next = _board.Step(parcel.Cell);
                if (next == null)
                    continue;

                parcel.BeginMove(next.Value, _board.CellToWorld(next.Value, -0.1f));

                var nextCell = _board.Get(next.Value);
                if (nextCell.IsDock)
                    ResolveDock(parcel, nextCell);
            }
        }

        private void ResolveDock(ParcelActor parcel, CellData dock)
        {
            if (!parcel.IsVip)
            {
                parcel.MarkArrived();
                return;
            }

            parcel.MarkArrived();
            if (dock.DockId == _level.TargetDockId)
                Win();
            else
                Fail($"Wrong dock ({dock.DockId})!");
        }

        private void Win()
        {
            if (_ended)
                return;
            _ended = true;
            _running = false;
            ProgressStore.MarkLevelCompleted(_level.Id);
            _hud.ShowResult(true, "Shipment rescued!", _level.Id < LevelCatalog.Count);
        }

        private void Fail(string reason)
        {
            if (_ended)
                return;
            _ended = true;
            _running = false;
            _hud.ShowResult(false, reason, false);
        }

        private void SpawnParcel(Vector2Int cell, bool vip)
        {
            if (!_board.InBounds(cell) || !_board.Get(cell).IsTraversable)
            {
                Debug.LogWarning($"Invalid parcel start {cell} on level {_level.Id}");
                return;
            }

            var go = new GameObject(vip ? "VIP" : "Parcel");
            go.transform.SetParent(transform, false);
            var actor = go.AddComponent<ParcelActor>();
            actor.Init(cell, vip, _board.CellToWorld(cell, -0.1f));
            _parcels.Add(actor);
        }

        private void ClearParcels()
        {
            for (int i = 0; i < _parcels.Count; i++)
            {
                if (_parcels[i] != null)
                    Destroy(_parcels[i].gameObject);
            }

            _parcels.Clear();
        }

        private void FitCamera()
        {
            if (_cam == null)
                return;

            IsoMath.GetBounds(_board.Width, _board.Height, out Vector2 min, out Vector2 max);
            // Bounds are local; board Origin already centers them at world 0
            float spanX = (max.x - min.x) * 0.5f + 0.6f;
            float spanY = (max.y - min.y) * 0.5f + 1.4f;

            _cam.orthographic = true;
            _cam.orthographicSize = Mathf.Max(spanY, spanX / Mathf.Max(0.1f, _cam.aspect));
            _cam.backgroundColor = PlaceholderArt.Navy;
            // Slight upward bias so HUD does not cover the board
            _cam.transform.position = new Vector3(0f, -0.15f, -10f);
        }
    }
}
