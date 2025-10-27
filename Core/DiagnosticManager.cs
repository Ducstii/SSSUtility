namespace SSSUtility.Core;

using SSSUtility.API;
using SSSUtility.Models;

/// <summary>
/// Provides diagnostic information and health monitoring for SSSUtility.
/// </summary>
public static class DiagnosticManager
{
    /// <summary>
    /// Health check report.
    /// </summary>
    public class HealthReport
    {
        public bool IsHealthy { get; set; }
        public int ActiveMenus { get; set; }
        public int ActivePlayers { get; set; }
        public int TotalSettings { get; set; }
        public int MenuVersion { get; set; }
        public List<string> Issues { get; set; } = new();
    }

    /// <summary>
    /// Performs a comprehensive health check.
    /// </summary>
    public static HealthReport PerformHealthCheck()
    {
        var report = new HealthReport();

        try
        {
            // Get registry stats
            var (menuCount, totalSettings, totalPages) = MenuRegistry.GetStats();
            report.ActiveMenus = menuCount;
            report.TotalSettings = totalSettings;

            // Get menu version
            report.MenuVersion = ConflictResolver.GetVersion();

            // Check for active player states
            // Note: This is an estimate since we don't expose the internal dictionary
            int playerCount = Player.List.Count(p => p.ReferenceHub != null && PageManager.GetState(p.ReferenceHub) != null);
            report.ActivePlayers = playerCount;

            // Validate registry
            if (!MenuRegistry.ValidateRegistry())
            {
                report.Issues.Add("Menu registry validation failed");
                report.IsHealthy = false;
            }

            // Check if SSS system is functioning
            if (ServerSpecificSettingsSync.DefinedSettings == null)
            {
                report.Issues.Add("ServerSpecificSettingsSync.DefinedSettings is null");
                report.IsHealthy = false;
            }

            // Healthy if no issues found
            if (report.Issues.Count == 0)
            {
                report.IsHealthy = true;
            }

            return report;
        }
        catch (Exception ex)
        {
            report.Issues.Add($"Exception during health check: {ex.Message}");
            report.IsHealthy = false;
            return report;
        }
    }

    /// <summary>
    /// Logs diagnostic information to console.
    /// </summary>
    public static void LogDiagnostics()
    {
        try
        {
            var (menuCount, totalSettings, totalPages) = MenuRegistry.GetStats();
            var version = ConflictResolver.GetVersion();

            Log.Info("=== SSSUtility Diagnostics ===");
            Log.Info($"Active Menus: {menuCount}");
            Log.Info($"Total Settings: {totalSettings}");
            Log.Info($"Total Pages: {totalPages}");
            Log.Info($"Menu Version: {version}");

            // List all registered menus
            if (menuCount > 0)
            {
                Log.Info("Registered Menus:");
                foreach (var menu in MenuRegistry.GetAllMenus())
                {
                    if (menu != null)
                    {
                        Log.Info($"  - {menu.PluginName}: '{menu.Name}' ({menu.Pages.Count} pages, ID range: {menu.IdRangeStart}-{menu.IdRangeEnd})");
                    }
                }
            }

            // Perform health check
            var health = PerformHealthCheck();
            if (health.IsHealthy)
            {
                Log.Info($"Health Status: Healthy ({health.ActivePlayers} players with menu state)");
            }
            else
            {
                Log.Warn("Health Status: Issues Detected");
                foreach (var issue in health.Issues)
                {
                    Log.Warn($"  - {issue}");
                }
            }

            Log.Info("================================");
        }
        catch (Exception ex)
        {
            Log.Error($"Error logging diagnostics: {ex}");
        }
    }

    /// <summary>
    /// Gets detailed statistics about menu registry.
    /// </summary>
    public static void LogRegistryStats()
    {
        try
        {
            var (menuCount, totalSettings, totalPages) = MenuRegistry.GetStats();
            
            Log.Info($"[SSSUtility] Registry Statistics:");
            Log.Info($"  Menus: {menuCount}");
            Log.Info($"  Total Settings: {totalSettings}");
            Log.Info($"  Total Pages: {totalPages}");
            Log.Info($"  Menu Version: {ConflictResolver.GetVersion()}");

            if (menuCount > 0)
            {
                Log.Info("Detailed Menu Information:");
                foreach (var menu in MenuRegistry.GetAllMenus())
                {
                    if (menu != null)
                    {
                        Log.Info($"  {menu.PluginName}:");
                        Log.Info($"    Name: {menu.Name}");
                        Log.Info($"    Pages: {menu.Pages.Count}");
                        Log.Info($"    ID Range: {menu.IdRangeStart}-{menu.IdRangeEnd}");
                        
                        var settingsCount = menu.GetAllSettings().Count();
                        Log.Info($"    Settings: {settingsCount}");
                        
                        var callbackCount = menu.ButtonCallbacks.Count + 
                                           menu.DropdownCallbacks.Count + 
                                           menu.SliderCallbacks.Count + 
                                           menu.KeybindCallbacks.Count + 
                                           menu.PlaintextCallbacks.Count + 
                                           menu.TwoButtonsCallbacks.Count + 
                                           menu.TextAreaCallbacks.Count;
                        Log.Info($"    Callbacks: {callbackCount}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error logging registry stats: {ex}");
        }
    }

    /// <summary>
    /// Validates all registered menus.
    /// </summary>
    public static bool ValidateAllMenus()
    {
        try
        {
            bool allValid = true;
            Log.Info("[SSSUtility] Validating all registered menus...");

            foreach (var menu in MenuRegistry.GetAllMenus())
            {
                if (menu != null)
                {
                    var result = MenuValidator.ValidateMenu(menu, menu.PluginName);
                    if (!result.IsValid)
                    {
                        allValid = false;
                    }
                }
            }

            if (allValid)
            {
                Log.Info("[SSSUtility] All menus validated successfully");
            }
            else
            {
                Log.Error("[SSSUtility] Some menus failed validation");
            }

            return allValid;
        }
        catch (Exception ex)
        {
            Log.Error($"Error validating menus: {ex}");
            return false;
        }
    }
}
