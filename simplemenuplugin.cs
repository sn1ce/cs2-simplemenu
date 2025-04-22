using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Events;
using CS2MenuManager.API.Menu;
using System.Linq;

public class SimpleMenuPlugin : BasePlugin
{
    public override string ModuleName => "SimpleMenuPlugin";
    public override string ModuleVersion => "1.1.0";
    public override string ModuleAuthor => "yourname";
    public override string ModuleDescription => "Menu & Workshop switching via chat";

    public override void Load(bool hotReload)
    {
        AddCommandListener("say", (player, command) =>
        {
            if (player == null)
                return HookResult.Continue;

            var msg = command.GetArg(1).ToLower();

            if (msg == "!sn_menu")
            {
                ShowMainMenu(player);
            }
            else if (msg.StartsWith("!ws"))
            {
                var parts = msg.Split(' ');
                if (parts.Length == 2 && ulong.TryParse(parts[1], out var mapId))
                {
                    Server.PrintToChatAll($"! {ChatColors.Red}[INFO] {ChatColors.Green}Loading workshop map ID {mapId}...");
                    Server.ExecuteCommand($"host_workshop_map {mapId}");
                }
                else
                {
                    player.PrintToChat($"! {ChatColors.Red}[INFO] {ChatColors.Green}Usage: !ws <workshop_id>");
                }
            }

            return HookResult.Continue;
        });

        RegisterEventHandler<EventPlayerTeam>((@event, info) =>
        {
            if (@event.Userid?.IsValid == true && !@event.Userid.IsBot)
            {
                @event.Userid.PrintToChat($"! {ChatColors.Red}[INFO] {ChatColors.Green}Use !sn_menu to open the settings menu.");
                @event.Userid.PrintToChat($"! {ChatColors.Red}[INFO] {ChatColors.Green}Use !ws <workshop_id> to switch to a workshop map.");
            }
            return HookResult.Continue;
        });

        RegisterEventHandler<EventRoundStart>((@event, info) =>
        {
            foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
            {
                player.PrintToChat($"! {ChatColors.Red}[INFO] {ChatColors.Green}Use !sn_menu to open the settings menu.");
                player.PrintToChat($"! {ChatColors.Red}[INFO] {ChatColors.Green}Use !ws <workshop_id> to switch to a workshop map.");
            }
            return HookResult.Continue;
        });
    }

    private bool isBhopEnabled = false;

    private void ShowMainMenu(CCSPlayerController player)
    {
        var menu = new WasdMenu("Simple Actions", this);

        menu.AddItem("Change Maps", (p, o) => ShowMapMenu(p, menu));
        menu.AddItem("Fun Maps", (p, o) => ShowFunCategoryMenu(p, menu));
        menu.AddItem("Vote Restart", (p, o) =>
        {
            Server.ExecuteCommand("mp_restartgame 3");
            Server.PrintToChatAll($"! {ChatColors.Red}[INFO] {ChatColors.Green}Match restarting in 3 seconds!");
        });

        menu.AddItem("TOGGLE Bhop", (p, o) =>
        {
            if (isBhopEnabled)
            {
                Server.ExecuteCommand("exec d_bhop");
                Server.PrintToChatAll($"! {ChatColors.Red}[INFO] {ChatColors.Green}BHOP disabled.");
            }
            else
            {
                Server.ExecuteCommand("exec bhop");
                Server.PrintToChatAll($"! {ChatColors.Red}[INFO] {ChatColors.Green}BHOP enabled.");
            }

            isBhopEnabled = !isBhopEnabled;
        });

        menu.Display(player, 20);
    }


    private void ShowMapMenu(CCSPlayerController player, WasdMenu parentMenu)
    {
        var mapMenu = new WasdMenu("Default Maps", this);

        string[] maps = { "de_dust2", "de_inferno", "de_mirage", "de_overpass", "de_nuke", "de_vertigo", "de_ancient" };

        foreach (var map in maps)
        {
            mapMenu.AddItem(map, (p, o) =>
            {
                Server.ExecuteCommand($"changelevel {map}");
                Server.PrintToChatAll($"! {ChatColors.Red}[INFO] {ChatColors.Green}Changing map to {map}...");
            });
        }

        mapMenu.PrevMenu = parentMenu;
        mapMenu.Display(player, 20);
    }

    private void ShowFunCategoryMenu(CCSPlayerController player, WasdMenu parentMenu)
    {
        var menu = new WasdMenu("Fun Map Categories", this);

        menu.AddItem("Hide n Seek", (p, o) => ShowFunMapMenu(p, "Hide n Seek", menu));
        menu.AddItem("AIM Maps", (p, o) => ShowFunMapMenu(p, "AIM", menu));
        menu.AddItem("Surf", (p, o) => ShowFunMapMenu(p, "Surf", menu));
        menu.AddItem("Fun", (p, o) => ShowFunMapMenu(p, "Fun", menu));

        menu.PrevMenu = parentMenu;
        menu.Display(player, 20);
    }

    private void ShowFunMapMenu(CCSPlayerController player, string category, WasdMenu parentMenu)
    {
        var menu = new WasdMenu($"{category} Maps", this);

        var maps = category switch
        {
            "Hide n Seek" => new[] {
                ("inferno Hide n Seek", "3097563690"),
                ("Kindergarten", "3464275170")
            },
            "AIM" => new[] {
                ("aim_map", "3084291314"),
                ("AWP Lego 2", "3146105097")
            },
            "Surf" => new[] {
                ("surf_utopia_njv", "3073875025"),
                ("surf_boreas", "3133346713")
            },
            "Fun" => new[] {
                ("Mirage Bricks", "3464733042"),
                ("Danger Zone Skining Island", "3462095803"),
                ("Extraction", "3460964702"),
                ("De_cache", "3437809122"),
                ("Flick", "3423073369")
            },
            _ => Array.Empty<(string, string)>()
        };

        foreach (var (name, id) in maps)
        {
            menu.AddItem(name, (p, o) =>
            {
                Server.ExecuteCommand($"host_workshop_map {id}");
                Server.PrintToChatAll($"! {ChatColors.Red}[INFO] {ChatColors.Green}Loading {name}...");
            });
        }

        menu.PrevMenu = parentMenu;
        menu.Display(player, 20);
    }
}
