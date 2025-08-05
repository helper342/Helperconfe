using Content.Server.Beam;
using Content.Server.Damage.Systems;
using Content.Shared.Actions;
using Content.Shared.Backmen.Abilities.Psionics;
using Content.Shared.Backmen.Psionics;
using Content.Shared.Backmen.Psionics.Events;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.Backmen.Abilities.Psionics;

public sealed class KamehamehaSystem : SharedKamehamehaSystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
    [Dependency] private readonly BeamSystem _beam = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<KamehamehaComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<KamehamehaComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<KamehamehaActionEvent>(OnPowerUsed);
    }

    [ValidatePrototypeId<EntityPrototype>] private const string ActionKamehameha = "ActionKamehameha";

    private void OnInit(EntityUid uid, KamehamehaComponent component, ComponentInit args)
    {
        _actions.AddAction(uid, ref component.KamehamehaAction, ActionKamehameha);

        if (TryComp<PsionicComponent>(uid, out var psionic) && psionic.PsionicAbility == null)
            psionic.PsionicAbility = component.KamehamehaAction;
    }

    private void OnShutdown(EntityUid uid, KamehamehaComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.KamehamehaAction);
    }

    private void OnPowerUsed(KamehamehaActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<KamehamehaComponent>(args.Performer, out var component))
            return;

        // Create the Kamehameha beam
        _beam.TryCreateBeam(args.Performer, args.Target, component.BeamPrototype);

        // Deal damage to the target
        var damageSpec = new DamageSpecifier();
        damageSpec.DamageDict.Add("Heat", component.Damage);
        _damage.TryChangeDamage(args.Target, damageSpec, origin: args.Performer);

        // Log the power usage
        _psionics.LogPowerUsed(args.Performer, "kamehameha", minGlimmer: 15, maxGlimmer: 25);
        args.Handled = true;

        // Set cooldown
        if (_actions.GetAction(component.KamehamehaAction) is { } action)
        {
            _actions.SetCooldown(component.KamehamehaAction, action.Comp.UseDelay ?? TimeSpan.FromMinutes(2));
        }
    }
}