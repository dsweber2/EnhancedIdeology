"""Plot the chance that a drawn debate turns into a social fight.

Mirrors `DebateFights.Chance` in `Source/Interactions/DebateFights.cs`:

    mean(F_i * d_i, F_r * d_r) / (1 + falloff * max_social) * topic_weight / 20 * apostasy

where F is vanilla's SocialFightChance at the debate base chance, clamped to 1.
Each line holds every input fixed except the higher Social skill of the pair.
Nothing here is imported by the mod; it exists to eyeball the tuning.
"""

import argparse
from dataclasses import dataclass
from pathlib import Path

import matplotlib.pyplot as plt
from matplotlib.ticker import LogLocator
import numpy as np

# Constants mirror DebateFights.
DRAW_FIGHT_BASE_CHANCE = 0.026
SOCIAL_FALLOFF = 0.2
REFERENCE_TOPIC_WEIGHT = 20.0
APOSTASY_PER_FAITH = 0.4
BIGOTED_FIGHT_FACTOR = 2.0
TOLERANT_FIGHT_FACTOR = 0.2

# Vanilla SocialFightChance factors used by the scenarios.
BLOODLUST = 4.0
OPINION_MINUS_100 = 4.0

# About one debate in five is a draw between evenly matched debaters (DebateDrawThreshold 0.25, SD 0.75).
DRAW_RATE = 0.19

# Vanilla socialFightBaseChance per interaction (Core Interactions_Social.xml). Divided by DRAW_RATE so they share
# the per-draw axis: a debate line at that height fights as often per debate as the vanilla interaction does.
VANILLA_REFERENCES = [("Slight", 0.005), ("Insult", 0.04)]

SURFACE = "#fcfcfb"
TEXT_PRIMARY = "#0b0b0b"
TEXT_SECONDARY = "#52514e"
GRID = "#e4e3df"
SERIES = ["#2a78d6", "#eb6834", "#1baf7a", "#eda100", "#e87ba4", "#008300", "#6250d6"]
OLD_FORMULA = "#8a8984"
REFERENCE = "#b5b4ae"


@dataclass(frozen=True)
class Scenario:
    label: str
    initiator_factor: float = 1.0
    recipient_factor: float = 1.0
    initiator_diversity: float = 1.0
    recipient_diversity: float = 1.0
    topic_weight: float = REFERENCE_TOPIC_WEIGHT
    apostasy_strictness: tuple[float, float] | None = None


def pawn_chance(factor, diversity):
    return min(DRAW_FIGHT_BASE_CHANCE * factor, 1.0) * diversity


def fight_chance(scenario, max_social, falloff, apostasy):
    mean_chance = (pawn_chance(scenario.initiator_factor, scenario.initiator_diversity)
                   + pawn_chance(scenario.recipient_factor, scenario.recipient_diversity)) / 2.0
    chance = mean_chance / (1.0 + falloff * max_social) * scenario.topic_weight / REFERENCE_TOPIC_WEIGHT
    if scenario.apostasy_strictness is not None:
        chance *= 1.0 + apostasy * sum(scenario.apostasy_strictness)
    return chance


def old_fight_chance(social):
    """The formula before the rework, for two healthy pawns of equal Social at opinion 0."""
    return 0.05 * 2.0 / (0.5 + 0.1 * social) ** 2


SCENARIOS = [
    Scenario("Both Diversity Exalted", initiator_diversity=TOLERANT_FIGHT_FACTOR,
             recipient_diversity=TOLERANT_FIGHT_FACTOR),
    Scenario("Same faith, 10-pt conviction gap", topic_weight=10.0),
    Scenario("Full-ladder gap (baseline)"),
    Scenario("Cross-faith, one strict apostasy", apostasy_strictness=(1.0, 0.0)),
    Scenario("One bloodlust pawn", initiator_factor=BLOODLUST),
    Scenario("Bitter rivals (opinion −100)", initiator_factor=OPINION_MINUS_100,
             recipient_factor=OPINION_MINUS_100),
    Scenario("Cross-faith, both strict apostasy + Abhorrent", initiator_diversity=BIGOTED_FIGHT_FACTOR,
             recipient_diversity=BIGOTED_FIGHT_FACTOR, apostasy_strictness=(1.0, 1.0)),
]


# Smallest vertical gap between two end-of-line labels, in decades of the log axis.
LABEL_GAP_DECADES = 0.075


