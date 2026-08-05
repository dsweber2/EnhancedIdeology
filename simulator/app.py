"""EnhancedBeliefs belief dynamics simulator. Run with: uv run marimo run app.py"""

import marimo as mo

app = mo.App(width="full")


@app.cell
def _():
    import marimo as mo
    return (mo,)


@app.cell
def _(mo):
    mode = mo.ui.radio(
        ["Python Sim", "C# Results"],
        value="C# Results",
        label="Mode",
        inline=True,
    )
    mo.vstack([mo.md("## EnhancedBeliefs — Belief Dynamics Simulator"), mode])
    return (mode,)


# ── Python Sim ───────────────────────────────────────────────────────────────

@app.cell
def _(mo, mode):
    mo.stop(mode.value != "Python Sim")
    num_pawns        = mo.ui.slider(2,   30,   value=10,   step=1,    label="Pawns")
    num_ideos        = mo.ui.slider(2,   5,    value=3,    step=1,    label="Ideologies")
    sim_days         = mo.ui.slider(30,  365,  value=120,  step=10,   label="Days")
    interact_rate    = mo.ui.slider(0.0, 3.0,  value=0.3,  step=0.1,  label="Interactions / pawn / day")
    conv_power       = mo.ui.slider(0.1, 2.0,  value=0.5,  step=0.1,  label="Conversion power")
    base_happiness   = mo.ui.slider(0.5, 1.0,  value=0.85, step=0.05, label="Base happiness")
    init_certainty   = mo.ui.slider(0.3, 1.0,  value=0.75, step=0.05, label="Initial certainty")
    ideo_mood_spread = mo.ui.slider(0.0, 20.0, value=0.0,  step=1.0,  label="Ideo mood spread")
    py_seed          = mo.ui.slider(0,   9999, value=42,   step=1,    label="Seed")
    mo.vstack([
        mo.hstack([num_pawns, num_ideos, sim_days, py_seed], justify="start"),
        mo.hstack([interact_rate, conv_power, base_happiness, init_certainty, ideo_mood_spread], justify="start"),
        mo.md("_Ideo mood spread: std dev of each ideology's net precept mood offset. 0 = all ideos identical; higher = some are feel-good, others punishing._"),
    ])
    return (num_pawns, num_ideos, sim_days, interact_rate, conv_power,
            base_happiness, init_certainty, ideo_mood_spread, py_seed)


@app.cell
def _(mo, mode, num_pawns, num_ideos, sim_days, interact_rate, conv_power,
       base_happiness, init_certainty, ideo_mood_spread, py_seed):
    mo.stop(mode.value != "Python Sim")
    from sim import SimParams, run_simulation
    py_df = run_simulation(SimParams(
        num_pawns=num_pawns.value,
        num_ideos=num_ideos.value,
        sim_days=sim_days.value,
        interaction_rate=interact_rate.value,
        conversion_power_base=conv_power.value,
        base_happiness_mean=base_happiness.value,
        initial_certainty_mean=init_certainty.value,
        ideo_mood_spread=ideo_mood_spread.value,
        seed=py_seed.value,
    ))
    return (py_df,)


