"""Preview the debate persuasion curve for EnhancedBeliefs R3.

A won debate walks the loser's per-issue stance along a fixed curve in
(rank, strength) space, from their own faith's pole `L=(r0,s0)` to the
winner's pole `W=(rw,sw)`.
Rank is linear in the arc parameter `t`; strength dips through a catenary
well so the pawn must lose conviction before crossing to the new rung.

This mirrors the proposed `PullStance` write-path.
Nothing here is imported by the mod; it exists to eyeball shape and tunables.
"""

import numpy as np
import matplotlib.pyplot as plt
from pathlib import Path

# Conviction bounds mirror IdeoTrackerData (Min/Max/AbsoluteMaxConvictionStrength).
MIN_CONVICTION = 0.0
MAX_CONVICTION = 20.0
ABSOLUTE_MAX_CONVICTION = 50.0

# The demo ladder runs rungs 0..3, so the widest possible leap spans 3.
LADDER_SPAN = 3.0


def well_shape(tt, curvature):
    """Catenary well: 1 at the poles, 0 at the midpoint, flat-bottomed, steep near the edges."""
    return (np.cosh(curvature * (2.0 * tt - 1.0)) - 1.0) / (np.cosh(curvature) - 1.0)


def depth_from_leap(r0, rw, max_depth, span=LADDER_SPAN):
    """Valley depth scaled by how far the conversion drags the pawn: a one-rung nudge barely
    dents conviction, a full-ladder reversal craters it. `max_depth` is the widest-leap dip.
    """
    return max_depth * abs(rw - r0) / span


# Universal conviction at either pole - a fixed constant, NOT a stored per-pawn felt-strength.
HOME_STRENGTH = 14.0


def geometric_strength(rank, r0, r_far, floor_fraction, curvature, home_strength=HOME_STRENGTH):
    """Conviction as a pure function of rank: a fixed valley strung between the home rung and a
    fixed far pole, floor = span * floor_fraction (the 'rw/6' rule). No stored state - rank is
    the only memory, and the dip you experience scales with how far along the fixed curve you get
    dragged, so a small hop barely descends and a full reversal reaches the floor for free.
    """
    floor = abs(r_far - r0) * floor_fraction
    tt = (rank - r0) / (r_far - r0)
    return floor + (home_strength - floor) * well_shape(tt, curvature)


def rank_at(tt, r0, rw):
    return r0 + tt * (rw - r0)


def strength_at(tt, s0, sw, depth, curvature):
    floor = min(s0, sw) - depth
    rim = (1.0 - tt) * s0 + tt * sw
    return floor + (rim - floor) * well_shape(tt, curvature)


def debate_steps(pull, speed_reference, s0, sw, depth, curvature, max_steps=200):
    """Discrete arc positions a repeated-debate sequence lands on.

    Each debate advances `t` a `pull` fraction of the remaining arc, slowed by the
    speed factor `(1 - s / speed_reference)` so rank crawls at high conviction.
    Returns the `t` values visited (the dots to scatter along the curve).
    """
    tt = 0.0
    visited = [tt]
    for _ in range(max_steps):
        strength = strength_at(tt, s0, sw, depth, curvature)
        speed_factor = max(0.0, 1.0 - strength / speed_reference)
        tt = tt + (1.0 - tt) * pull * speed_factor
        visited.append(tt)
        if tt > 0.999:
            break
    return np.asarray(visited)


# --- The three memory models, all sharing the same linear rank step r' = r + pull*(rw - r). ---
# Only the strength update differs; that is the whole argument.

def walk_fixed_origin(pull, r0, rw, s0, sw, depth, curvature, steps):
    """Global fixed curve: strength read off one valley strung between remembered poles.

    Remembers the origin `(r0, s0)` for the whole walk. Strength = the point on that
    single curve at the current arc progress `t`.
    """
    ranks, strengths = [], []
    tt = 0.0
    for _ in range(steps):
        ranks.append(rank_at(tt, r0, rw))
        strengths.append(strength_at(tt, s0, sw, depth, curvature))
        tt = tt + (1.0 - tt) * pull
    return np.asarray(ranks), np.asarray(strengths)


