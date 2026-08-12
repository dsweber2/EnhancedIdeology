namespace EnhancedIdeology.Tests;

// Covers the cross-precept "Interactions" couplings (preceptPolicy.md): induced stances, where holding one
// precept implies a stance on another issue, and directional penalties for single-rung target issues.
public class InteractionCouplingTests : SeededTest
{
    [Fact]
    public void InducedRank_TreesDesired_ImpliesDisapprovingTreeCutting()
    {
        var (treeCutting, _) = SimIssues.Ladder("TreeCutting",
            "TreeCutting_Disapproved", "TreeCutting_Horrible", "TreeCutting_Prohibited");
        var (_, treeRungs) = SimIssues.Ladder("Trees", "Trees_Desired", "AM_Trees_Despised");
        var ideo = new IdeoBuilder().WithName("TreeLovers").AddPrecept(treeRungs[0]).Build();

        // Valuing trees induces the "disapproved" rung (rank 0) of the tree-cutting ladder.
        Assert.Equal(0f, PreceptPolicy.InducedRank(ideo, treeCutting));
        Assert.Contains(treeCutting, PreceptPolicy.InducedIssues(ideo));
    }

    [Fact]
    public void InducedRank_TreesDespised_SitsBeyondDontCare()
    {
        var (treeCutting, _) = SimIssues.Ladder("TreeCutting",
            "TreeCutting_Disapproved", "TreeCutting_Horrible", "TreeCutting_Prohibited");
        var (_, treeRungs) = SimIssues.Ladder("Trees", "Trees_Desired", "AM_Trees_Despised");
        var ideo = new IdeoBuilder().WithName("TreeHaters").AddPrecept(treeRungs[1]).Build();

        // TreeCutting has no Don't-care entry, so its Don't-care rank is -1; despising trees sits one step
        // further into the permissive end, -2.
        Assert.Equal(-2f, PreceptPolicy.InducedRank(ideo, treeCutting));
    }

    [Fact]
    public void InducedRank_NoCouplingSource_IsNull()
    {
        var (treeCutting, _) = SimIssues.Ladder("TreeCutting", "TreeCutting_Disapproved");
        var ideo = new IdeoBuilder().WithName("Indifferent").Build();

        Assert.Null(PreceptPolicy.InducedRank(ideo, treeCutting));
        Assert.Empty(PreceptPolicy.InducedIssues(ideo));
    }

    [Fact]
    public void CouplingPenalty_MechanoidDespiser_PenalizesEnhancerOneWay()
    {
        var (_, mlRungs) = SimIssues.Ladder("MechanoidLabor", "MechanoidLabor_Enhanced");
        var (_, mechRungs) = SimIssues.Ladder("VME_Mechanoids",
            "VME_Mechanoids_Despised", "VME_Mechanoids_Exalted");
        var despiser = new IdeoBuilder().WithName("MechHaters").AddPrecept(mechRungs[0]).Build();
        var enhancer = new IdeoBuilder().WithName("Enhancers").AddPrecept(mlRungs[0]).Build();

        // The despiser sours on the enhancer, scaled by their conviction on the mechanoid issue; not mutual.
        Assert.Equal(12f, PreceptPolicy.CouplingPenalty(despiser, enhancer, _ => 12f));
        Assert.Equal(0f, PreceptPolicy.CouplingPenalty(enhancer, despiser, _ => 12f));
    }

    [Fact]
    public void CouplingPenalty_DiversityValuer_PenalizesXenotypeSupremacist()
    {
        var (_, divRungs) = SimIssues.Ladder("IdeoDiversity",
            "IdeoDiversity_Abhorrent", "IdeoDiversity_Standard", "IdeoDiversity_Approved");
        var xeno = SimIssues.Special("PreferredXenotypes");
        var tolerant = new IdeoBuilder().WithName("Tolerant").AddPrecept(divRungs[2]).Build();
        var neutral = new IdeoBuilder().WithName("Neutral").AddPrecept(divRungs[1]).Build();
        var supremacist = new IdeoBuilder().WithName("Supremacists")
            .AddXenotypePrecept(xeno, new XenotypeDef { defName = "Hussar" }).Build();

        // Appreciating diversity sours a faith on any xenotype supremacist, scaled by conviction; the neutral
        // diversity rung does not, and the direction is one-way.
        Assert.Equal(9f, PreceptPolicy.CouplingPenalty(tolerant, supremacist, _ => 9f));
        Assert.Equal(0f, PreceptPolicy.CouplingPenalty(neutral, supremacist, _ => 9f));
        Assert.Equal(0f, PreceptPolicy.CouplingPenalty(supremacist, tolerant, _ => 9f));
    }

    [Fact]
    public void InducedStance_AddsAxis_DeepeningStructuralDisagreement()
    {
        Rand.SetSeed(1);
        var world = new SimWorld();
        world.Initialize();

        SimIssues.Ladder("TreeCutting", "TreeCutting_Disapproved", "TreeCutting_Horrible", "TreeCutting_Prohibited");
        var (_, treeRungs) = SimIssues.Ladder("Trees", "Trees_Desired", "AM_Trees_Despised");
        var loverIdeo = new IdeoBuilder().WithName("TreeLovers").AddPrecept(treeRungs[0]).Build();
        var haterIdeo = new IdeoBuilder().WithName("TreeHaters").AddPrecept(treeRungs[1]).Build();
        world.AddIdeo(loverIdeo);
        world.AddIdeo(haterIdeo);

        var pawn = new PawnBuilder().WithIdeo(loverIdeo).WithLabel("P").Build(world);
        var tracker = world.Comp.PawnTracker.EnsurePawnHasIdeoTracker(pawn);

        // The lover and hater clash on Trees AND on the induced TreeCutting axis, so both issues push the
        // structural opinion negative - it bottoms out well below the neutral midpoint.
        Assert.True(tracker.StructuralIdeoOpinion(haterIdeo) < 50f);
    }
}
