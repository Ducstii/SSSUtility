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
            if (string.IsNullOrEmpty(pluginName))
            {
                Log.Error("[SSSUtility] Cannot register menu with null or empty plugin name");
                return;
            }

            if (menu == null)
            {
                Log.Error($"[SSSUtility] Cannot register null menu for plugin '{pluginName}'");
                return;
            }

            if (menu.Pages == null || menu.Pages.Count == 0)
            {
                Log.Warn($"[SSSUtility] Menu '{menu.Name}' has no pages");
            }

            if (_menus.ContainsKey(pluginName))
            {
                Log.Warn($"[SSSUtility] Menu for plugin '{pluginName}' already registered. Unregistering old menu first.");
                Unregister(pluginName);
            }

            menu.PluginName = pluginName;
            _menus[pluginName] = menu;

            // Map all SettingIds to this menu
            int settingCount = 0;
            foreach (var setting in menu.GetAllSettings())
            {
                if (setting != null)
                {
                    _settingIdToMenu[setting.SettingId] = menu;
                    settingCount++;
                }
            }

            Log.Info($"[SSSUtility] Registered menu '{menu.Name}' for plugin '{pluginName}' with {menu.Pages.Count} pages, {settingCount} settings");
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
            Log.Debug("[SSSUtility] MenuRegistry cleared");
        }
    }

    /// <summary>
    /// Gets statistics about registered menus.
    /// </summary>
    public static (int MenuCount, int TotalSettings, int TotalPages) GetStats()
    {
        lock (_lock)
        {
            int totalSettings = 0;
            int totalPages = 0;

            foreach (var menu in _menus.Values)
            {
                if (menu?.Pages != null)
                {
                    totalPages += menu.Pages.Count;
                    
                    try
                    {
                        totalSettings += menu.GetAllSettings().Count();
                    }
                    catch
                    {
                        // Skip menu if we can't get settings
                    }
                }
            }

            return (_menus.Count, totalSettings, totalPages);
        }
    }

    /// <summary>
    /// Validates that all registered menus are valid and have no ID conflicts.
    /// </summary>
    public static bool ValidateRegistry()
    {
        lock (_lock)
        {
            try
            {
                var seenIds = new HashSet<int>();
                bool isValid = true;

                foreach (var kvp in _menus)
                {
                    var menu = kvp.Value;
                    if (menu == null)
                    {
                        Log.Warn($"[SSSUtility] Found null menu in registry for '{kvp.Key}'");
                        isValid = false;
                        continue;
                    }

                    try
                    {
                        foreach (var setting in menu.GetAllSettings())
                        {
                            if (setting != null)
                            {
                                if (seenIds.Contains(setting.SettingId))
                                {
                                    Log.Error($"[SSSUtility] Duplicate setting ID {setting.SettingId} found in menu '{menu.Name}'");
                                    isValid = false;
                                }
                                else
                                {
                                    seenIds.Add(setting.SettingId);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[SSSUtility] Error validating menu '{menu.Name}': {ex}");
                        isValid = false;
                    }
                }

                return isValid;
            }
            catch (Exception ex)
            {
                Log.Error($"[SSSUtility] Error in ValidateRegistry: {ex}");
                return false;
            }
        }
    }
}

