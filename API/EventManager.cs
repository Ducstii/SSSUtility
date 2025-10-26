namespace SSSUtility.API;

using SSSUtility.Models;

public static class EventManager
{
    public static void Initialize()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSettingValueReceived;
        ServerSpecificSettingsSync.ServerOnStatusReceived += OnStatusReceived;
        Log.Debug("[SSSUtility] Event manager initialized");
    }

    public static void Cleanup()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSettingValueReceived;
        ServerSpecificSettingsSync.ServerOnStatusReceived -= OnStatusReceived;
        Log.Debug("[SSSUtility] Event manager cleaned up");
    }

    private static void OnSettingValueReceived(ReferenceHub hub, ServerSpecificSettingBase setting)
    {
        try
        {
            var menu = MenuRegistry.GetMenuBySettingId(setting.SettingId);
            if (menu == null)
            {
                // Not our setting, ignore
                return;
            }

            // Route to appropriate handler based on setting type
            switch (setting)
            {
                case SSButton button:
                    HandleButton(menu, hub, button);
                    break;
                case SSDropdownSetting dropdown:
                    HandleDropdown(menu, hub, dropdown);
                    break;
                case SSSliderSetting slider:
                    HandleSlider(menu, hub, slider);
                    break;
                case SSKeybindSetting keybind:
                    HandleKeybind(menu, hub, keybind);
                    break;
                case SSPlaintextSetting plaintext:
                    HandlePlaintext(menu, hub, plaintext);
                    break;
                case SSTwoButtonsSetting twoButtons:
                    HandleTwoButtons(menu, hub, twoButtons);
                    break;
                case SSTextArea textArea:
                    HandleTextArea(menu, hub, textArea);
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error handling setting value: {ex}");
        }
    }

    private static void OnStatusReceived(ReferenceHub hub, SSSUserStatusReport status)
    {
        try
        {
            var state = Core.PageManager.GetOrCreateState(hub);
            state.IsTabOpen = status.TabOpen;

            Log.Debug($"[SSSUtility] Player {hub.nicknameSync.MyNick} tab status: {(status.TabOpen ? "open" : "closed")}");
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error handling status: {ex}");
        }
    }

    private static void HandleButton(Menu menu, ReferenceHub hub, SSButton button)
    {
        // Check if it's the page selector
        if (menu.PageSelectorDropdown != null && button.SettingId == menu.PageSelectorDropdown.SettingId)
        {
            // This shouldn't happen (page selector is dropdown), but handle just in case
            return;
        }

        var player = Player.Get(hub);
        if (player == null) return;

        // Invoke individual callback if exists
        if (menu.ButtonCallbacks.TryGetValue(button.SettingId, out var callback))
        {
            callback?.Invoke(player);
        }

        // Invoke global callback
        menu.OnButtonPressed?.Invoke(player, button.SettingId, button);
    }

    private static void HandleDropdown(Menu menu, ReferenceHub hub, SSDropdownSetting dropdown)
    {
        int selectedIndex = dropdown.SyncSelectionIndexValidated;

        // Check if it's the page selector
        if (menu.PageSelectorDropdown != null && dropdown.SettingId == menu.PageSelectorDropdown.SettingId)
        {
            Core.PageManager.SwitchPage(hub, menu, selectedIndex);
            return;
        }

        var player = Player.Get(hub);
        if (player == null) return;

        // Invoke individual callback if exists
        if (menu.DropdownCallbacks.TryGetValue(dropdown.SettingId, out var callback))
        {
            callback?.Invoke(player, selectedIndex);
        }

        // Invoke global callback
        menu.OnDropdownChanged?.Invoke(player, dropdown.SettingId, selectedIndex, dropdown);
    }

    private static void HandleSlider(Menu menu, ReferenceHub hub, SSSliderSetting slider)
    {
        float value = slider.SyncFloatValue;

        var player = Player.Get(hub);
        if (player == null) return;

        // Invoke individual callback if exists
        if (menu.SliderCallbacks.TryGetValue(slider.SettingId, out var callback))
        {
            callback?.Invoke(player, value);
        }

        // Invoke global callback
        menu.OnSliderChanged?.Invoke(player, slider.SettingId, value, slider);
    }

    private static void HandleKeybind(Menu menu, ReferenceHub hub, SSKeybindSetting keybind)
    {
        // Keybind pressed state
        bool isPressed = keybind.SyncIsPressed;
        KeyCode key = keybind.AssignedKeyCode;

        var player = Player.Get(hub);
        if (player == null) return;

        // Check if this is a Nexus keybind menu and route to KeybindManager
        if (menu.Name == "Nexus-Keybinds")
        {
            try
            {
                // Import KeybindManager dynamically to avoid circular dependency
                var keybindManagerType = Type.GetType("Nexus.Features.Menus.KeybindManager");
                if (keybindManagerType != null)
                {
                    var onKeybindPressedMethod = keybindManagerType.GetMethod("OnKeybindPressed", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    
                    if (onKeybindPressedMethod != null && isPressed)
                    {
                        onKeybindPressedMethod.Invoke(null, new object[] { player, key });
                        return; // Handled by KeybindManager, don't process further
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[SSSUtility] Error routing keybind to KeybindManager: {ex}");
            }
        }

        // Invoke individual callback if exists
        if (menu.KeybindCallbacks.TryGetValue(keybind.SettingId, out var callback))
        {
            callback?.Invoke(player, key);
        }

        // Invoke global callback
        menu.OnKeybindChanged?.Invoke(player, keybind.SettingId, key, keybind);
    }

    private static void HandlePlaintext(Menu menu, ReferenceHub hub, SSPlaintextSetting plaintext)
    {
        string value = plaintext.SyncInputText;

        var player = Player.Get(hub);
        if (player == null) return;

        // Invoke individual callback if exists
        if (menu.PlaintextCallbacks.TryGetValue(plaintext.SettingId, out var callback))
        {
            callback?.Invoke(player, value);
        }

        // Invoke global callback
        menu.OnPlaintextChanged?.Invoke(player, plaintext.SettingId, value, plaintext);
    }

    private static void HandleTwoButtons(Menu menu, ReferenceHub hub, SSTwoButtonsSetting twoButtons)
    {
        bool isB = twoButtons.SyncIsB;

        var player = Player.Get(hub);
        if (player == null) return;

        // Invoke individual callback if exists
        if (menu.TwoButtonsCallbacks.TryGetValue(twoButtons.SettingId, out var callback))
        {
            callback?.Invoke(player, isB);
        }

        // Invoke global callback
        menu.OnTwoButtonsPressed?.Invoke(player, twoButtons.SettingId, isB, twoButtons);
    }

    private static void HandleTextArea(Menu menu, ReferenceHub hub, SSTextArea textArea)
    {
        // TextArea is read-only (ResponseMode = None), so we don't handle value changes
        // Just invoke callbacks with the label
        string text = textArea.Label;

        var player = Player.Get(hub);
        if (player == null) return;

        // Invoke individual callback if exists
        if (menu.TextAreaCallbacks.TryGetValue(textArea.SettingId, out var callback))
        {
            callback?.Invoke(player, text);
        }

        // Invoke global callback
        menu.OnTextAreaChanged?.Invoke(player, textArea.SettingId, text, textArea);
    }
}

