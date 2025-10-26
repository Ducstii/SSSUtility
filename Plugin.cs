using Exiled.API.Enums;
using Exiled.API.Features;
using SSSUtility.API;
using SSSUtility.Core;
using SSSUtility.Models;

namespace SSSUtility
{
    public class Plugin : Plugin<PluginConfig>
    {
        public override string Name => "SSSUtility";
        public override string Author => "Ducstii";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion => new Version(9, 0, 0);
        public override PluginPriority Priority => PluginPriority.Highest;

        private static Plugin _instance;
        public static Plugin Instance => _instance;

        public override void OnEnabled()
        {
            _instance = this;

            EventManager.Initialize();

            Exiled.Events.Handlers.Player.Verified += OnPlayerVerified;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;

            Log.Info($"[SSSUtility] v{Version} enabled");
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            EventManager.Cleanup();

            Exiled.Events.Handlers.Player.Verified -= OnPlayerVerified;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;

            MenuRegistry.Clear();
            PageManager.Clear();
            ConflictResolver.Reset();

            _instance = null;
            Log.Info("[SSSUtility] Disabled");
            base.OnDisabled();
        }

        private void OnPlayerVerified(Exiled.Events.EventArgs.Player.VerifiedEventArgs ev)
        {
        }

        private void OnWaitingForPlayers()
        {
            PageManager.Clear();
        }
    }

    public class PluginConfig : Exiled.API.Interfaces.IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
    }
}

public static class SSSUtilityAPI
{
    public static void RegisterMenu(string pluginName, SSSUtility.Models.Menu menu)
    {
        if (SSSUtility.Plugin.Instance == null)
        {
            Log.Error("[SSSUtility] Cannot register menu - SSSUtility plugin is not loaded!");
            return;
        }

        SSSUtility.API.ConflictResolver.AssignIdsAndRebuild(menu);
        SSSUtility.API.MenuRegistry.Register(pluginName, menu);

        Log.Info($"[SSSUtility] Registered menu '{menu.Name}' for plugin '{pluginName}'");
    }

    public static void UnregisterMenu(string pluginName)
    {
        SSSUtility.API.MenuRegistry.Unregister(pluginName);
        SSSUtility.API.ConflictResolver.RebuildDefinedSettings();
    }

    public static SSSUtility.Models.Menu GetMenu(string pluginName)
    {
        return SSSUtility.API.MenuRegistry.GetMenu(pluginName);
    }

    public static SSSUtility.Models.PlayerMenuState GetPlayerState(Player player)
    {
        return SSSUtility.Core.PageManager.GetState(player.ReferenceHub);
    }

    public static void SendMenuToPlayer(Player player, string pluginName, int pageIndex = 0)
    {
        var menu = SSSUtility.API.MenuRegistry.GetMenu(pluginName);
        if (menu == null)
        {
            Log.Warn($"[SSSUtility] No menu registered for plugin '{pluginName}'");
            return;
        }

        SSSUtility.Core.PageManager.SendMenu(player.ReferenceHub, menu, pageIndex);
    }

    public static void SendMenuToAll(string pluginName, int pageIndex = 0)
    {
        var menu = SSSUtility.API.MenuRegistry.GetMenu(pluginName);
        if (menu == null)
        {
            Log.Warn($"[SSSUtility] No menu registered for plugin '{pluginName}'");
            return;
        }

        foreach (var player in Player.List)
        {
            SSSUtility.Core.PageManager.SendMenu(player.ReferenceHub, menu, pageIndex);
        }
    }

    public static void RefreshAllMenus()
    {
        SSSUtility.API.ConflictResolver.RebuildDefinedSettings();
        ServerSpecificSettingsSync.SendToAll();
    }

    public static void UpdateButtonLabel(string pluginName, int settingId, string newLabel, Func<ReferenceHub, bool> filter = null)
    {
        var menu = SSSUtility.API.MenuRegistry.GetMenu(pluginName);
        if (menu != null)
        {
            SSSUtility.Core.UpdateManager.UpdateButtonLabel(menu, settingId, newLabel, filter);
        }
    }

    public static void UpdateButtonHint(string pluginName, int settingId, string newHint, Func<ReferenceHub, bool> filter = null)
    {
        var menu = SSSUtility.API.MenuRegistry.GetMenu(pluginName);
        if (menu != null)
        {
            SSSUtility.Core.UpdateManager.UpdateButtonHint(menu, settingId, newHint, filter);
        }
    }

    public static void UpdateButton(string pluginName, int settingId, string newLabel, string newHint = null, Func<ReferenceHub, bool> filter = null)
    {
        var menu = SSSUtility.API.MenuRegistry.GetMenu(pluginName);
        if (menu != null)
        {
            SSSUtility.Core.UpdateManager.UpdateButton(menu, settingId, newLabel, newHint, filter);
        }
    }
}