def spread_labels(ends):
    """End-of-line label heights, pushed apart upward from the lowest so no two sit closer than the gap."""
    order = np.argsort(ends)
    heights = np.log10(np.asarray(ends, dtype=float))
    for previous, current in zip(order, order[1:]):
        heights[current] = max(heights[current], heights[previous] + LABEL_GAP_DECADES)
    return 10 ** heights


def plot(out_path, falloff, apostasy):
    social = np.arange(0, 21)
    fig, axis = plt.subplots(figsize=(10, 6.2), facecolor=SURFACE)
    axis.set_facecolor(SURFACE)

    axis.plot(social, old_fight_chance(social), color=OLD_FORMULA, linewidth=2, linestyle="--",
              label="Old formula (both pawns at this Social)", zorder=2)
    axis.annotate("Old formula", (4, old_fight_chance(4)), xytext=(8, 4), textcoords="offset points",
                  va="center", fontsize=9, color=TEXT_SECONDARY)
    for (name, chance), style in zip(VANILLA_REFERENCES, ("-.", ":")):
        axis.axhline(chance / DRAW_RATE, color=REFERENCE, linewidth=1.5, linestyle=style, zorder=1,
                     label=f"Vanilla {name} ({chance:.1%} per interaction ÷ draw rate)")
    ends = []
    for color, scenario in zip(SERIES, SCENARIOS):
        values = [fight_chance(scenario, skill, falloff, apostasy) for skill in social]
        axis.plot(social, values, color=color, linewidth=2, label=scenario.label, zorder=3)
        ends.append(values[-1])
    for scenario, label_y in zip(SCENARIOS, spread_labels(ends)):
        axis.annotate(scenario.label, (social[-1], label_y), xytext=(8, 0), textcoords="offset points",
                      va="center", fontsize=9, color=TEXT_PRIMARY, annotation_clip=False)

    axis.set_yscale("log")
    axis.yaxis.set_major_locator(LogLocator(base=10, subs=(1.0, 2.0, 5.0)))
    axis.yaxis.set_major_formatter(plt.FuncFormatter(lambda value, _: f"{value * 100:g}%"))
    axis.set_xlim(0, 20)
    axis.set_xticks(range(0, 21))
    axis.set_xlabel("Higher Social skill of the two debaters", color=TEXT_SECONDARY)
    axis.set_ylabel("Fight chance per drawn debate (log scale)", color=TEXT_SECONDARY)
    axis.set_title(f"Chance a drawn debate becomes a social fight (k = {falloff:g}, apostasy {apostasy:g} per faith)", color=TEXT_PRIMARY, loc="left", fontsize=13, pad=34)
    axis.text(0, 1.01, f"Healthy pawns at neutral opinion unless noted. Per debate, multiply by the draw rate "
              f"(≈{DRAW_RATE:.0%} for evenly matched debaters).\n"
              f"Light grey lines: vanilla per-interaction fight chances divided by that draw rate.",
              transform=axis.transAxes, fontsize=9, color=TEXT_SECONDARY, va="bottom")

    axis.grid(True, which="major", color=GRID, linewidth=0.8)
    axis.grid(True, which="minor", axis="y", color=GRID, linewidth=0.4)
    for side in ("top", "right"):
        axis.spines[side].set_visible(False)
    for side in ("left", "bottom"):
        axis.spines[side].set_color(GRID)
    axis.tick_params(colors=TEXT_SECONDARY)

    axis.legend(loc="upper center", bbox_to_anchor=(0.5, -0.12), ncol=2, frameon=False, fontsize=9,
                labelcolor=TEXT_PRIMARY)
    fig.tight_layout()
    fig.savefig(out_path, dpi=130, facecolor=SURFACE, bbox_inches="tight")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--falloff", type=float, default=SOCIAL_FALLOFF,
                        help="social falloff k in 1 / (1 + k * max_social); defaults to the mod's value")
    parser.add_argument("--apostasy", type=float, default=APOSTASY_PER_FAITH,
                        help="fight multiplier gained per strict-apostasy faith; defaults to the mod's value")
    args = parser.parse_args()
    results = Path(__file__).parent / "results"
    results.mkdir(exist_ok=True)
    suffix = "" if args.falloff == SOCIAL_FALLOFF else f"_k{args.falloff:g}"
    suffix += "" if args.apostasy == APOSTASY_PER_FAITH else f"_apostasy{args.apostasy:g}"
    plot(results / f"debate_fight_odds{suffix}.png", args.falloff, args.apostasy)