def walk_memoryless_redraw(pull, r0, rw, s0, sw, depth, curvature, steps):
    """Memoryless: redraw a fresh valley from the CURRENT point to the winner each debate,
    step a `pull` fraction of it. No origin remembered - the current point is the new pole.
    """
    ranks, strengths = [], []
    rank, strength = rank_at(0.0, r0, rw), s0
    for _ in range(steps):
        ranks.append(rank)
        strengths.append(strength)
        floor = min(strength, sw) - depth
        rim = (1.0 - pull) * strength + pull * sw
        strength = np.clip(floor + (rim - floor) * well_shape(pull, curvature),
                           MIN_CONVICTION, ABSOLUTE_MAX_CONVICTION)
        rank = rank + pull * (rw - rank)
    return np.asarray(ranks), np.asarray(strengths)


def walk_relax_to_baseline(pull, r0, rw, baseline, depth, curvature, steps):
    """Memoryless in rank, but the rim is a personality constant `baseline`, not the origin.
    Strength relaxes toward `strength_at(t)` with both poles pinned at `baseline`, so it
    dips through the valley and recovers to the pawn's own firmness on the new rung.
    """
    ranks, strengths = [], []
    rank, strength = rank_at(0.0, r0, rw), baseline
    for _ in range(steps):
        ranks.append(rank)
        strengths.append(strength)
        rank = rank + pull * (rw - rank)
        tt = (rank - r0) / (rw - r0)
        target = strength_at(tt, baseline, baseline, depth, curvature)
        strength = strength + (target - strength) * pull
    return np.asarray(ranks), np.asarray(strengths)


def plot_pole_pairs(axis, curvature, max_depth):
    """A few loser/winner pole pairs on one (rank, strength) axis; depth scales with the leap."""
    tt = np.linspace(0.0, 1.0, 400)
    pairs = [
        ("symmetric, 0 to 3", 0.0, 3.0, 14.0, 14.0),
        ("asymmetric conviction, 0 to 3", 0.0, 3.0, 16.0, 8.0),
        ("shorter hop, 1 to 2", 1.0, 2.0, 14.0, 14.0),
        ("reversed, 3 to 0", 3.0, 0.0, 10.0, 16.0),
    ]
    for label, r0, rw, s0, sw in pairs:
        depth = depth_from_leap(r0, rw, max_depth)
        rank = rank_at(tt, r0, rw)
        strength = strength_at(tt, s0, sw, depth, curvature)
        line, = axis.plot(rank, strength, label=label)
        axis.scatter([r0, rw], [s0, sw], color=line.get_color(), zorder=3)
    axis.set_title(f"pole pairs (k={curvature}, max_depth={max_depth})")
    axis.set_xlabel("rank")
    axis.set_ylabel("conviction strength")
    axis.legend(fontsize=7)


def plot_curvature_sweep(axis, max_depth):
    tt = np.linspace(0.0, 1.0, 400)
    r0, rw, s0, sw = 0.0, 3.0, 14.0, 14.0
    depth = depth_from_leap(r0, rw, max_depth)
    for curvature in (0.5, 1.5, 2.5, 4.0):
        axis.plot(rank_at(tt, r0, rw), strength_at(tt, s0, sw, depth, curvature),
                  label=f"k={curvature}")
    axis.set_title(f"curvature sweep (full-ladder leap, depth={depth})")
    axis.set_xlabel("rank")
    axis.set_ylabel("conviction strength")
    axis.legend(fontsize=7)


def plot_leap_sweep(axis, curvature, max_depth):
    """Depth scales with the leap: same k and max_depth, different rung gaps."""
    tt = np.linspace(0.0, 1.0, 400)
    for rw in (1.0, 2.0, 3.0):
        depth = depth_from_leap(0.0, rw, max_depth)
        axis.plot(rank_at(tt, 0.0, rw), strength_at(tt, 14.0, 14.0, depth, curvature),
                  marker="", label=f"0 to {rw:.0f}  (depth={depth:.1f})")
    axis.set_title(f"leap sweep: dip scales with rung gap (k={curvature}, max_depth={max_depth})")
    axis.set_xlabel("rank")
    axis.set_ylabel("conviction strength")
    axis.legend(fontsize=7)


