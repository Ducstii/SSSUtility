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
            if (hub == null)
            {
                Log.Warn("[SSSUtility] Received setting value from null hub");
                return;
            }
            
            if (setting == null)
            {
                Log.Warn($"[SSSUtility] Received null setting from {hub.nicknameSync?.MyNick ?? "unknown"}");
                return;
            }

            var menu = MenuRegistry.GetMenuBySettingId(setting.SettingId);
            if (menu == null)
            {
                // This setting doesn't belong to any SSSUtility menu - this is normal
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
                default:
                    Log.Debug($"[SSSUtility] Unknown setting type: {setting.GetType().Name} (ID: {setting.SettingId})");
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error handling setting value from {hub?.nicknameSync?.MyNick ?? "unknown"}: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
            Log.Error($"[SSSUtility] Setting ID: {setting?.SettingId ?? -1}, Type: {setting?.GetType().Name ?? "null"}");
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
            Log.Error($"[SSSUtility] Error handling status: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
        }
    }

    private static void HandleButton(Menu menu, ReferenceHub hub, SSButton button)
    {
        try
        {
            if (menu == null || hub == null || button == null)
            {
                Log.Warn("[SSSUtility] HandleButton called with null parameter");
                return;
            }

            // Make sure this isn't accidentally the page selector (should be dropdown)
            if (menu.PageSelectorDropdown != null && button.SettingId == menu.PageSelectorDropdown.SettingId)
            {
                return;
            }

            var player = Player.Get(hub);
            if (player == null)
            {
                Log.Warn($"[SSSUtility] Could not get player for hub {hub.nicknameSync?.MyNick ?? "unknown"}");
                return;
            }

            // Run the specific action for this button
            if (menu.ButtonCallbacks.TryGetValue(button.SettingId, out var callback))
            {
                callback?.Invoke(player);
            }

            // Tell any other systems that this button was pressed
            menu.OnButtonPressed?.Invoke(player, button.SettingId, button);
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error in HandleButton: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
        }
    }

    private static void HandleDropdown(Menu menu, ReferenceHub hub, SSDropdownSetting dropdown)
    {
        try
        {
            if (menu == null || hub == null || dropdown == null)
            {
                Log.Warn("[SSSUtility] HandleDropdown called with null parameter");
                return;
            }

            int selectedIndex = dropdown.SyncSelectionIndexValidated;

            // Handle page navigation if this is the page selector
            if (menu.PageSelectorDropdown != null && dropdown.SettingId == menu.PageSelectorDropdown.SettingId)
            {
                Core.PageManager.SwitchPage(hub, menu, selectedIndex);
                return;
            }

            var player = Player.Get(hub);
            if (player == null)
            {
                Log.Warn($"[SSSUtility] Could not get player for hub {hub.nicknameSync?.MyNick ?? "unknown"}");
                return;
            }

            // Execute the handler for this dropdown selection
            if (menu.DropdownCallbacks.TryGetValue(dropdown.SettingId, out var callback))
            {
                callback?.Invoke(player, selectedIndex);
            }

            // Let other systems know about this dropdown change
            menu.OnDropdownChanged?.Invoke(player, dropdown.SettingId, selectedIndex, dropdown);
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error in HandleDropdown: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
        }
    }

    private static void HandleSlider(Menu menu, ReferenceHub hub, SSSliderSetting slider)
    {
        try
        {
            if (menu == null || hub == null || slider == null)
            {
                Log.Warn("[SSSUtility] HandleSlider called with null parameter");
                return;
            }

            float value = slider.SyncFloatValue;

            var player = Player.Get(hub);
            if (player == null)
            {
                Log.Warn($"[SSSUtility] Could not get player for hub {hub.nicknameSync?.MyNick ?? "unknown"}");
                return;
            }

            // Run the handler for this slider value
            if (menu.SliderCallbacks.TryGetValue(slider.SettingId, out var callback))
            {
                callback?.Invoke(player, value);
            }

            // Broadcast this slider change to any listeners
            menu.OnSliderChanged?.Invoke(player, slider.SettingId, value, slider);
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error in HandleSlider: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
        }
    }

    private static void HandleKeybind(Menu menu, ReferenceHub hub, SSKeybindSetting keybind)
    {
        try
        {
            if (menu == null || hub == null || keybind == null)
            {
                Log.Warn("[SSSUtility] HandleKeybind called with null parameter");
                return;
            }

            bool isPressed = keybind.SyncIsPressed;
            KeyCode key = keybind.AssignedKeyCode;

            var player = Player.Get(hub);
            if (player == null)
            {
                Log.Warn($"[SSSUtility] Could not get player for hub {hub.nicknameSync?.MyNick ?? "unknown"}");
                return;
            }

            // Call the specific callback for this keybind
            if (menu.KeybindCallbacks.TryGetValue(keybind.SettingId, out var callback))
            {
                callback?.Invoke(player, key);
            }

            // Notify any global listeners about this keybind change
            menu.OnKeybindChanged?.Invoke(player, keybind.SettingId, key, keybind);
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error in HandleKeybind: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
        }
    }

    private static void HandlePlaintext(Menu menu, ReferenceHub hub, SSPlaintextSetting plaintext)
    {
        try
        {
            if (menu == null || hub == null || plaintext == null)
            {
                Log.Warn("[SSSUtility] HandlePlaintext called with null parameter");
                return;
            }

            string value = plaintext.SyncInputText;

            var player = Player.Get(hub);
            if (player == null)
            {
                Log.Warn($"[SSSUtility] Could not get player for hub {hub.nicknameSync?.MyNick ?? "unknown"}");
                return;
            }

            // Run the specific handler for this text input
            if (menu.PlaintextCallbacks.TryGetValue(plaintext.SettingId, out var callback))
            {
                callback?.Invoke(player, value);
            }

            // Let other systems know about this text change
            menu.OnPlaintextChanged?.Invoke(player, plaintext.SettingId, value, plaintext);
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error in HandlePlaintext: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
        }
    }

    private static void HandleTwoButtons(Menu menu, ReferenceHub hub, SSTwoButtonsSetting twoButtons)
    {
        try
        {
            if (menu == null || hub == null || twoButtons == null)
            {
                Log.Warn("[SSSUtility] HandleTwoButtons called with null parameter");
                return;
            }

            bool isB = twoButtons.SyncIsB;

            var player = Player.Get(hub);
            if (player == null)
            {
                Log.Warn($"[SSSUtility] Could not get player for hub {hub.nicknameSync?.MyNick ?? "unknown"}");
                return;
            }

            // Execute the handler for whichever button was pressed
            if (menu.TwoButtonsCallbacks.TryGetValue(twoButtons.SettingId, out var callback))
            {
                callback?.Invoke(player, isB);
            }

            // Broadcast this button press to any listeners
            menu.OnTwoButtonsPressed?.Invoke(player, twoButtons.SettingId, isB, twoButtons);
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error in HandleTwoButtons: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
        }
    }

    private static void HandleTextArea(Menu menu, ReferenceHub hub, SSTextArea textArea)
    {
        try
        {
            if (menu == null || hub == null || textArea == null)
            {
                Log.Warn("[SSSUtility] HandleTextArea called with null parameter");
                return;
            }

            // TextArea is read-only, so we just pass along the display text
            string text = textArea.Label;

            var player = Player.Get(hub);
            if (player == null)
            {
                Log.Warn($"[SSSUtility] Could not get player for hub {hub.nicknameSync?.MyNick ?? "unknown"}");
                return;
            }

            // Call any specific handlers for this text area
            if (menu.TextAreaCallbacks.TryGetValue(textArea.SettingId, out var callback))
            {
                callback?.Invoke(player, text);
            }

            // Notify global listeners about this text area interaction
            menu.OnTextAreaChanged?.Invoke(player, textArea.SettingId, text, textArea);
        }
        catch (Exception ex)
        {
            Log.Error($"[SSSUtility] Error in HandleTextArea: {ex.Message}");
            Log.Error($"[SSSUtility] Stack trace: {ex.StackTrace}");
        }
    }
}

