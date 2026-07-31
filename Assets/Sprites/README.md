# DockIQ Sprite Naming Convention

View: **isometric 2D** tracks (optionally stacked floors). Tiny warehouse **robots** drive on rails while you reconfigure the network.

## Tracks (`Assets/Sprites/Belts/`)
| File | Use |
|------|-----|
| `belt_straight.png` | Iso track diamond |
| `spawn_pad.png` | Spawn / launch pad |
| `direction_arrow.png` | Travel-direction overlay |
| `path_waypoint.png` | Movable-path preview marker |

## Devices (`Assets/Sprites/Devices/`)
| File | Use |
|------|-----|
| `switch.png` | Railway switch / turnout |
| `rotator.png` | Rotating intersection / turntable |
| `bridge_open.png` / `bridge_closed.png` | Drawbridge states |
| `lift.png` | Same-floor freight lift pad |
| `elevator.png` | Cross-floor elevator |
| `reflector.png` | Mirror (reverse travel) |
| `obstacle.png` | Blocking scrap / fallen robot |
| `liftable_down.png` / `liftable_up.png` | Liftable crate states |

## Robots (`Assets/Sprites/Robots/`)
| File | Use |
|------|-----|
| `robot.png` | Decoy AGV |
| `robot_rescue.png` | Rescue AGV fallback |
| `selection_ring.png` | Rescue highlight ring |

## Cargo / docks (already wired)
| File | Use |
|------|-----|
| `Assets/UI/Parcels.png` | 48 cargo icons (`Parcels_0`…`Parcels_47`) |
| `Assets/UI/Gates.png` | 4 dock gates (`Gates_0`…`Gates_3`) |
| `Assets/UI/BoardArtCatalog.asset` | All board sprites assigned on `LevelController` |

Wire / refresh via **DockIQ → Import Gameplay Sprites** or **DockIQ → Ensure Board Art References**.

## Gameplay legend (level strings)

**Tracks & basics**
- `^>v<` track · `+` switch · `R` fixed rotator · `B` bridge · `S` spawn · `1`–`9` docks

**Lifts & elevators**
- `A`/`a` same-floor lift pair 0 · `C`/`c` lift pair 1
- `E` cross-layer elevator pair 0 · `e` elevator pair 1

**Gadgets**
- `M` fixed reflector (180° reverse)
- `O` fixed obstacle (clash = fail)
- `X` liftable obstacle (tap to raise; clash when down)

**48 levels** unlock in order via `GameConstants.TotalLevels`.