def plot_debate_dots(axis, curvature, max_depth):
    """Where repeated debates actually land, with and without the speed factor."""
    tt = np.linspace(0.0, 1.0, 400)
    r0, rw, s0, sw = 0.0, 3.0, 14.0, 14.0
    depth = depth_from_leap(r0, rw, max_depth)
    pull = 0.1
    axis.plot(rank_at(tt, r0, rw), strength_at(tt, s0, sw, depth, curvature),
              color="0.7", zorder=1)

    slowed = debate_steps(pull, ABSOLUTE_MAX_CONVICTION, s0, sw, depth, curvature)
    axis.scatter(rank_at(slowed, r0, rw), strength_at(slowed, s0, sw, depth, curvature),
                 s=14, zorder=3, label=f"speed-scaled ({len(slowed)-1} debates)")

    flat = debate_steps(pull, np.inf, s0, sw, depth, curvature)
    axis.scatter(rank_at(flat, r0, rw), strength_at(flat, s0, sw, depth, curvature),
                 s=14, marker="x", zorder=2, label=f"flat rate ({len(flat)-1} debates)")

    axis.set_title(f"debate steps, pull={pull} (k={curvature}, depth={depth})")
    axis.set_xlabel("rank")
    axis.set_ylabel("conviction strength")
    axis.legend(fontsize=7)


def plot_memory_models(curvature, max_depth):
    """Three strength rules (identical linear rank step) across leap sizes, so the leap-scaled
    depth shows its hand: the relax-lag vanishes on short hops and only bites on full reversals.
    """
    r0, baseline, sw = 0.0, 14.0, 14.0
    pull, steps = 0.1, 60
    leaps = [1.0, 3.0]

    figure, axes = plt.subplots(len(leaps), 2, figsize=(13, 5 * len(leaps)))
    for row, rw in enumerate(leaps):
        depth = depth_from_leap(r0, rw, max_depth)
        models = [
            ("fixed origin (remembers start)",
             walk_fixed_origin(pull, r0, rw, baseline, sw, depth, curvature, steps)),
            ("memoryless redraw (craters)",
             walk_memoryless_redraw(pull, r0, rw, baseline, sw, depth, curvature, steps)),
            ("relax to baseline B (recommended)",
             walk_relax_to_baseline(pull, r0, rw, baseline, depth, curvature, steps)),
        ]

        left, right = axes[row]
        for label, (ranks, strengths) in models:
            left.plot(ranks, strengths, marker="o", markersize=3, label=label)
        left.scatter([r0, rw], [baseline, sw], color="black", zorder=4)
        left.axhline(baseline - depth, color="0.8", linestyle="--", linewidth=1)
        left.set_title(f"leap 0 to {rw:.0f}: (rank, strength) trajectory (depth={depth:.1f})")
        left.set_xlabel("rank")
        left.set_ylabel("conviction strength")
        left.legend(fontsize=8)

        debates = np.arange(steps)
        for label, (_, strengths) in models:
            right.plot(debates, strengths, marker="o", markersize=3, label=label)
        right.axhline(baseline - depth, color="0.8", linestyle="--", linewidth=1,
                      label="valley floor B - depth")
        right.set_title(f"leap 0 to {rw:.0f}: conviction vs. debates fought")
        right.set_xlabel("debates")
        right.set_ylabel("conviction strength")
        right.legend(fontsize=8)

    figure.suptitle(f"EnhancedBeliefs persuasion: what carries the memory (k={curvature}, max_depth={max_depth})")
    figure.tight_layout()
    return figure


def fit_symmetric_cosh(vertex_rank, floor, r1, s1, r2, s2):
    """Fit s(r) = floor + A*(cosh((r - vertex_rank)/w) - 1) through two points, vertex pinned.
    Returns (A, w). Symmetric about the vertex: only A and w are free.
    """
    a, b = r1 - vertex_rank, r2 - vertex_rank

    def residual(w):
        return (s1 - floor) * (np.cosh(b / w) - 1.0) - (s2 - floor) * (np.cosh(a / w) - 1.0)

    ws = np.geomspace(0.02, 80.0, 4000)
    vals = residual(ws)
    crossings = np.where(np.diff(np.sign(vals)) != 0)[0]
    if len(crossings) == 0:
        w = ws[np.argmin(np.abs(vals))]
    else:
        lo, hi = ws[crossings[0]], ws[crossings[0] + 1]
        for _ in range(80):
            mid = 0.5 * (lo + hi)
            if residual(lo) * residual(mid) <= 0.0:
                hi = mid
            else:
                lo = mid
        w = 0.5 * (lo + hi)
    amplitude = (s1 - floor) / (np.cosh(a / w) - 1.0)
    return amplitude, w


