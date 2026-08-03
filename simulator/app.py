"""EnhancedBeliefs belief dynamics simulator. Run with: uv run marimo run app.py"""

import marimo as mo

app = mo.App(width="full")


@app.cell
def _():
    import marimo as mo
    return (mo,)


@app.cell
def _(mo):
    num_pawns        = mo.ui.slider(2,   30,   value=10,   step=1,    label="Pawns")
    num_ideos        = mo.ui.slider(2,   5,    value=3,    step=1,    label="Ideologies")
    sim_days         = mo.ui.slider(30,  365,  value=120,  step=10,   label="Days")
    interact_rate    = mo.ui.slider(0.0, 3.0,  value=0.3,  step=0.1,  label="Interactions / pawn / day")
    conv_power       = mo.ui.slider(0.1, 2.0,  value=0.5,  step=0.1,  label="Conversion power")
    base_happiness   = mo.ui.slider(0.5, 1.0,  value=0.85, step=0.05, label="Base happiness")
    init_certainty   = mo.ui.slider(0.3, 1.0,  value=0.75, step=0.05, label="Initial certainty")
    ideo_mood_spread = mo.ui.slider(0.0, 20.0, value=0.0,  step=1.0,  label="Ideo mood spread")
    seed             = mo.ui.slider(0,   9999, value=42,   step=1,    label="Seed")

    mo.vstack([
        mo.md("## EnhancedBeliefs — Belief Dynamics Simulator"),
        mo.hstack([num_pawns, num_ideos, sim_days, seed], justify="start"),
        mo.hstack([interact_rate, conv_power, base_happiness, init_certainty, ideo_mood_spread], justify="start"),
        mo.md("_Ideo mood spread: std dev of each ideology's net precept mood offset. 0 = all ideos identical; higher = some are feel-good, others punishing._"),
    ])
    return (num_pawns, num_ideos, sim_days, interact_rate, conv_power, base_happiness, init_certainty, ideo_mood_spread, seed)


@app.cell
def _(num_pawns, num_ideos, sim_days, interact_rate, conv_power, base_happiness, init_certainty, ideo_mood_spread, seed):
    from sim import SimParams, run_simulation

    params = SimParams(
        num_pawns=num_pawns.value,
        num_ideos=num_ideos.value,
        sim_days=sim_days.value,
        interaction_rate=interact_rate.value,
        conversion_power_base=conv_power.value,
        base_happiness_mean=base_happiness.value,
        initial_certainty_mean=init_certainty.value,
        ideo_mood_spread=ideo_mood_spread.value,
        seed=seed.value,
    )
    df = run_simulation(params)
    return (df, params)


@app.cell
def _(df, mo):
    import plotly.express as px

    df_plot = df.copy()
    df_plot["pawn"] = "Pawn " + df_plot["pawn_id"].astype(str)
    df_plot["ideo"] = "Ideo " + df_plot["ideo_id"].astype(str)

    fig_certainty = px.line(
        df_plot,
        x="day", y="certainty",
        color="pawn", line_dash="ideo",
        title="Certainty over time",
        labels={"certainty": "Certainty", "day": "Day", "pawn": "Pawn", "ideo": "Ideology"},
        range_y=[0, 1],
    )
    fig_certainty.update_layout(legend=dict(orientation="h", yanchor="bottom", y=1.02))

    fig_opinion = px.line(
        df_plot,
        x="day", y="own_opinion",
        color="pawn", line_dash="ideo",
        title="Opinion of own ideology over time",
        labels={"own_opinion": "Opinion", "day": "Day", "pawn": "Pawn", "ideo": "Ideology"},
        range_y=[0, 1],
    )
    fig_opinion.update_layout(legend=dict(orientation="h", yanchor="bottom", y=1.02))

    ideo_counts = (
        df.groupby(["day", "ideo_id"])
        .size()
        .reset_index(name="count")
    )
    ideo_counts["ideo"] = "Ideo " + ideo_counts["ideo_id"].astype(str)
    fig_distribution = px.area(
        ideo_counts,
        x="day", y="count", color="ideo",
        title="Ideology headcount over time",
        labels={"count": "Pawns", "day": "Day", "ideo": "Ideology"},
    )

    offsets = (
        df[df["day"] == 0]
        .drop_duplicates("ideo_id")[["ideo_id", "ideo_mood_offset"]]
        .sort_values("ideo_id")
    )
    offsets_md = "**Ideo mood offsets this run:** " + ", ".join(
        f"Ideo {int(row.ideo_id)}: {row.ideo_mood_offset:+.1f}"
        for row in offsets.itertuples()
    )

    mo.vstack([
        mo.md(offsets_md),
        mo.ui.plotly(fig_certainty),
        mo.ui.plotly(fig_opinion),
        mo.ui.plotly(fig_distribution),
    ])
