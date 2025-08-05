namespace Content.Shared.Backmen.Abilities.Psionics;

[RegisterComponent]
public sealed partial class KamehamehaComponent : Component
{
    [DataField]
    public int Damage = 50;

    [DataField]
    public float Range = 8.0f;

    [DataField]
    public string BeamPrototype = "KamehamehaBeam";

    public EntityUid? KamehamehaAction = null;
}