def plot_three_point_cosh(floor_fraction):
    """Fit a symmetric cosh (vertex = (rw+rm)/2, floor = sw*fraction) through the current point
    and the winner point. Shows the catch: it is pinned but goes flat/insane when the two points
    sit at unequal distances from the vertex yet similar heights - the common case.
    """
    sw = 14.0
    span = LADDER_SPAN
    cases = [
        # (label, winner rank rw, extreme rm, current rank ri, current strength si)
        ("sane: current at the far extreme (equidistant)", 0.0, 3.0, 3.0, 9.0),
        ("insane: current inside, same height as winner", 0.0, 3.0, 2.0, 14.0),
        ("insane-ish: current near vertex", 0.0, 3.0, 1.7, 12.0),
    ]

    figure, axis = plt.subplots(figsize=(9, 6))
    for label, rw, rm, ri, si in cases:
        vertex = 0.5 * (rw + rm)
        floor = sw * floor_fraction
        amplitude, width = fit_symmetric_cosh(vertex, floor, ri, si, rw, sw)
        rank = np.linspace(min(rw, rm), max(rw, rm), 400)
        strength = floor + amplitude * (np.cosh((rank - vertex) / width) - 1.0)
        line, = axis.plot(rank, strength, label=f"{label}  (w={width:.1f}, min={strength.min():.1f})")
        axis.scatter([ri, rw], [si, sw], color=line.get_color(), zorder=3)
        axis.scatter([vertex], [floor], color=line.get_color(), marker="x", zorder=3)

    axis.axhline(sw * floor_fraction, color="0.8", linestyle="--", linewidth=1,
                 label=f"target floor sw/6 = {sw * floor_fraction:.2f}")
    axis.set_title("symmetric cosh through 3 points: pinned, but flattens when points are lopsided")
    axis.set_xlabel("rank")
    axis.set_ylabel("conviction strength")
    axis.legend(fontsize=8)
    figure.tight_layout()
    return figure


def lopsided_strength(rank, vertex, floor, amp_winner, width_winner, amp_home, width_home, winner_dir):
    """Two independent cosh arms meeting at a shared vertex (floor, zero slope both sides).
    The arm on the winner's side of the vertex uses (amp_winner, width_winner); the other
    (amp_home, width_home). Lopsided: each side has its own curvature.
    """
    offset = rank - vertex
    on_winner_side = np.sign(offset) == np.sign(winner_dir)
    amp = np.where(on_winner_side, amp_winner, amp_home)
    width = np.where(on_winner_side, amp * 0 + width_winner, amp * 0 + width_home)
    return floor + amp * (np.cosh(offset / width) - 1.0)


def amp_for_anchor(anchor_rank, anchor_strength, vertex, floor, width):
    """Amplitude that makes an arm of the given width pass through (anchor_rank, anchor_strength)."""
    return (anchor_strength - floor) / (np.cosh((anchor_rank - vertex) / width) - 1.0)


def arc_length_step(ri, rw, step_len, rank_weight, curve_of, samples=4000):
    """Advance a fixed distance ALONG the curve from ri toward rw, not a fraction of the rank gap.

    Arc length uses metric ds^2 = (rank_weight * drank)^2 + dstrength^2, so steep high-conviction
    stretches accrue length fast (rank crawls) while the flat valley floor lets rank race across.
    `rank_weight` sets how many conviction-units one rung of rank is worth in that metric.
    """
    ranks = np.linspace(ri, rw, samples)
    strengths = curve_of(ranks)
    seg = np.hypot(rank_weight * np.diff(ranks), np.diff(strengths))
    cumulative = np.concatenate([[0.0], np.cumsum(seg)])
    landed_rank = float(np.interp(min(step_len, cumulative[-1]), cumulative, ranks))
    landed_strength = float(curve_of(np.array([landed_rank]))[0])
    return landed_rank, landed_strength


