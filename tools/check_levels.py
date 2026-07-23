#!/usr/bin/env python3
"""Fast static-config solvability checker for DockIQ levels."""
from __future__ import annotations
from pathlib import Path
from itertools import product
import re
import copy

N, E, S, W = 0, 1, 2, 3
OFF = {N: (0, 1), E: (1, 0), S: (0, -1), W: (-1, 0)}
OPP = {N: S, E: W, S: N, W: E}
CW = {N: E, E: S, S: W, W: N}
CCW = {N: W, E: N, S: E, W: S}


def parse_catalog():
    text = "\n".join(p.read_text() for p in Path("Assets/Scripts/Levels").glob("LevelCatalog*.cs"))
    blocks = re.split(r"(?:add|Add)\(new LevelDef", text)[1:]
    levels = []
    for block in blocks:
        idm = re.search(r"Id\s*=\s*(\d+)", block)
        if not idm:
            continue
        lid = int(idm.group(1))
        target = int(re.search(r"TargetDockId\s*=\s*(\d+)", block).group(1))
        start = re.search(r"RobotStart\s*=\s*new Vector2Int\((\d+)\s*,\s*(\d+)\)", block)
        layer_m = re.search(r"RobotLayer\s*=\s*(\d+)", block)
        rlayer = int(layer_m.group(1)) if layer_m else 0
        face_m = re.search(r"RobotFacing\s*=\s*Dir\.(\w+)", block)
        facing = {"North": N, "East": E, "South": S, "West": W}.get(
            face_m.group(1) if face_m else "East", E)
        time_m = re.search(r"TimeLimit\s*=\s*([\d.]+)f", block)
        tick_m = re.search(r"TickSeconds\s*=\s*([\d.]+)f", block)
        time_limit = float(time_m.group(1)) if time_m else 40
        tick = float(tick_m.group(1)) if tick_m else 0.45
        max_ticks = int(time_limit / tick) + 5

        layers = []
        for l0 in re.finditer(r"L0\(\s*((?:\"[^\"]+\"\s*,?\s*)+)\)", block):
            rows = re.findall(r"\"([^\"]+)\"", l0.group(1))
            layers = [rows]
        for l2 in re.finditer(r"L2\(\s*new\[\]\s*\{([^}]+)\}\s*,\s*new\[\]\s*\{([^}]+)\}", block):
            g = re.findall(r"\"([^\"]+)\"", l2.group(1))
            u = re.findall(r"\"([^\"]+)\"", l2.group(2))
            layers = [g, u]

        movables = []
        for m in re.finditer(
                r"Mov\('([^']+)',\s*(\d+)(?:,\s*(\d+))?,\s*((?:P\([^)]+\)\s*,?\s*)+)\)", block):
            kind = m.group(1)
            start_i = int(m.group(2))
            mode = int(m.group(3) or 0)
            path = []
            for p in re.findall(r"P\((\d+)\s*,\s*(\d+)(?:\s*,\s*(\d+))?\)", m.group(4)):
                path.append((int(p[0]), int(p[1]), int(p[2]) if p[2] else 0))
            movables.append({"kind": kind, "start": start_i, "mode": mode, "path": path})

        levels.append({
            "id": lid, "target": target,
            "start": (int(start.group(1)), int(start.group(2)), rlayer),
            "facing": facing, "max_ticks": max_ticks,
            "layers": layers, "movables": movables,
        })
    return levels


class Cell:
    __slots__ = ("typ", "facing", "dock", "lift_target", "elev_target",
                 "switch_face", "rot_mode", "bridge_open", "liftable_up", "movable_id")

    def __init__(self):
        self.typ = "empty"
        self.facing = E
        self.dock = 0
        self.lift_target = None
        self.elev_target = None
        self.switch_face = E
        self.rot_mode = 0
        self.bridge_open = False
        self.liftable_up = False
        self.movable_id = -1


