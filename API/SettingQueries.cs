namespace SSSUtility.API;

using SSSUtility.Models;

public static class SettingQueries
{
    public static T GetPlayerSettingValue<T>(Player player, int settingId) where T : ServerSpecificSettingBase
    {
        if (player == null || player.ReferenceHub == null)
        {
            Log.Warn("[SSSUtility] GetPlayerSettingValue called with null player");
            return null;
        }

        try
        {
            return ServerSpecificSettingsSync.GetSettingOfUser<T>(player.ReferenceHub, settingId);
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error getting player setting value: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    public static bool TryGetPlayerSettingValue<T>(Player player, int settingId, out T value) where T : ServerSpecificSettingBase
    {
        value = null;
        
        if (player == null || player.ReferenceHub == null)
        {
            return false;
        }

        try
        {
            return ServerSpecificSettingsSync.TryGetSettingOfUser<T>(player.ReferenceHub, settingId, out value);
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error trying to get player setting value: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    public static bool IsPlayerTabOpen(Player player)
    {
        if (player == null || player.ReferenceHub == null)
        {
            return false;
        }

        try
        {
            return ServerSpecificSettingsSync.IsTabOpenForUser(player.ReferenceHub);
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error checking if player tab is open: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    public static int GetPlayerVersion(Player player)
    {
        if (player == null || player.ReferenceHub == null)
        {
            return 0;
        }

        try
        {
            return ServerSpecificSettingsSync.GetUserVersion(player.ReferenceHub);
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error getting player version: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
            return 0;
        }
    }

    public static bool IsPlayerMenuOutdated(Player player)
    {
        if (player == null || player.ReferenceHub == null)
        {
            return false;
        }

        try
        {
            int playerVersion = GetPlayerVersion(player);
            return playerVersion < ServerSpecificSettingsSync.Version;
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error checking if menu is outdated: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    public static void ResetPlayerSettings(Player player, Menu menu)
    {
        if (player == null || player.ReferenceHub == null)
        {
            Log.Warn("[SSSUtility] ResetPlayerSettings called with null player");
            return;
        }

        if (menu == null)
        {
            Log.Warn("[SSSUtility] ResetPlayerSettings called with null menu");
            return;
        }

        try
        {
            int resetCount = 0;
            
            foreach (var setting in menu.GetAllSettings())
            {
                if (TryGetPlayerSettingValue<ServerSpecificSettingBase>(player, setting.SettingId, out var playerSetting))
                {
                    setting.ApplyDefaultValues();
                    resetCount++;
                }
            }
            
            Log.Debug($"[SSSUtility] Reset {resetCount} settings for player {player.Nickname} in menu '{menu.Name}'");
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error resetting player settings: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
        }
    }

    public static int GetPlayerCurrentPage(Player player, string menuPluginName)
    {
        if (player == null || player.ReferenceHub == null)
        {
            return -1;
        }

        try
        {
            var state = Core.PageManager.GetState(player.ReferenceHub);
            if (state != null && state.CurrentMenuPlugin == menuPluginName)
            {
                return state.CurrentPageIndex;
            }
            return -1;
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error getting player current page: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
            return -1;
        }
    }

    public static bool IsPlayerViewingMenu(Player player, string menuPluginName)
    {
        return GetPlayerCurrentPage(player, menuPluginName) >= 0;
    }
}
