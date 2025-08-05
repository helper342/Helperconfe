using Content.Server.Actions;
using Content.Server.Administration;
using Content.Server.Backmen.Abilities.Psionics;
using Content.Shared.Administration;
using Content.Shared.Backmen.Abilities.Psionics;
using Content.Shared.Mobs.Components;
using Robust.Shared.Console;
using Robust.Shared.Player;

namespace Content.Server.Backmen.Psionics;

[AdminCommand(AdminFlags.Logs)]
public sealed class ListPsionicsCommand : IConsoleCommand
{
    public string Command => "lspsionics";
    public string Description => Loc.GetString("command-lspsionic-description");
    public string Help => Loc.GetString("command-lspsionic-help");
    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        var action = entMan.System<ActionsSystem>();
        foreach (var (actor, mob, psionic, meta) in entMan.EntityQuery<ActorComponent, MobStateComponent, PsionicComponent, MetaDataComponent>())
        {
            entMan.TryGetComponent<MetaDataComponent>(psionic.PsionicAbility, out var skill);
            // filter out xenos, etc, with innate telepathy
            if (skill != null)
                shell.WriteLine(meta.EntityName + " (" + meta.Owner + ") - " + actor.PlayerSession.Name + " - " + Loc.GetString(skill.EntityName));
        }
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class AddKamehamehaCommand : IConsoleCommand
{
    public string Command => "addkamehameha";
    public string Description => "Grants Kamehameha ability to a player";
    public string Help => "addkamehameha <player>";
    
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteLine("Usage: addkamehameha <player>");
            return;
        }

        var entMan = IoCManager.Resolve<IEntityManager>();
        var psionicSystem = entMan.System<PsionicAbilitiesSystem>();
        
        if (!entMan.TryGetPlayerData(args[0], out var data))
        {
            shell.WriteLine("Player not found");
            return;
        }

        if (data.ContentData()?.Mind?.CurrentEntity is not { } entity)
        {
            shell.WriteLine("Player has no entity");
            return;
        }

        psionicSystem.AddPsionics(entity, "KamehamehaComponent");
        shell.WriteLine($"Granted Kamehameha ability to {args[0]}");
    }
}
