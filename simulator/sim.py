"""
Belief dynamics simulator for EnhancedBeliefs.

Models the three-component opinion system (base + personal + relationship),
certainty ticks, and social conversion interactions.

Key approximation: precept-sourced mood is derived from own-ideo opinion
rather than simulated from individual precept thoughts.
Global mood = base_happiness + precept_mood_contribution, and is only used
as a gate condition for the inactivity certainty penalty.
"""

from __future__ import annotations

from dataclasses import dataclass, field

import numpy as np
import pandas as pd

# Ported from GameComponent_EnhancedBeliefs.cs
CERTAINTY_LOSS_FROM_INACTIVITY = np.array([
    [3,  0.01],
    [5,  0.02],
    [10, 0.03],
    [30, 0.05],
], dtype=float)

CERTAINTY_OFFSET_FROM_THOUGHTS = np.array([
    [-50, -0.15],
    [-30, -0.07],
    [-10, -0.03],
    [-5,  -0.015],
    [-3,  -0.005],
    [0,    0.0],
    [3,    0.005],
    [5,    0.012],
    [10,   0.025],
    [30,   0.05],
    [50,   0.12],
], dtype=float)

# Input is sum of interpersonal opinions with co-ideologues (vanilla [-100, 100] scale)
CERTAINTY_MULTIPLIER_FROM_RELATIONSHIPS = np.array([
    [-1000, -0.9],
    [-500,  -0.5],
    [-200,  -0.3],
    [-100,  -0.1],
    [-50,   -0.05],
    [-10,   -0.02],
    [0,      0.0],
    [10,     0.01],
    [50,     0.03],
    [100,    0.07],
    [200,    0.2],
    [500,    0.4],
    [1000,   0.6],
], dtype=float)

INACTIVITY_THRESHOLD_DAYS = 3.0
INACTIVITY_MOOD_THRESHOLD = 0.8
CONVERSION_CERTAINTY_THRESHOLD = 0.2
CONVERSION_OPINION_THRESHOLD_NORMAL = 0.85
CONVERSION_OPINION_THRESHOLD_BREAKDOWN = 0.6


def _interp(curve: np.ndarray, xx: float) -> float:
    return float(np.interp(xx, curve[:, 0], curve[:, 1]))


@dataclass
class Pawn:
    pawn_id: int
    ideo_id: int
    certainty: float
    # Structural compatibility per ideo, derived from memes/precepts/traits; [0, 100]
    base_opinions: dict[int, float]
    # Accumulated through interactions, debates, book-reading; [−100, 100]
    personal_opinions: dict[int, float]
    # Net mood offset per ideo from its precept set; positive = feel-good ideo, negative = demanding
    ideo_mood_offsets: dict[int, float] = field(default_factory=dict)
    # Social opinion of each other pawn; [-100, 100] vanilla scale
    interpersonal_opinions: dict[int, float] = field(default_factory=dict)
    days_without_positive_precept: float = 0.0
    base_happiness: float = 0.85

    def ideo_opinion(self, ideo_id: int) -> float:
        """Combined opinion [0, 1] of a given ideology."""
        # Own ideo uses live certainty as the base, mirroring IdeoTrackerData.IdeoOpinion
        base = self.certainty * 100.0 if ideo_id == self.ideo_id else self.base_opinions.get(ideo_id, 30.0)
        personal = self.personal_opinions.get(ideo_id, 0.0)
        personal = float(np.clip(personal, -base, 100.0 - base))
        return float(np.clip((base + personal) / 100.0, 0.0, 1.0))

    def precept_mood_sum(self) -> float:
        """
        Approximate precept-sourced mood offset sum.
        Ideo opinion drives the core term (high opinion → positive precept mood),
        base happiness shifts it (general mood affects ideological engagement),
        and each ideo has a fixed net offset representing whether its precept set
        is mostly feel-good (positive) or demanding/punishing (negative).
        """
        ideo_term = (self.ideo_opinion(self.ideo_id) - 0.5) * 60.0
        happiness_term = (self.base_happiness - 0.8) * 30.0
        ideo_offset = self.ideo_mood_offsets.get(self.ideo_id, 0.0)
        return ideo_term + happiness_term + ideo_offset

    def global_mood(self) -> float:
        return float(np.clip(
            self.base_happiness + self.precept_mood_sum() / 100.0 * 0.4,
            0.0, 1.0
        ))

    def co_ideologue_relationship_sum(self, pawns: list[Pawn]) -> float:
        """Sum of interpersonal opinions with pawns who share this ideo."""
        return sum(
            self.interpersonal_opinions.get(pp.pawn_id, 0.0)
            for pp in pawns
            if pp.pawn_id != self.pawn_id and pp.ideo_id == self.ideo_id
        )


