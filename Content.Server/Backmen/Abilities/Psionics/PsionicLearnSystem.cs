using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared.DoAfter;
using Content.Shared.Backmen.Abilities.Psionics;
using Content.Shared.Backmen.Abilities.Psionics.Events;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Robust.Shared.Audio.Systems;

namespace Content.Server.Backmen.Abilities.Psionics;

public sealed class PsionicLearnSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PsionicAbilitiesSystem _psionics = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PsionicLearnComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<PsionicLearnComponent, PsionicLearnDoAfterEvent>(OnUsed, after: new []{typeof(DoAfterSystem)});
        SubscribeLocalEvent<PsionicLearnComponent, ExaminedEvent>(OnExamine);
    }

    private void OnUseInHand(EntityUid uid, PsionicLearnComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        var usesRemaining = component.GetUsesRemaining();

        if (usesRemaining <= 0)
        {
            _popup.PopupEntity(Loc.GetString("psionic-item-no-uses"), uid, args.User);
            return;
        }

        // Check if user already has this ability
        if (HasComp(args.User, component.AbilityComponent))
        {
            _popup.PopupEntity(Loc.GetString("psionic-item-already-knows"), uid, args.User);
            return;
        }

        args.Handled = true;

        var doAfterEvent = new PsionicLearnDoAfterEvent();
        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, component.DoAfterDuration, doAfterEvent, uid)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true
        };

        _doAfterSystem.TryStartDoAfter(doAfterArgs);
        _audio.PlayPvs(component.UseSound, uid);
    }

    private void OnUsed(EntityUid uid, PsionicLearnComponent component, PsionicLearnDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        // Check if user already has this ability
        if (HasComp(args.User, component.AbilityComponent))
        {
            _popup.PopupEntity(Loc.GetString("psionic-item-already-knows"), uid, args.User);
            return;
        }

        // Grant the psionic ability
        _psionics.AddPsionics(args.User, component.AbilityComponent);
        _popup.PopupEntity(Loc.GetString("psionic-item-learned"), uid, args.User);
        _audio.PlayPvs(component.UseSound, uid);

        var usesRemaining = component.GetUsesRemaining();
        usesRemaining--;

        component.UsesRemaining = usesRemaining;
        Dirty(uid, component);

        if (component.DeleteAfterUse && usesRemaining <= 0)
        {
            EntityManager.QueueDeleteEntity(uid);
        }
    }

    private void OnExamine(EntityUid uid, PsionicLearnComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var usesRemaining = component.GetUsesRemaining();
        args.PushMarkup(Loc.GetString("psionic-item-uses-remaining", ("uses", usesRemaining)));
    }
}