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
using System.Text.Json;

public class SimpleMenuPlugin : BasePlugin
{
    public override string ModuleName => "SimpleMenuPlugin";
    public override string ModuleVersion => "1.0.6";
    public override string ModuleAuthor => "sn1ce";
    public override string ModuleDescription => "Menu & Workshop switching via chat";

    private bool isBhopEnabled = false;
    private Dictionary<string, MenuDefinition> customMenus = new();

    public override void Load(bool hotReload)
    {
        string pluginDir = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "SimpleMenuPlugin");
        string menuFilePath = Path.Combine(pluginDir, "menus.json");

        if (!Directory.Exists(pluginDir))
        {
            Directory.CreateDirectory(pluginDir);
        }

        if (!File.Exists(menuFilePath))
        {
            File.WriteAllText(menuFilePath, """
        [
        {
            "Command": "!tz_menu",
            "Title": "TZ Menu",
            "Items": [
            {
                "Label": "Restart Match",
                "Command": "mp_restartgame 1",
                "Message": "Match restarting..."
            },
            {
                "Label": "Change to Inferno",
                "Command": "changelevel de_inferno",
                "Message": "Changing to de_inferno..."
            }
            ]
        },
        {
            "Command": "!fun_menu",
            "Title": "Fun Stuff",
            "Items": [
            {
                "Label": "Enable Cheats",
                "Command": "sv_cheats 1",
                "Message": "Cheats activated!"
            },
            {
                "Label": "Give Deagle",
                "Command": "give weapon_deagle",
                "Message": "Enjoy your Deagle!"
            }
            ]
        }
        ]
        """);
        }
        LoadCustomMenus();

        AddCommandListener("say", (player, command) =>
        {
            if (player == null) return HookResult.Continue;

            var msg = command.GetArg(1).ToLower();

            if (msg == "!sn_menu") ShowMainMenu(player);
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
            else if (customMenus.TryGetValue(msg, out var customMenu))
            {
                ShowCustomMenu(player, customMenu);
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

        Console.WriteLine(@"
          _____  __  __ ____  
         / ____||  \/  |  _ \ 
        | (___  | \  / | |_) |
         \___ \ | |\/| |  _ < 
         ____) || |  | | |_) |
        |_____/ |_|  |_|____/ 
        SIMPLE MENU PLUGIN by sn1ce
        ");
    }

    private void LoadCustomMenus()
    {
        string configDir = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "SimpleMenuPlugin");
        string jsonPath = Path.Combine(configDir, "menus.json");

        if (!File.Exists(jsonPath))
        {
            Console.WriteLine($"[SimpleMenuPlugin] menus.json not found at {jsonPath}");
            return;
        }

        try
        {
            var json = File.ReadAllText(jsonPath);
            var menus = JsonSerializer.Deserialize<List<MenuDefinition>>(json);
            if (menus != null)
            {
                foreach (var menu in menus)
                {
                    if (!string.IsNullOrWhiteSpace(menu.Command))
                    {
                        customMenus[menu.Command.ToLower()] = menu;
                        Console.WriteLine($"[SimpleMenuPlugin] Loaded dynamic menu: {menu.Command}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SimpleMenuPlugin] Error loading menus.json: {ex.Message}");
        }
    }

    private void ShowCustomMenu(CCSPlayerController player, MenuDefinition menuDef)
    {
        var menu = new WasdMenu(menuDef.Title ?? "Custom Menu", this);

        foreach (var item in menuDef.Items)
        {
            menu.AddItem(item.Label, (p, o) =>
            {
                Server.ExecuteCommand(item.Command);
                Server.PrintToChatAll($"! {ChatColors.Red}[INFO] {ChatColors.Green}{item.Message}");
            });
        }

        menu.Display(player, 20);
    }

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
            Server.ExecuteCommand(isBhopEnabled ? "exec d_bhop" : "exec bhop");
            Server.PrintToChatAll($"! {ChatColors.Red}[INFO] {ChatColors.Green}BHOP {(isBhopEnabled ? "disabled" : "enabled")}.");
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
            "Hide n Seek" => new[] { ("inferno Hide n Seek", "3097563690"), ("Kindergarten", "3464275170") },
            "AIM" => new[] { ("aim_map", "3084291314"), ("AWP Lego 2", "3146105097") },
            "Surf" => new[] { ("surf_utopia_njv", "3073875025"), ("surf_boreas", "3133346713") },
            "Fun" => new[] {
                ("Mirage Bricks", "3464733042"), ("Danger Zone Skining Island", "3462095803"),
                ("Extraction", "3460964702"), ("De_cache", "3437809122"), ("Flick", "3423073369")
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

    public class MenuDefinition
    {
        public string Command { get; set; } = string.Empty;
        public string Title { get; set; } = "Menu";
        public List<MenuItem> Items { get; set; } = new();
    }

    public class MenuItem
    {
        public string Label { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}