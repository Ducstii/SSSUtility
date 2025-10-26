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
                // This setting doesn't belong to any SSSUtility menu
                return;
            }

            // Figure out what type of setting this is and handle it accordingly
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
        // Make sure this isn't accidentally the page selector (should be dropdown)
        if (menu.PageSelectorDropdown != null && button.SettingId == menu.PageSelectorDropdown.SettingId)
        {
            return;
        }

        var player = Player.Get(hub);
        if (player == null) return;

        // Run the specific action for this button
        if (menu.ButtonCallbacks.TryGetValue(button.SettingId, out var callback))
        {
            callback?.Invoke(player);
        }

        // Tell any other systems that this button was pressed
        menu.OnButtonPressed?.Invoke(player, button.SettingId, button);
    }

    private static void HandleDropdown(Menu menu, ReferenceHub hub, SSDropdownSetting dropdown)
    {
        int selectedIndex = dropdown.SyncSelectionIndexValidated;

        // Handle page navigation if this is the page selector
        if (menu.PageSelectorDropdown != null && dropdown.SettingId == menu.PageSelectorDropdown.SettingId)
        {
            Core.PageManager.SwitchPage(hub, menu, selectedIndex);
            return;
        }

        var player = Player.Get(hub);
        if (player == null) return;

        // Execute the handler for this dropdown selection
        if (menu.DropdownCallbacks.TryGetValue(dropdown.SettingId, out var callback))
        {
            callback?.Invoke(player, selectedIndex);
        }

        // Let other systems know about this dropdown change
        menu.OnDropdownChanged?.Invoke(player, dropdown.SettingId, selectedIndex, dropdown);
    }

    private static void HandleSlider(Menu menu, ReferenceHub hub, SSSliderSetting slider)
    {
        float value = slider.SyncFloatValue;

        var player = Player.Get(hub);
        if (player == null) return;

        // Run the handler for this slider value
        if (menu.SliderCallbacks.TryGetValue(slider.SettingId, out var callback))
        {
            callback?.Invoke(player, value);
        }

        // Broadcast this slider change to any listeners
        menu.OnSliderChanged?.Invoke(player, slider.SettingId, value, slider);
    }

    private static void HandleKeybind(Menu menu, ReferenceHub hub, SSKeybindSetting keybind)
    {
        bool isPressed = keybind.SyncIsPressed;
        KeyCode key = keybind.AssignedKeyCode;

        var player = Player.Get(hub);
        if (player == null) return;

        // Call the specific callback for this keybind
        if (menu.KeybindCallbacks.TryGetValue(keybind.SettingId, out var callback))
        {
            callback?.Invoke(player, key);
        }

        // Notify any global listeners about this keybind change
        menu.OnKeybindChanged?.Invoke(player, keybind.SettingId, key, keybind);
    }

    private static void HandlePlaintext(Menu menu, ReferenceHub hub, SSPlaintextSetting plaintext)
    {
        string value = plaintext.SyncInputText;

        var player = Player.Get(hub);
        if (player == null) return;

        // Run the specific handler for this text input
        if (menu.PlaintextCallbacks.TryGetValue(plaintext.SettingId, out var callback))
        {
            callback?.Invoke(player, value);
        }

        // Let other systems know about this text change
        menu.OnPlaintextChanged?.Invoke(player, plaintext.SettingId, value, plaintext);
    }

    private static void HandleTwoButtons(Menu menu, ReferenceHub hub, SSTwoButtonsSetting twoButtons)
    {
        bool isB = twoButtons.SyncIsB;

        var player = Player.Get(hub);
        if (player == null) return;

        // Execute the handler for whichever button was pressed
        if (menu.TwoButtonsCallbacks.TryGetValue(twoButtons.SettingId, out var callback))
        {
            callback?.Invoke(player, isB);
        }

        // Broadcast this button press to any listeners
        menu.OnTwoButtonsPressed?.Invoke(player, twoButtons.SettingId, isB, twoButtons);
    }

    private static void HandleTextArea(Menu menu, ReferenceHub hub, SSTextArea textArea)
    {
        // TextArea is read-only, so we just pass along the display text
        string text = textArea.Label;

        var player = Player.Get(hub);
        if (player == null) return;

        // Call any specific handlers for this text area
        if (menu.TextAreaCallbacks.TryGetValue(textArea.SettingId, out var callback))
        {
            callback?.Invoke(player, text);
        }

        // Notify global listeners about this text area interaction
        menu.OnTextAreaChanged?.Invoke(player, textArea.SettingId, text, textArea);
    }
}