def build_base(level):
    layers_ascii = level["layers"]
    Lc = len(layers_ascii)
    H = len(layers_ascii[0])
    W = len(layers_ascii[0][0])
    grid = [[[Cell() for _ in range(H)] for _ in range(W)] for _ in range(Lc)]
    lifts, elevs = {}, {}
    devices = []  # (kind, x, y, L)

    for L, rows in enumerate(layers_ascii):
        for yi, row in enumerate(rows):
            y = H - 1 - yi
            for x, ch in enumerate(row):
                c = grid[L][x][y]
                if ch in ". ":
                    continue
                if ch in "^>v<":
                    c.typ = "track"
                    c.facing = {"^": N, ">": E, "v": S, "<": W}[ch]
                elif ch == "+":
                    c.typ = "switch"
                    devices.append(("switch", x, y, L))
                elif ch == "R":
                    c.typ = "rotator"
                    devices.append(("rotator", x, y, L))
                elif ch == "B":
                    c.typ = "bridge"
                    devices.append(("bridge", x, y, L))
                elif ch in "Aa":
                    c.typ = "lift"
                    lifts.setdefault(0, []).append((x, y, L))
                elif ch in "Cc":
                    c.typ = "lift"
                    lifts.setdefault(1, []).append((x, y, L))
                elif ch == "E":
                    c.typ = "elev"
                    elevs.setdefault(0, []).append((x, y, L))
                elif ch == "e":
                    c.typ = "elev"
                    elevs.setdefault(1, []).append((x, y, L))
                elif ch == "M":
                    c.typ = "reflector"
                elif ch == "X":
                    c.typ = "liftable"
                    devices.append(("liftable", x, y, L))
                elif ch == "O":
                    c.typ = "obstacle"
                elif ch in "Ss":
                    c.typ = "spawn"
                    c.facing = E
                elif ch.isdigit():
                    c.typ = "dock"
                    c.dock = int(ch)

    for pads in lifts.values():
        if len(pads) == 2:
            a, b = pads
            grid[a[2]][a[0]][a[1]].lift_target = b
            grid[b[2]][b[0]][b[1]].lift_target = a
    for pads in elevs.values():
        if len(pads) == 2:
            a, b = pads
            grid[a[2]][a[0]][a[1]].elev_target = b
            grid[b[2]][b[0]][b[1]].elev_target = a

    return grid, W, H, Lc, devices


def place_movables(grid, movables, indices_modes):
    """indices_modes: list of (path_idx, rot_mode) per movable."""
    # clear previous movable marks — operate on fresh clone
    for i, m in enumerate(movables):
        idx, mode = indices_modes[i]
        x, y, L = m["path"][idx]
        c = grid[L][x][y]
        kind = m["kind"]
        if kind in "Rr":
            c.typ = "rotator"
            c.rot_mode = mode
        elif kind in "mM":
            c.typ = "reflector"
        else:
            c.typ = "obstacle"
        c.movable_id = i


def apply_device_config(grid, devices, config):
    for (kind, x, y, L), val in zip(devices, config):
        c = grid[L][x][y]
        if kind == "switch":
            c.switch_face = val
        elif kind == "rotator":
            c.rot_mode = val
        elif kind == "bridge":
            c.bridge_open = bool(val)
        elif kind == "liftable":
            c.liftable_up = bool(val)


def resolve_exit(c, entry):
    if c.typ == "switch":
        return True, c.switch_face
    if c.typ == "rotator":
        if c.rot_mode == 1:
            return True, CCW[entry]
        if c.rot_mode == 2:
            return True, CW[entry]
        return True, entry
    if c.typ == "bridge":
        return c.bridge_open, entry
    if c.typ == "reflector":
        return True, OPP[entry]
    if c.typ == "liftable":
        return c.liftable_up, entry
    if c.typ == "obstacle":
        return False, entry
    return True, entry


def is_clash(c):
    return c.typ == "obstacle" or (c.typ == "liftable" and not c.liftable_up)


def inb(W, H, Lc, p):
    x, y, L = p
    return 0 <= L < Lc and 0 <= x < W and 0 <= y < H


def can_enter(grid, W, H, Lc, p):
    if not inb(W, H, Lc, p):
        return False
    c = grid[p[2]][p[0]][p[1]]
    if c.typ == "empty":
        return False
    if c.typ == "bridge" and not c.bridge_open:
        return False
    return True


