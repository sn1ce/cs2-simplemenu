# Simple Menu Plugin

A Counter-Strike 2 plugin that adds an in-game menu for players to perform simple actions via chat commands. This plugin is developed for an internal server which provides the functionality to change maps, toggle BHop, restart rounds, and more.  
It's designed for community use—so everyone can control things without needing admin permissions. Powered by [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) and [CS2MenuManager](https://github.com/schwarper/CS2MenuManager).

---

## ✅ Requirements

- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) (latest version)
- CS2MenuManager dependency (automatically handled via NuGet or manually included)
---

## 🎮 What does it do?

This plugin adds a customizable in-game menu, accessible via chat, to perform various actions such as:

- 🌐 Switching to default or workshop maps
- 🐰 Enabling/disabling BHop config
- 🗺️ Browsing map categories (Fun, AIM, Surf, Hide & Seek)
- 🛠️ Easily expandable with more menu options

Commands available:

- `!sn_menu` – Opens the main menu
- `!ws <workshop_id>` – Instantly changes to a workshop map

---

## 📦 Installation

1. **Build the plugin** (or download the release ZIP):
   - Clone the repo or download a release
   - Run:  
     ```bash
     dotnet build simplemenuplugin.csproj -c Release
     ```
   - Output: `SimpleMenuPlugin.dll` and dependencies

2. **Copy to your server:**
   - Upload the folder `plugins/SimpleMenuPlugin` to:
     ```
     csgo/addons/counterstrikesharp/plugins/
     ```

3. **Restart your CS2 server**

4. ✅ Done! Join and type `!sn_menu` to use it.

---

## 🛠️ Configuration

No additional config is required. Permissions are open by default, but you can restrict actions using CounterStrikeSharp's admin system if needed.

---

## 📜 License

MIT License

---

Contributions, ideas, and workshop map suggestions are always welcome. 🎉