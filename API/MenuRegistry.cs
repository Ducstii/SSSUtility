namespace SSSUtility.API;

using SSSUtility.Models;

public static class MenuRegistry
{
    private static readonly Dictionary<string, Menu> _menus = new();
    private static readonly Dictionary<int, Menu> _settingIdToMenu = new();
    private static readonly object _lock = new();

    public static void Register(string pluginName, Menu menu)
    {
        lock (_lock)
        {
            if (_menus.ContainsKey(pluginName))
            {
                Log.Warn($"[SSSUtility] Menu for plugin '{pluginName}' already registered. Unregistering old menu first.");
                Unregister(pluginName);
            }

            menu.PluginName = pluginName;
            _menus[pluginName] = menu;

            // Map all SettingIds to this menu
            foreach (var setting in menu.GetAllSettings())
            {
                _settingIdToMenu[setting.SettingId] = menu;
            }

            Log.Info($"[SSSUtility] Registered menu '{menu.Name}' for plugin '{pluginName}' with {menu.Pages.Count} pages");
        }
    }

    public static void Unregister(string pluginName)
    {
        lock (_lock)
        {
            if (!_menus.TryGetValue(pluginName, out var menu))
            {
                Log.Warn($"[SSSUtility] No menu registered for plugin '{pluginName}'");
                return;
            }

            // Remove SettingId mappings
            foreach (var setting in menu.GetAllSettings())
            {
                _settingIdToMenu.Remove(setting.SettingId);
            }

            _menus.Remove(pluginName);
            Log.Info($"[SSSUtility] Unregistered menu for plugin '{pluginName}'");
        }
    }

    public static Menu GetMenu(string pluginName)
    {
        lock (_lock)
        {
            return _menus.TryGetValue(pluginName, out var menu) ? menu : null;
        }
    }

    public static Menu GetMenuBySettingId(int settingId)
    {
        lock (_lock)
        {
            return _settingIdToMenu.TryGetValue(settingId, out var menu) ? menu : null;
        }
    }

    public static IEnumerable<Menu> GetAllMenus()
    {
        lock (_lock)
        {
            return _menus.Values.ToList();
        }
    }

    public static void Clear()
    {
        lock (_lock)
        {
            _menus.Clear();
            _settingIdToMenu.Clear();
        }
    }
}

