# DockIQ Sprite Naming Convention

View: **isometric 2D** tracks (optionally stacked floors). Tiny warehouse **robots** drive on rails while you reconfigure the network.

## Tracks (`Assets/Sprites/Belts/` or `Tracks/`)
| File | Use |
|------|-----|
| `belt_straight.png` / `track.png` | Iso track diamond |
| `switch.png` | Railway switch / turnout |
| `rotator.png` | Rotating intersection / turntable |
| `bridge_open.png` / `bridge_closed.png` | Drawbridge |
| `lift.png` | Same-floor freight lift pad |
| `elevator.png` | Cross-floor elevator |
| `reflector.png` | Mirror (reverse travel) |
| `obstacle.png` | Blocking scrap / fallen robot |
| `liftable.png` | Liftable crate |

## Robots / Parcels
| File | Use |
|------|-----|
| `Assets/UI/Parcels.png` | Authoring spritesheet (48 cargo icons, `Parcels_0`…`Parcels_47`) |
| `Assets/Resources/UI/Parcels.png` | Runtime copy loaded per level for the rescue cargo |
| `parcel.png` / `robot.png` | Fallback yard robot |
| `parcel_vip.png` / `robot_rescue.png` | Fallback rescue highlight |

Rescue cargo uses `SpriteCatalog.ParcelForLevel(levelId)` (`Parcels_0` = level 1). Decoys keep the grey placeholder robot.

## Docks / UI
| File | Use |
|------|-----|
| `dock.png` | Shipping dock / gate |
| `banner_panel.png` | Mission HUD |

## Gameplay legend (level strings)

**Tracks & basics**
- `^>v<` track · `+` switch · `R` fixed rotator · `B` bridge · `S` spawn · `1`–`9` docks

**Lifts & elevators**
- `A`/`a` same-floor lift pair 0 · `C`/`c` lift pair 1
- `E` cross-layer elevator pair 0 · `e` elevator pair 1

**New gadgets**
- `M` fixed reflector (180° reverse)
- `O` fixed obstacle (clash = fail)
- `X` liftable obstacle (tap to raise; clash when down)

**Movables** (authored in `LevelDef.Movables`, not ASCII)
- Kind `R` = moving rotator (tap cycles path; after last slot, tap rotates mode, then wraps)
- Kind `m` = moving reflector (tap cycles path)
- Kind `O` = sliding obstacle (tap cycles path; clash = fail)
- Path cells must be traversable track; piece starts at `StartIndex`

**Layers**
- `LevelDef.Layers` is `string[][]` — index 0 = ground, 1+ = upper decks (drawn with vertical offset)

**48 levels** unlock in order via `GameConstants.TotalLevels`.