@dataclass
class SimParams:
    num_pawns: int = 10
    num_ideos: int = 3
    sim_days: int = 120
    # Expected social interactions per pawn per day (cross-ideo pairs)
    interaction_rate: float = 0.3
    conversion_power_base: float = 0.5
    base_happiness_mean: float = 0.85
    base_happiness_std: float = 0.05
    initial_certainty_mean: float = 0.75
    initial_certainty_std: float = 0.15
    # Std dev of per-ideo mood offset. 0 = all ideos identical; higher = some ideos are
    # feel-good (positive offset) while others are demanding/punishing (negative offset).
    ideo_mood_spread: float = 0.0
    seed: int = 42


def _make_pawns(params: SimParams, rng: np.random.Generator) -> list[Pawn]:
    ideo_ids = list(range(params.num_ideos))
    ideo_mood_offsets = {
        iid: float(rng.normal(0.0, params.ideo_mood_spread))
        for iid in ideo_ids
    }
    pawns: list[Pawn] = []
    for ii in range(params.num_pawns):
        ideo_id = ideo_ids[ii % params.num_ideos]
        certainty = float(np.clip(
            rng.normal(params.initial_certainty_mean, params.initial_certainty_std),
            0.05, 1.0
        ))
        base_happiness = float(np.clip(
            rng.normal(params.base_happiness_mean, params.base_happiness_std),
            0.3, 1.0
        ))
        base_opinions = {
            iid: (certainty * 100.0 if iid == ideo_id else float(rng.uniform(15.0, 55.0)))
            for iid in ideo_ids
        }
        pawns.append(Pawn(
            pawn_id=ii,
            ideo_id=ideo_id,
            certainty=certainty,
            base_opinions=base_opinions,
            personal_opinions={iid: 0.0 for iid in ideo_ids},
            ideo_mood_offsets=ideo_mood_offsets,
            interpersonal_opinions={
                jj: float(rng.normal(0.0, 25.0))
                for jj in range(params.num_pawns) if jj != ii
            },
            base_happiness=base_happiness,
        ))
    return pawns


def _check_conversion(
    pawn: Pawn,
    ideo_ids: list[int],
    rng: np.random.Generator,
    priority_ideo: int | None = None,
    breakdown: bool = False,
) -> bool:
    threshold = CONVERSION_OPINION_THRESHOLD_BREAKDOWN if breakdown else CONVERSION_OPINION_THRESHOLD_NORMAL
    candidates = sorted(
        (iid for iid in ideo_ids if iid != pawn.ideo_id),
        key=lambda iid: pawn.ideo_opinion(iid),
    )
    # Priority ideo goes last so it's tried first after reversal
    if priority_ideo is not None and priority_ideo in candidates:
        candidates.remove(priority_ideo)
        candidates.append(priority_ideo)

    for ideo_id in reversed(candidates):
        opinion = pawn.ideo_opinion(ideo_id)
        chance = opinion if priority_ideo == ideo_id else opinion * 0.5
        if opinion > threshold and rng.random() < chance:
            pawn.personal_opinions[ideo_id] = pawn.certainty * 100.0
            pawn.personal_opinions[pawn.ideo_id] = 0.0
            pawn.base_opinions[ideo_id] = opinion * 100.0
            pawn.ideo_id = ideo_id
            pawn.certainty = float(np.clip(opinion, 0.1, 1.0))
            return True
    return False