@app.cell
def _(mo, mode, py_df):
    mo.stop(mode.value != "Python Sim")
    import plotly.express as _px

    _df_plot = py_df.copy()
    _df_plot["pawn"] = "Pawn " + _df_plot["pawn_id"].astype(str)
    _df_plot["ideo"] = "Ideo " + _df_plot["ideo_id"].astype(str)

    _fig_certainty = _px.line(
        _df_plot, x="day", y="certainty",
        color="pawn", line_dash="ideo",
        title="Certainty over time",
        labels={"certainty": "Certainty", "day": "Day", "pawn": "Pawn", "ideo": "Ideology"},
        range_y=[0, 1],
    )
    _fig_certainty.update_layout(legend=dict(orientation="h", yanchor="bottom", y=1.02))

    _fig_opinion = _px.line(
        _df_plot, x="day", y="own_opinion",
        color="pawn", line_dash="ideo",
        title="Opinion of own ideology over time",
        labels={"own_opinion": "Opinion", "day": "Day", "pawn": "Pawn", "ideo": "Ideology"},
        range_y=[0, 1],
    )
    _fig_opinion.update_layout(legend=dict(orientation="h", yanchor="bottom", y=1.02))

    _ideo_counts = (
        py_df.groupby(["day", "ideo_id"])
        .size()
        .reset_index(name="count")
    )
    _ideo_counts["ideo"] = "Ideo " + _ideo_counts["ideo_id"].astype(str)
    _fig_distribution = _px.area(
        _ideo_counts, x="day", y="count", color="ideo",
        title="Ideology headcount over time",
        labels={"count": "Pawns", "day": "Day", "ideo": "Ideology"},
    )

    _offsets = (
        py_df[py_df["day"] == 0]
        .drop_duplicates("ideo_id")[["ideo_id", "ideo_mood_offset"]]
        .sort_values("ideo_id")
    )
    _offsets_md = "**Ideo mood offsets this run:** " + ", ".join(
        f"Ideo {int(row.ideo_id)}: {row.ideo_mood_offset:+.1f}"
        for row in _offsets.itertuples()
    )

    mo.vstack([
        mo.md(_offsets_md),
        mo.ui.plotly(_fig_certainty),
        mo.ui.plotly(_fig_opinion),
        mo.ui.plotly(_fig_distribution),
    ])


# ── C# Results ───────────────────────────────────────────────────────────────

@app.cell
def _(mo, mode):
    mo.stop(mode.value != "C# Results")
    cs_scenario = mo.ui.dropdown(
        ["ConversionPressure", "BaselineStability", "MemeDebateConflict"],
        value="ConversionPressure",
        label="Scenario",
    )
    cs_runs    = mo.ui.slider(1, 20, value=5,  step=1,    label="Runs")
    cs_seed    = mo.ui.slider(0, 9999, value=42, step=1,  label="Start seed")
    cs_out_dir = mo.ui.text(value="results",               label="Output directory")
    cs_run_btn = mo.ui.button(
        label="▶ Run C# Sim",
        on_click=lambda count: (count or 0) + 1,
        value=0,
    )
    mo.vstack([
        mo.hstack([cs_scenario, cs_runs, cs_seed, cs_out_dir], justify="start"),
        cs_run_btn,
        mo.md("_Results are loaded automatically from the output directory. Click 'Run C# Sim' to generate fresh results._"),
    ])
    return (cs_scenario, cs_runs, cs_seed, cs_out_dir, cs_run_btn)


@app.cell
def _(mo, mode, cs_scenario, cs_runs, cs_seed, cs_out_dir, cs_run_btn):
    mo.stop(mode.value != "C# Results")
    import os
    import glob
    import json
    import subprocess
    import pandas as pd

    sim_dir = os.path.dirname(os.path.abspath(__file__))
    proj    = os.path.join(sim_dir, "EnhancedBeliefs.Sim", "EnhancedBeliefs.Sim.csproj")
    out_abs = os.path.join(sim_dir, cs_out_dir.value)

    run_msg = ""
    if cs_run_btn.value:
        cmd = [
            "dotnet", "run", "--project", proj, "--",
            "--scenario", cs_scenario.value,
            "--runs",     str(cs_runs.value),
            "--seed",     str(cs_seed.value),
            "--out",      out_abs,
        ]
        with mo.status.spinner(f"Running {cs_scenario.value} x{cs_runs.value}…"):
            result = subprocess.run(cmd, capture_output=True, text=True, cwd=sim_dir)
        if result.returncode == 0:
            run_msg = f"✓ {result.stdout.strip()}"
        else:
            run_msg = f"✗ Sim failed:\n```\n{result.stderr}\n```"

    jsonl_files = sorted(glob.glob(os.path.join(out_abs, cs_scenario.value, "seed_*.jsonl")))

    rows = []
    for _seed_idx, _fpath in enumerate(jsonl_files):
        with open(_fpath) as _fh:
            for _line in _fh:
                _rec = json.loads(_line)
                _rec["seed"] = _seed_idx
                rows.append(_rec)

    cs_df = pd.DataFrame(rows) if rows else pd.DataFrame()
    cs_run_status = run_msg
    cs_load_summary = (
        f"Loaded **{len(jsonl_files)} run(s)**, {len(rows):,} records from `{out_abs}/{cs_scenario.value}/`."
        if rows else ""
    )
    return (cs_df, cs_run_status, cs_load_summary)