def try_step(grid, W, H, Lc, pos, facing, suppress=False):
    c = grid[pos[2]][pos[0]][pos[1]]
    if c.typ in ("empty", "dock"):
        return None

    def transfer(target):
        if target is None or not inb(W, H, Lc, target):
            return None
        tc = grid[target[2]][target[0]][target[1]]
        if tc.typ == "empty":
            return None
        dx, dy = OFF[facing]
        after = (target[0] + dx, target[1] + dy, target[2])
        if can_enter(grid, W, H, Lc, after):
            ac = grid[after[2]][after[0]][after[1]]
            if ac.typ not in ("lift", "elev"):
                if is_clash(ac):
                    return ("clash", after, facing)
                return ("ok", after, facing)
        if is_clash(tc):
            return ("clash", target, facing)
        return ("stay", target, facing)

    if not suppress and c.typ == "elev" and c.elev_target:
        return transfer(c.elev_target)
    if not suppress and c.typ == "lift" and c.lift_target:
        return transfer(c.lift_target)

    ok, exit_d = resolve_exit(c, facing)
    if not ok:
        return None
    dx, dy = OFF[exit_d]
    nxt = (pos[0] + dx, pos[1] + dy, pos[2])
    if not can_enter(grid, W, H, Lc, nxt):
        return None
    nc = grid[nxt[2]][nxt[0]][nxt[1]]
    if is_clash(nc):
        return ("clash", nxt, exit_d)
    return ("ok", nxt, exit_d)


def simulate(level, grid, W, H, Lc):
    sx, sy, sL = level["start"]
    facing = level["facing"]
    sc = grid[sL][sx][sy]
    if sc.typ in ("spawn", "track"):
        facing = sc.facing
    pos = (sx, sy, sL)
    suppress = False
    target = level["target"]
    seen_loop = set()

    for t in range(level["max_ticks"]):
        c = grid[pos[2]][pos[0]][pos[1]]
        if c.typ == "dock":
            return c.dock == target
        key = (pos, facing, suppress)
        if key in seen_loop:
            return False
        seen_loop.add(key)

        step = try_step(grid, W, H, Lc, pos, facing, suppress)
        if step is None:
            return False
        kind, nxt, nf = step
        if kind == "clash":
            return False
        stayed = nxt == pos
        used = (c.typ in ("lift", "elev")) and not suppress
        if stayed:
            if used:
                suppress = True
            else:
                return False  # stuck
        else:
            suppress = False
            pos = nxt
            facing = nf
            nc = grid[pos[2]][pos[0]][pos[1]]
            if nc.typ == "dock":
                return nc.dock == target
    return False


def device_domains(devices):
    domains = []
    for kind, *_ in devices:
        if kind == "switch":
            domains.append([N, E, S, W])
        elif kind == "rotator":
            domains.append([0, 1, 2])
        elif kind in ("bridge", "liftable"):
            domains.append([0, 1])
        else:
            domains.append([0])
    return domains


def movable_domains(movables):
    domains = []
    for m in movables:
        path_len = len(m["path"])
        if m["kind"] in "Rr":
            domains.append([(i, mode) for i in range(path_len) for mode in range(3)])
        else:
            domains.append([(i, 0) for i in range(path_len)])
    return domains


def solve(level):
    base, W, H, Lc, devices = build_base(level)
    movables = level["movables"]
    ddom = device_domains(devices)
    mdom = movable_domains(movables)

    # Cap configs
    n_dev = 1
    for d in ddom:
        n_dev *= len(d)
    n_mov = 1
    for d in mdom:
        n_mov *= len(d)
    total = n_dev * n_mov
    if total > 500_000:
        return False, f"too many configs {total}"

    dev_iter = product(*ddom) if ddom else [()]
    mov_iter = product(*mdom) if mdom else [()]

    checked = 0
    for dcfg in (product(*ddom) if ddom else [()]):
        for mcfg in (product(*mdom) if mdom else [()]):
            checked += 1
            grid = copy.deepcopy(base)
            # clear movable overlays from base paths (they may be track)
            # base doesn't have movables placed
            if mcfg:
                # ensure path cells that were overwritten in a previous sense — fresh deepcopy each time
                place_movables(grid, movables, mcfg)
            if dcfg:
                apply_device_config(grid, devices, dcfg)
            # Fixed rotators in devices may conflict with movable on same cell — rare
            if simulate(level, grid, W, H, Lc):
                return True, f"ok configs_checked={checked}"
    return False, f"exhausted {checked}"


def main():
    import os
    os.chdir("/Users/swapnil/Desktop/AppleGames/DockIQ/SM0025_DockIQ")
    levels = parse_catalog()
    levels.sort(key=lambda x: x["id"])
    print(f"Parsed {len(levels)} levels")
    bad = []
    for lv in levels:
        ok, info = solve(lv)
        print(f"L{lv['id']:02d} {'OK' if ok else 'FAIL'} {info}")
        if not ok:
            bad.append(lv["id"])
    print("UNSOLVABLE:", bad)


if __name__ == "__main__":
    main()
