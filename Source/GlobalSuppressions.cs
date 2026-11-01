// GlobalSuppressions.cs
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>", Scope = "member", Target = "~M:System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute.#ctor(System.String)")]
// Public fields in Thought subclasses are intentional: RimWorld's Scribe system and external Harmony patches need ref access.
[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "RimWorld Scribe/Harmony compatibility", Scope = "member", Target = "~F:EnhancedIdeology.Thought_MemeMemory.SourceMemeLabel")]
[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "RimWorld Scribe/Harmony compatibility", Scope = "member", Target = "~F:EnhancedIdeology.Thought_CognitiveDissonance.StoredMoodOffset")]
// RimWorld's XML loader fills DefModExtension data through public fields, and it reads lists only as List<T>.
[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "RimWorld XML loader", Scope = "type", Target = "~T:EnhancedIdeology.TraitIssueLinks")]
[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "RimWorld XML loader", Scope = "type", Target = "~T:EnhancedIdeology.TraitIssueLink")]
[assembly: SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "RimWorld XML loader", Scope = "member", Target = "~F:EnhancedIdeology.TraitIssueLinks.links")]
