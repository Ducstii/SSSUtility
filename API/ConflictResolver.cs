namespace SSSUtility.API;

using SSSUtility.Models;

public static class ConflictResolver
{
    private static int _nextAvailableId = 10000;
    private static int _version = 1;

    public static void AssignIdsAndRebuild(Menu menu)
    {
        try
        {
            if (menu == null)
            {
                Log.Error("[SSSUtility] AssignIdsAndRebuild called with null menu");
                return;
            }

            int startId = _nextAvailableId;
            
            // Validate we won't overflow int.MaxValue
            if (startId < 0 || startId > int.MaxValue - 10000)
            {
                Log.Error($"[SSSUtility] ID range exhausted! _nextAvailableId={startId}");
                _nextAvailableId = 10000; // Reset to start
                startId = _nextAvailableId;
            }

            menu.RemapIds(startId);
            _nextAvailableId = menu.IdRangeEnd + 1;

            Log.Debug($"[SSSUtility] Assigned ID range {menu.IdRangeStart}-{menu.IdRangeEnd} to menu '{menu.Name}'");

            RebuildDefinedSettings();
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error in AssignIdsAndRebuild: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
        }
    }

    public static void RebuildDefinedSettings()
    {
        try
        {
            var allSettings = new List<ServerSpecificSettingBase>();

            foreach (var menu in MenuRegistry.GetAllMenus())
            {
                try
                {
                    var settings = menu.GetAllSettings();
                    allSettings.AddRange(settings);
                }
                catch (Exception ex)
                {
                    Log.Error($"[SSSUtility] Error getting settings from menu '{menu.Name}': {ex.Message}");
                    Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
                }
            }

            // Validate array before assigning
            if (allSettings == null || allSettings.Count == 0)
            {
                Log.Warn("[SSSUtility] RebuildDefinedSettings: No settings to register");
                ServerSpecificSettingsSync.DefinedSettings = Array.Empty<ServerSpecificSettingBase>();
            }
            else
            {
                ServerSpecificSettingsSync.DefinedSettings = allSettings.ToArray();
            }

            _version++;
            ServerSpecificSettingsSync.Version = _version;

            Log.Debug($"[SSSUtility] Rebuilt DefinedSettings with {allSettings.Count} total settings (version {_version})");
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Critical error in RebuildDefinedSettings: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
        }
    }

    public static int GetVersion() => _version;

    public static void Reset()
    {
        _nextAvailableId = 10000;
        _version = 1;
        Log.Debug("[SSSUtility] ConflictResolver reset");
    }

    public static bool ValidateIdRange(Menu newMenu)
    {
        try
        {
            if (newMenu == null)
            {
                return false;
            }

            var allMenus = MenuRegistry.GetAllMenus();
            foreach (var existingMenu in allMenus)
            {
                if (existingMenu == newMenu || existingMenu.PluginName == newMenu.PluginName)
                    continue;

                // Check for overlap
                if (newMenu.IdRangeStart <= existingMenu.IdRangeEnd && 
                    newMenu.IdRangeEnd >= existingMenu.IdRangeStart)
                {
                    Log.Error($"[SSSUtility] ID range conflict: '{newMenu.Name}' ({newMenu.IdRangeStart}-{newMenu.IdRangeEnd}) " +
                             $"overlaps '{existingMenu.Name}' ({existingMenu.IdRangeStart}-{existingMenu.IdRangeEnd})");
                    return false;
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error validating ID range: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
            return false;
        }
    }
}

