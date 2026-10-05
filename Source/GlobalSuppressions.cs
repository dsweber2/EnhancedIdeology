// GlobalSuppressions.cs
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>", Scope = "member", Target = "~M:System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute.#ctor(System.String)")]
// Public fields in Thought subclasses are intentional: RimWorld's Scribe system and external Harmony patches need ref access.
[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "RimWorld Scribe/Harmony compatibility", Scope = "member", Target = "~F:EnhancedIdeology.Thought_MemeMemory.SourceMemeLabel")]
[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "RimWorld Scribe/Harmony compatibility", Scope = "member", Target = "~F:EnhancedIdeology.Thought_CognitiveDissonance.StoredMoodOffset")]