def curve_from(config, ri, si):
    """Build the (rank -> strength) curve the pawn rides given its live position (ri, si).

    Redrawn from the current point each debate (memoryless): winner (rw, sw) and the structural
    vertex (rw + rm)/2 are fixed data, the home arm bends to honour (ri, si). Once the pawn
    crosses the vertex the muddle is behind it, so (ri, si) becomes the local floor of a
    symmetric bowl fit to the winner. Returns the curve plus its vertex, floor, and a label.
    """
    rw, rm, sw = config["rw"], config["rm"], config["sw"]
    structural_vertex = 0.5 * (rw + rm)
    winner_dir = rw - structural_vertex
    # The crossed/not-crossed switch is a DIAGONAL in (rank, strength), not a vertical line at
    # the vertex: at zero conviction the boundary sits on the structural vertex, but as conviction
    # rises it slides toward the home pole by cross_shift * (distance vertex->rm). This keeps a
    # firmly-held near-vertex stance on the crossed (bowl) side, away from the singular region
    # where anchoring the home arm right on the vertex would blow the amplitude up.
    boundary_rank = structural_vertex + (si / ABSOLUTE_MAX_CONVICTION) * config["cross_shift"] * (rm - structural_vertex)
    crossed = np.sign(ri - boundary_rank) == np.sign(winner_dir)

    if crossed:
        vertex, floor = ri, si
        width_winner = width_home = config["width_winner"]
        amp_winner = amp_home = amp_for_anchor(rw, sw, vertex, floor, width_winner)
        vertex_label = f"pawn is the floor (crossed): ({vertex:.1f}, {floor:.1f})"
    else:
        vertex = structural_vertex
        floor = sw * config["floor_fraction"]
        width_winner, width_home = config["width_winner"], config["width_home"]
        amp_winner = amp_for_anchor(rw, sw, vertex, floor, width_winner)
        amp_home = amp_for_anchor(ri, si, vertex, floor, width_home)
        vertex_label = f"vertex ({vertex:.1f}, sw*{config['floor_fraction']:.2f}={floor:.1f})"

    def curve_of(ranks):
        return lopsided_strength(ranks, vertex, floor, amp_winner, width_winner,
                                 amp_home, width_home, winner_dir)
    return curve_of, vertex, floor, vertex_label


def converge_walk(config, tol_rank=0.01, tol_strength=0.5, max_steps=500):
    """Repeat the arc-length debate step until the pawn is within (tol_rank, tol_strength) of
    the winner. The curve is redrawn from the live point each step. Returns the visited ranks
    and strengths (the trajectory), origin included.
    """
    rw, sw = config["rw"], config["sw"]
    step_len, rank_weight = config["step_len"], config["rank_weight"]
    ri, si = config["ri"], config["si"]
    ranks, strengths = [ri], [si]
    for _ in range(max_steps):
        if abs(ri - rw) <= tol_rank and abs(si - sw) <= tol_strength:
            break
        curve_of, *_ = curve_from(config, ri, si)
        ri, si = arc_length_step(ri, rw, step_len, rank_weight, curve_of)
        ranks.append(ri)
        strengths.append(si)
    return np.asarray(ranks), np.asarray(strengths)


def plot_convergence(config):
    """Full debate sequence from the sandbox origin to the winner. Left: the trajectory dots
    laid over the initial curve. Right: rank and strength vs debate number, converging to the
    winner's (rw, sw).
    """
    rw, rm, sw = config["rw"], config["rm"], config["sw"]
    ranks, strengths = converge_walk(config)
    steps = np.arange(len(ranks))

    curve_of, _, _, _ = curve_from(config, config["ri"], config["si"])
    backdrop_rank = np.linspace(min(rw, rm), max(rw, rm), 400)

    figure, (left, right) = plt.subplots(1, 2, figsize=(13, 5))

    left.plot(backdrop_rank, curve_of(backdrop_rank), color="0.8", zorder=1,
              label="initial curve")
    left.plot(ranks, strengths, color="tab:orange", marker="o", markersize=3, zorder=2,
              label=f"trajectory ({len(ranks) - 1} debates)")
    left.scatter([rw], [sw], color="tab:green", zorder=3, label=f"winner ({rw}, {sw})")
    left.scatter([config["ri"]], [config["si"]], color="tab:blue", zorder=3,
                 label=f"origin ({config['ri']}, {config['si']})")
    left.set_title("trajectory over the initial curve")
    left.set_xlabel("rank")
    left.set_ylabel("conviction strength")
    left.set_ylim(MIN_CONVICTION, ABSOLUTE_MAX_CONVICTION)
    left.legend(fontsize=8)

    right.plot(steps, ranks, color="tab:purple", marker="o", markersize=3, label="rank")
    right.plot(steps, strengths, color="tab:orange", marker="o", markersize=3, label="strength")
    right.axhline(rw, color="tab:purple", linestyle="--", linewidth=1)
    right.axhline(sw, color="tab:orange", linestyle="--", linewidth=1)
    right.set_title(f"convergence to winner ({len(ranks) - 1} debates)")
    right.set_xlabel("debate number")
    right.set_ylabel("rank / conviction strength")
    right.legend(fontsize=8)

    figure.tight_layout()
    return figure


