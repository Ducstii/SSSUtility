namespace SSSUtility.API;

using SSSUtility.Models;

/// <summary>
/// Provides methods to query player setting values and menu state.
/// </summary>
public static class SettingQueries
{
    /// <summary>
    /// Gets a player's current value for a specific setting.
    /// </summary>
    /// <typeparam name="T">The setting type (e.g., SSDropdownSetting, SSSliderSetting)</typeparam>
    /// <param name="player">The player to query</param>
    /// <param name="settingId">The setting ID</param>
    /// <returns>The player's setting value, or null if not found</returns>
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
            Log.Error($"[SSSUtility] Error getting player setting value: {ex}");
            return null;
        }
    }

    /// <summary>
    /// Tries to get a player's current value for a specific setting.
    /// </summary>
    /// <typeparam name="T">The setting type</typeparam>
    /// <param name="player">The player to query</param>
    /// <param name="settingId">The setting ID</param>
    /// <param name="value">The setting value if found</param>
    /// <returns>True if the setting was found, false otherwise</returns>
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
            Log.Error($"[SSSUtility] Error trying to get player setting value: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Checks if a player has the menu tab open.
    /// </summary>
    /// <param name="player">The player to check</param>
    /// <returns>True if the tab is open, false otherwise</returns>
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
            Log.Error($"[SSSUtility] Error checking if player tab is open: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Gets the menu version the player has.
    /// </summary>
    /// <param name="player">The player to check</param>
    /// <returns>The player's menu version, or 0 if unavailable</returns>
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
            Log.Error($"[SSSUtility] Error getting player version: {ex}");
            return 0;
        }
    }

    /// <summary>
    /// Checks if a player's menu is outdated compared to the server.
    /// </summary>
    /// <param name="player">The player to check</param>
    /// <returns>True if the player's menu version is outdated</returns>
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
            Log.Error($"[SSSUtility] Error checking if menu is outdated: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Resets all settings in a menu to their default values for a specific player.
    /// </summary>
    /// <param name="player">The player to reset</param>
    /// <param name="menu">The menu containing the settings</param>
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
            Log.Error($"[SSSUtility] Error resetting player settings: {ex}");
        }
    }

    /// <summary>
    /// Gets the current page index for a player viewing a menu.
    /// </summary>
    /// <param name="player">The player to check</param>
    /// <param name="menuPluginName">The plugin name of the menu</param>
    /// <returns>The current page index, or -1 if not viewing the menu</returns>
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
            Log.Error($"[SSSUtility] Error getting player current page: {ex}");
            return -1;
        }
    }

    /// <summary>
    /// Checks if a player is currently viewing a specific menu.
    /// </summary>
    /// <param name="player">The player to check</param>
    /// <param name="menuPluginName">The plugin name of the menu</param>
    /// <returns>True if the player is viewing the menu</returns>
    public static bool IsPlayerViewingMenu(Player player, string menuPluginName)
    {
        return GetPlayerCurrentPage(player, menuPluginName) >= 0;
    }
}
