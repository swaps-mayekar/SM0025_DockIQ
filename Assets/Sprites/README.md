# DockIQ Sprite Naming Convention

View: **isometric 2D** (classic 2:1 diamond tiles). Draw art in isometric perspective — not top-down.

Drop generated 2D art into these folders. Runtime placeholders are used until files exist.
Also place copies under `Assets/Resources/Sprites/...` if you want `SpriteCatalog` to pick them up at runtime.

## Belts (`Assets/Sprites/Belts/`)
| File | Use |
|------|-----|
| `belt_straight.png` | Iso conveyor floor diamond (pivot center) |
| `belt_corner.png` | Optional corner piece |
| `belt_floor.png` | Empty warehouse floor tile |

## Devices (`Assets/Sprites/Devices/`)
| File | Use |
|------|-----|
| `switch.png` | Tap-to-rotate junction (iso) |
| `splitter.png` | Tap-to-change output lane |
| `scanner.png` | Future — label gate |
| `elevator.png` | Future — floor transfer |
| `robot_arm.png` | Future — pick/place |
| `jam_button.png` | Future — clear jam |

## Parcels (`Assets/Sprites/Parcels/`)
| File | Use |
|------|-----|
| `parcel.png` | Iso cardboard crate (decoy) |
| `parcel_vip.png` | Gold-outline VIP shipment |

## Docks (`Assets/Sprites/Docks/`)
| File | Use |
|------|-----|
| `dock.png` | Iso shipping dock bay |

## UI (`Assets/Sprites/UI/`)
| File | Use |
|------|-----|
| `banner_panel.png` | Request / HUD panel |
| `button.png` | Menu / HUD buttons |

**Recommended:** 256×128 (2:1) or 512×256 PNG diamonds for floor tiles; crates ~256×256 with transparent BG. Pivot at tile center. Code handles grid→screen projection and depth sorting.
