namespace SSSUtility.API;

using SSSUtility.Models;

/// <summary>
/// Validates menu structure and configuration before registration.
/// </summary>
public static class MenuValidator
{
    /// <summary>
    /// Validates a menu structure and returns detailed results.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public int TotalSettings { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Validates a menu before registration.
    /// </summary>
    public static ValidationResult ValidateMenu(Menu menu, string pluginName)
    {
        var result = new ValidationResult();

        try
        {
            if (menu == null)
            {
                result.Errors.Add("Menu is null");
                result.IsValid = false;
                return result;
            }

            if (string.IsNullOrEmpty(pluginName))
            {
                result.Errors.Add("Plugin name is null or empty");
                result.IsValid = false;
                return result;
            }

            if (string.IsNullOrEmpty(menu.Name))
            {
                result.Warnings.Add("Menu name is null or empty");
            }

            // Validate pages
            if (menu.Pages == null)
            {
                result.Errors.Add("Menu.Pages is null");
                result.IsValid = false;
                return result;
            }

            if (menu.Pages.Count == 0)
            {
                result.Warnings.Add("Menu has no pages");
            }

            result.TotalPages = menu.Pages.Count;

            // Validate each page
            for (int i = 0; i < menu.Pages.Count; i++)
            {
                var page = menu.Pages[i];
                if (page == null)
                {
                    result.Errors.Add($"Page at index {i} is null");
                    result.IsValid = false;
                    continue;
                }

                if (page.OwnEntries == null)
                {
                    result.Errors.Add($"Page '{page.Name}' at index {i} has null OwnEntries");
                    result.IsValid = false;
                    continue;
                }

                // Validate page entries
                foreach (var entry in page.OwnEntries)
                {
                    if (entry == null)
                    {
                        result.Errors.Add($"Page '{page.Name}' contains null entry");
                        result.IsValid = false;
                        continue;
                    }

                    if (entry.SettingId == 0)
                    {
                        result.Warnings.Add($"Page '{page.Name}' has entry with SettingId=0 (label: {entry.Label ?? "null"})");
                    }

                    result.TotalSettings++;
                }
            }

            // Check for duplicate setting IDs within the menu
            var seenIds = new HashSet<int>();
            foreach (var setting in menu.GetAllSettings())
            {
                if (setting != null)
                {
                    if (seenIds.Contains(setting.SettingId))
                    {
                        result.Errors.Add($"Duplicate setting ID {setting.SettingId} found (label: {setting.Label ?? "null"})");
                        result.IsValid = false;
                    }
                    else
                    {
                        seenIds.Add(setting.SettingId);
                    }
                }
            }

            // Validate page selector
            if (menu.Pages.Count > 1)
            {
                if (menu.PageSelectorDropdown == null)
                {
                    result.Warnings.Add("Menu has multiple pages but no PageSelectorDropdown");
                }
                else
                {
                    if (menu.PageSelectorDropdown.SettingId == 0)
                    {
                        result.Warnings.Add("PageSelectorDropdown has SettingId=0");
                    }
                }
            }

            // Validate pinned section
            if (menu.PinnedSection == null)
            {
                result.Errors.Add("PinnedSection is null");
                result.IsValid = false;
            }

            // Validate callbacks (check if any are registered)
            bool hasAnyCallbacks = menu.ButtonCallbacks.Count > 0 ||
                                   menu.DropdownCallbacks.Count > 0 ||
                                   menu.SliderCallbacks.Count > 0 ||
                                   menu.KeybindCallbacks.Count > 0 ||
                                   menu.PlaintextCallbacks.Count > 0 ||
                                   menu.TwoButtonsCallbacks.Count > 0 ||
                                   menu.TextAreaCallbacks.Count > 0;

            if (!hasAnyCallbacks && result.TotalSettings > 0)
            {
                result.Warnings.Add("Menu has settings but no callbacks registered");
            }

            // Check if menu has valid ID range
            if (menu.IdRangeStart > menu.IdRangeEnd)
            {
                result.Errors.Add($"Invalid ID range: start ({menu.IdRangeStart}) > end ({menu.IdRangeEnd})");
                result.IsValid = false;
            }

            // Result is valid if no errors
            result.IsValid = result.Errors.Count == 0;

            return result;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Exception during validation: {ex.Message}");
            result.IsValid = false;
            return result;
        }
    }

    /// <summary>
    /// Validates that the menu's ID range doesn't conflict with existing menus.
    /// </summary>
    public static bool ValidateIdRange(Menu menu)
    {
        return ConflictResolver.ValidateIdRange(menu);
    }

    /// <summary>
    /// Validates and logs results to console.
    /// </summary>
    public static bool ValidateAndLog(Menu menu, string pluginName)
    {
        var result = ValidateMenu(menu, pluginName);

        if (result.IsValid)
        {
            Log.Info($"[SSSUtility] Menu '{menu.Name}' validation passed: {result.TotalPages} pages, {result.TotalSettings} settings");
            if (result.Warnings.Count > 0)
            {
                foreach (var warning in result.Warnings)
                {
                    Log.Warn($"[SSSUtility] Validation warning for '{menu.Name}': {warning}");
                }
            }
        }
        else
        {
            Log.Error($"[SSSUtility] Menu '{menu.Name}' validation failed:");
            foreach (var error in result.Errors)
            {
                Log.Error($"[SSSUtility]   - {error}");
            }
            if (result.Warnings.Count > 0)
            {
                foreach (var warning in result.Warnings)
                {
                    Log.Warn($"[SSSUtility]   - {warning}");
                }
            }
        }

        return result.IsValid;
    }
}