def plot_lopsided_sandbox(config):
    """Turn-the-dials view of the three-point lopsided cosh. Every number lives in `config`.

    Fixed data: three real points thread the curve - winner (rw, sw), current pawn (ri, si),
    and floor = sw * floor_fraction at the structural vertex (rw + rm)/2. Free knobs: per-side
    widths. The home-extreme recovery is DERIVED (whatever the curve says at rm), not specified.
    """
    rw, rm, sw = config["rw"], config["rm"], config["sw"]
    ri, si = config["ri"], config["si"]
    curve_of, vertex, floor, vertex_label = curve_from(config, ri, si)

    rank = np.linspace(min(rw, rm), max(rw, rm), 400)
    strength = curve_of(rank)
    recovery_home = float(curve_of(np.array([rm]))[0])

    structural_vertex = 0.5 * (rw + rm)
    boundary_top = structural_vertex + config["cross_shift"] * (rm - structural_vertex)

    figure, axis = plt.subplots(figsize=(9, 6))
    axis.plot(rank, strength, zorder=1)
    axis.plot([structural_vertex, boundary_top], [0.0, ABSOLUTE_MAX_CONVICTION],
              color="0.7", linestyle=":", zorder=1, label="crossed switch boundary")
    axis.scatter([rw], [sw], color="tab:green", zorder=3, label=f"winner (rw={rw}, sw={sw})")
    axis.scatter([rm], [recovery_home], color="tab:red", zorder=3,
                 label=f"home extreme (rm={rm}, derived recovery={recovery_home:.1f})")
    axis.scatter([vertex], [floor], color="black", marker="x", zorder=3, label=vertex_label)
    axis.scatter([ri], [si], color="tab:blue", zorder=3, label=f"current pawn (ri={ri}, si={si})")

    step_len, rank_weight = config["step_len"], config["rank_weight"]
    ri_next, si_next = arc_length_step(ri, rw, step_len, rank_weight, curve_of)
    axis.annotate("", xy=(ri_next, si_next), xytext=(ri, si),
                  arrowprops=dict(arrowstyle="->", color="tab:orange", lw=1.5), zorder=4)
    axis.scatter([ri_next], [si_next], color="tab:orange", zorder=4,
                 label=f"after 1 debate (arc={step_len}): r={ri_next:.2f}, s={si_next:.1f}")

    axis.set_title(f"lopsided cosh sandbox  (w_win={config['width_winner']}, w_home={config['width_home']})")
    axis.set_xlabel("rank")
    axis.set_ylabel("conviction strength")
    axis.set_ylim(MIN_CONVICTION, ABSOLUTE_MAX_CONVICTION)
    axis.legend(fontsize=8)
    figure.tight_layout()
    return figure


