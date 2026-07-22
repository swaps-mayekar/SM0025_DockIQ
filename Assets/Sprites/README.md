# DockIQ Sprite Naming Convention

Drop generated 2D art into these folders. Runtime placeholders are used until files exist.

## Belts (`Assets/Sprites/Belts/`)
| File | Use |
|------|-----|
| `belt_straight.png` | Straight conveyor segment (arrow points up / North) |
| `belt_corner.png` | Optional corner piece |
| `belt_floor.png` | Empty warehouse floor tile |

## Devices (`Assets/Sprites/Devices/`)
| File | Use |
|------|-----|
| `switch.png` | Tap-to-rotate junction (default facing North) |
| `splitter.png` | Tap-to-change output lane |
| `scanner.png` | Future — label gate |
| `elevator.png` | Future — floor transfer |
| `robot_arm.png` | Future — pick/place |
| `jam_button.png` | Future — clear jam |

## Parcels (`Assets/Sprites/Parcels/`)
| File | Use |
|------|-----|
| `parcel.png` | Normal cardboard decoy |
| `parcel_vip.png` | Gold-outline VIP shipment |

## Docks (`Assets/Sprites/Docks/`)
| File | Use |
|------|-----|
| `dock.png` | Shipping dock bay (number overlaid in UI/text) |

## UI (`Assets/Sprites/UI/`)
| File | Use |
|------|-----|
| `banner_panel.png` | Request / HUD panel |
| `button.png` | Menu / HUD buttons |

**Recommended:** 256×256 or 512×512 PNG, transparent background, North-facing default for directional art (code rotates in 90° steps).
