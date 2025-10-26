namespace SSSUtility.API;

using SSSUtility.Models;

public static class ConflictResolver
{
    private static int _nextAvailableId = 10000;
    private static int _version = 1;

    public static void AssignIdsAndRebuild(Menu menu)
    {
        int startId = _nextAvailableId;
        menu.RemapIds(startId);
        _nextAvailableId = menu.IdRangeEnd + 1;

        Log.Debug($"[SSSUtility] Assigned ID range {menu.IdRangeStart}-{menu.IdRangeEnd} to menu '{menu.Name}'");

        RebuildDefinedSettings();
    }

    public static void RebuildDefinedSettings()
    {
        var allSettings = new List<ServerSpecificSettingBase>();

        foreach (var menu in MenuRegistry.GetAllMenus())
        {
            allSettings.AddRange(menu.GetAllSettings());
        }

        ServerSpecificSettingsSync.DefinedSettings = allSettings.ToArray();
        _version++;
        ServerSpecificSettingsSync.Version = _version;

        Log.Info($"[SSSUtility] Rebuilt DefinedSettings with {allSettings.Count} total settings (version {_version})");
    }

    public static int GetVersion() => _version;

    public static void Reset()
    {
        _nextAvailableId = 10000;
        _version = 1;
    }
}