def plot_geometric(curvature, floor_fraction):
    """The stateless model: strength read straight off a fixed rank->strength curve.
    Left: the curve for a few home rungs. Right: where partial conversions land on it.
    """
    r0, r_far = 0.0, LADDER_SPAN
    rank = np.linspace(r0, r_far, 400)

    figure, (left, right) = plt.subplots(1, 2, figsize=(13, 5))

    left.plot(rank, geometric_strength(rank, r0, r_far, floor_fraction, curvature))
    floor = abs(r_far - r0) * floor_fraction
    left.axhline(floor, color="0.8", linestyle="--", linewidth=1, label=f"floor = span/6 = {floor:.2f}")
    left.scatter([r0, r_far], [HOME_STRENGTH, HOME_STRENGTH], color="black", zorder=3)
    left.set_title(f"stateless conviction curve (k={curvature}, home={HOME_STRENGTH})")
    left.set_xlabel("rank")
    left.set_ylabel("conviction strength")
    left.legend(fontsize=8)

    right.plot(rank, geometric_strength(rank, r0, r_far, floor_fraction, curvature), color="0.7")
    for winner_rung in (1.0, 2.0, 3.0):
        landed = np.linspace(r0, winner_rung, 200)
        strengths = geometric_strength(landed, r0, r_far, floor_fraction, curvature)
        right.plot(landed, strengths, marker="", linewidth=3,
                   label=f"pushed to rung {winner_rung:.0f} (min {strengths.min():.1f})")
    right.set_title("dip scales with how far you're pushed, for free")
    right.set_xlabel("rank")
    right.set_ylabel("conviction strength")
    right.legend(fontsize=8)

    figure.suptitle("EnhancedBeliefs persuasion: stateless conviction (rank is the only memory)")
    figure.tight_layout()
    return figure


def main():
    curvature = 2.5
    max_depth = 6.0
    floor_fraction = 1.0 / 6.0
    out_dir = Path(__file__).parent / "results"
    out_dir.mkdir(exist_ok=True)

    shapes, axes = plt.subplots(2, 2, figsize=(12, 9))
    plot_pole_pairs(axes[0, 0], curvature, max_depth)
    plot_curvature_sweep(axes[0, 1], max_depth)
    plot_leap_sweep(axes[1, 0], curvature, max_depth)
    plot_debate_dots(axes[1, 1], curvature, max_depth)
    shapes.suptitle("EnhancedBeliefs debate persuasion: conviction valley in (rank, strength)")
    shapes.tight_layout()
    shapes.savefig(out_dir / "conviction_valley.png", dpi=130)

    memory = plot_memory_models(curvature, max_depth)
    memory.savefig(out_dir / "memory_models.png", dpi=130)

    geometric = plot_geometric(curvature, floor_fraction)
    geometric.savefig(out_dir / "geometric.png", dpi=130)

    three_point = plot_three_point_cosh(floor_fraction)
    three_point.savefig(out_dir / "three_point.png", dpi=130)

    # Every dial for the lopsided three-point cosh lives here - tweak and re-run.
    rw = 1.0  # winner's rung (the target we head toward)
    rm = 0.0  # ladder extreme in the current pawn's direction
    sandbox_config = {
        "rw": rw,
        "rm": rm,
        "sw": 50.0,           # winner's conviction
        "ri": 0.9999,            # where the current pawn sits now
        "si": 10,           # the pawn's ACTUAL current conviction (real data, anchors home arm)
        "floor_fraction": 1.0 / 32.0,   # floor = sw * this, at the vertex (rw+rm)/2
        "cross_shift": 0.5,   # how far (frac of vertex->rm) the crossed switch slides at max conviction
        "width_winner": 0.3,  # curvature of the winner-side arm (smaller = steeper)
        "width_home": .1,    # curvature of the home-side arm
        "step_len": 3.0,      # ARC length covered in one won debate (along the curve, not rank)
        "rank_weight": 20.0/(rw-rm),  # conviction-units one rung of rank is worth in the arc metric
    }
    sandbox = plot_lopsided_sandbox(sandbox_config)
    sandbox.savefig(out_dir / "lopsided_sandbox.png", dpi=130)

    convergence = plot_convergence(sandbox_config)
    convergence.savefig(out_dir / "convergence.png", dpi=130)

    print(f"wrote {out_dir / 'conviction_valley.png'}")
    print(f"wrote {out_dir / 'memory_models.png'}")
    print(f"wrote {out_dir / 'geometric.png'}")
    print(f"wrote {out_dir / 'three_point.png'}")
    print(f"wrote {out_dir / 'lopsided_sandbox.png'}")
    print(f"wrote {out_dir / 'convergence.png'}")


if __name__ == "__main__":
    main()
