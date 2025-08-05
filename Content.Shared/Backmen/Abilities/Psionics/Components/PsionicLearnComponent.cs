using Robust.Shared.Audio;

namespace Content.Shared.Backmen.Abilities.Psionics;

[RegisterComponent]
public sealed partial class PsionicLearnComponent : Component
{
    /// <summary>
    /// The psionic ability component to be learned when the item is used.
    /// </summary>
    [DataField(required: true)]
    public string AbilityComponent { get; set; } = string.Empty;

    /// <summary>
    /// The amount of time it takes to learn the ability.
    /// </summary>
    [DataField]
    public float DoAfterDuration = 5f;

    /// <summary>
    /// The sound to play when the item is used.
    /// </summary>
    [DataField]
    public SoundSpecifier? UseSound = new SoundPathSpecifier("/Audio/Items/Paper/paper_scribble1.ogg");

    /// <summary>
    /// Maximum number of times the item can be used. If 0, the item is single-use.
    /// </summary>
    [DataField]
    public int MaxUses = 1;

    /// <summary>
    /// Whether the item should be deleted after the last use.
    /// </summary>
    [DataField]
    public bool DeleteAfterUse = true;

    /// <summary>
    /// Current number of uses remaining.
    /// </summary>
    [ViewVariables]
    public int? UsesRemaining = null;

    public int GetUsesRemaining()
    {
        return UsesRemaining ?? MaxUses;
    }
}