def run_simulation(params: SimParams) -> pd.DataFrame:
    rng = np.random.default_rng(params.seed)
    ideo_ids = list(range(params.num_ideos))
    pawns = _make_pawns(params, rng)
    rows: list[dict] = []

    for day in range(params.sim_days):
        for pawn in pawns:
            rows.append({
                "day": day,
                "pawn_id": pawn.pawn_id,
                "ideo_id": pawn.ideo_id,
                "ideo_mood_offset": pawn.ideo_mood_offsets.get(pawn.ideo_id, 0.0),
                "certainty": pawn.certainty,
                "own_opinion": pawn.ideo_opinion(pawn.ideo_id),
                "global_mood": pawn.global_mood(),
            })

        # Certainty tick — mirrors IdeoTracker_CertaintyChange + CertaintyChangeRecache
        for pawn in pawns:
            precept_mood = pawn.precept_mood_sum()
            delta = _interp(CERTAINTY_OFFSET_FROM_THOUGHTS, precept_mood)

            rel_sum = pawn.co_ideologue_relationship_sum(pawns)
            rel_mult = 1.0 + _interp(CERTAINTY_MULTIPLIER_FROM_RELATIONSHIPS, rel_sum) * np.sign(delta)
            delta *= rel_mult

            if delta > 0:
                pawn.days_without_positive_precept = 0.0
            else:
                pawn.days_without_positive_precept += 1.0

            if (pawn.global_mood() < INACTIVITY_MOOD_THRESHOLD
                    and pawn.days_without_positive_precept > INACTIVITY_THRESHOLD_DAYS):
                inactivity_loss = _interp(CERTAINTY_LOSS_FROM_INACTIVITY, pawn.days_without_positive_precept)
                delta -= inactivity_loss

            pawn.certainty = float(np.clip(pawn.certainty + delta, 0.0, 1.0))

            if pawn.certainty <= CONVERSION_CERTAINTY_THRESHOLD:
                _check_conversion(pawn, ideo_ids, rng, breakdown=(pawn.certainty <= 0.0))

        # Social interactions — mirrors InteractionWorker_AdvancedConversionAttempt
        num_interactions = max(1, int(params.num_pawns * params.interaction_rate))
        for _ in range(num_interactions):
            ii, jj = rng.choice(params.num_pawns, size=2, replace=False)
            initiator, recipient = pawns[ii], pawns[jj]
            if initiator.ideo_id == recipient.ideo_id:
                continue

            opinion_of_initiator = recipient.interpersonal_opinions.get(initiator.pawn_id, 0.0)
            power = params.conversion_power_base * (1.0 + 0.005 * opinion_of_initiator)
            power *= float(rng.uniform(0.8, 1.2))
            power = max(0.0, power)

            recipient.certainty = float(np.clip(recipient.certainty - 0.04 * power, 0.0, 1.0))

            if rng.random() < 0.2:
                # Draw / mutual disillusionment — mirrors IdeologicalDebateMeme draw path.
                # Both pawns lose personal opinion of the other's ideo; neither is convinced.
                recipient.personal_opinions[initiator.ideo_id] = (
                    recipient.personal_opinions.get(initiator.ideo_id, 0.0) - 3.0 * power
                )
                initiator.personal_opinions[recipient.ideo_id] = (
                    initiator.personal_opinions.get(recipient.ideo_id, 0.0) - 3.0 * power
                )
            else:
                # Normal conversion pressure — recipient nudged toward initiator's ideo
                recipient.personal_opinions[initiator.ideo_id] = (
                    recipient.personal_opinions.get(initiator.ideo_id, 0.0) + 8.0 * power
                )
                recipient.personal_opinions[recipient.ideo_id] = (
                    recipient.personal_opinions.get(recipient.ideo_id, 0.0) - 2.0 * power
                )

            if power > 0 and recipient.certainty <= CONVERSION_CERTAINTY_THRESHOLD:
                _check_conversion(recipient, ideo_ids, rng, priority_ideo=initiator.ideo_id)

    return pd.DataFrame(rows)