@app.cell
def _(mo, cs_df, cs_run_status, cs_load_summary):
    _no_data = cs_df is None or len(cs_df) == 0
    _status_lines = [s for s in [cs_run_status, cs_load_summary] if s]
    _status = mo.md("\n\n".join(_status_lines)) if _status_lines else None

    mo.stop(
        _no_data,
        mo.vstack([_status, mo.md("_No results found. Adjust the output directory or click 'Run C# Sim'._")])
        if _status else mo.md("_No results found. Adjust the output directory or click 'Run C# Sim'._"),
    )
    import numpy as np
    import plotly.graph_objects as go

    _COLORS = [
        "#636EFA", "#EF553B", "#00CC96", "#AB63FA", "#FFA15A",
        "#19D3F3", "#FF6692", "#B6E880", "#FF97FF", "#FECB52",
    ]

    def _quantile_stats(df, group_cols, value_col):
        q = (
            df.groupby(group_cols)[value_col]
            .quantile([0.25, 0.5, 0.75])
            .unstack()
        )
        q.columns = ["q25", "median", "q75"]
        return q.reset_index()

    def _band_traces(stats, ideos, color_map, x_col="day"):
        traces = []
        for ideo in ideos:
            sub = stats[stats["ideoName"] == ideo].sort_values(x_col)
            color = color_map[ideo]
            xs = sub[x_col].tolist()
            traces.append(go.Scatter(
                x=xs + xs[::-1],
                y=sub["q75"].tolist() + sub["q25"].tolist()[::-1],
                fill="toself", fillcolor=color, opacity=0.15,
                line=dict(width=0), showlegend=False, hoverinfo="skip",
            ))
            traces.append(go.Scatter(
                x=xs, y=sub["median"].tolist(),
                name=ideo, line=dict(color=color), mode="lines",
            ))
        return traces

    _layout_defaults = dict(
        legend=dict(orientation="h", yanchor="bottom", y=1.02),
        xaxis_title="Day",
    )

    _ideos = sorted(cs_df["ideoName"].unique())
    _color_map = {ideo: _COLORS[ii % len(_COLORS)] for ii, ideo in enumerate(_ideos)}

    _cert_stats = _quantile_stats(cs_df, ["day", "ideoName"], "certainty")
    _fig_cert = go.Figure(_band_traces(_cert_stats, _ideos, _color_map))
    _fig_cert.update_layout(
        title="Certainty over time (median ± IQR)",
        yaxis=dict(title="Certainty", range=[0, 1]),
        **_layout_defaults,
    )

    _opinion_stats = _quantile_stats(cs_df, ["day", "ideoName"], "baseOpinion")
    _fig_opinion = go.Figure(_band_traces(_opinion_stats, _ideos, _color_map))
    _fig_opinion.update_layout(
        title="Base opinion of own ideology (median ± IQR)",
        yaxis=dict(title="Opinion", range=[0, 1]),
        **_layout_defaults,
    )

    _headcount_per_seed = (
        cs_df.groupby(["day", "ideoName", "seed"])["pawnId"]
        .nunique()
        .reset_index(name="count")
    )
    _headcount_stats = _quantile_stats(_headcount_per_seed, ["day", "ideoName"], "count")
    _fig_dist = go.Figure(_band_traces(_headcount_stats, _ideos, _color_map))
    _fig_dist.update_layout(
        title="Ideology headcount over time (median ± IQR)",
        yaxis_title="Pawns",
        **_layout_defaults,
    )

    _header_parts = [s for s in [cs_run_status, cs_load_summary] if s]
    _header = [mo.md("\n\n".join(_header_parts))] if _header_parts else []
    mo.vstack(_header + [
        mo.ui.plotly(_fig_cert),
        mo.ui.plotly(_fig_opinion),
        mo.ui.plotly(_fig_dist),
    ])
