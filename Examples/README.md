# SSSUtility - ServerSpecific Settings Utility Library

A comprehensive utility library that simplifies the creation and management of ServerSpecific Settings menus in SCP:SL plugins using Exiled.

## Features

- **Fluent API**: Easy-to-use builder pattern for creating menus
- **All 8 Setting Types**: Support for buttons, dropdowns, sliders, keybinds, text inputs, and more
- **Multi-Page Navigation**: Automatic page selector and navigation
- **Conflict Resolution**: Prevents SettingId conflicts between multiple plugins
- **Event Handling**: Simple callback system for user interactions
- **Persistence**: LiteDB-based storage for player preferences
- **Dynamic Updates**: Change labels, hints, and options at runtime
- **Player State Tracking**: Know which menu/page players are viewing

## Installation

1. Place `SSSUtility.dll` in your `EXILED/Plugins` folder
2. SSSUtility will load automatically with high priority
3. Other plugins can then depend on it

## Quick Start

### Basic Menu with Buttons

```csharp
using SSSUtility;
using SSSUtility.API;

public class MyPlugin : Plugin<Config>
{
    private Menu _menu;
    
    public override void OnEnabled()
    {
        _menu = MenuBuilder.Create("Weapon Loadout")
            .AddPage("Weapons")
                .AddHeader("Primary Weapons")
                .AddButton("M4A1", player => GiveWeapon(player, "M4A1"))
                .AddButton("AK-47", player => GiveWeapon(player, "AK47"))
                .AddHeader("Secondary Weapons")
                .AddButton("Glock", player => GiveWeapon(player, "Glock"))
            .EndPage()
            .Build();
            
        SSSUtility.RegisterMenu(Name, _menu);
        base.OnEnabled();
    }
    
    public override void OnDisabled()
    {
        SSSUtility.UnregisterMenu(Name);
        base.OnDisabled();
    }
    
    private void GiveWeapon(Player player, string weapon)
    {
        player.ShowHint($"You selected: {weapon}");
        // Your weapon logic here
    }
}
```

### Multi-Page Menu

```csharp
_menu = MenuBuilder.Create("Player Settings")
    .AddPage("Gameplay")
        .AddHeader("Difficulty")
        .AddDropdown("Mode", new[] { "Easy", "Normal", "Hard" }, 
            (player, index) => SetDifficulty(player, index))
        .AddSlider("Volume", 0f, 100f, 50f, 
            (player, value) => SetVolume(player, value))
    .EndPage()
    .AddPage("Controls")
        .AddHeader("Keybinds")
        .AddKeybind("Jump", KeyCode.Space, 
            (player, key) => SetJumpKey(player, key))
        .AddKeybind("Sprint", KeyCode.LeftShift)
    .EndPage()
    .AddPage("Profile")
        .AddPlaintext("Nickname", 32, 
            (player, name) => SaveNickname(player, name))
        .AddTextArea("Bio", SSTextArea.FoldoutMode.NotCollapsable,
            (player, bio) => SaveBio(player, bio))
    .EndPage()
    .Build();
```

### Using All Setting Types

```csharp
_menu = MenuBuilder.Create("Advanced Settings")
    .AddPage("Main")
        // Group Header
        .AddHeader("Visual Settings")
        
        // Button (with hold time)
        .AddButton("Reset Settings", player => ResetSettings(player), 
            holdTime: 2.0f, hint: "Hold for 2 seconds to reset")
        
        // Dropdown (with different types)
        .AddDropdown("Theme", new[] { "Dark", "Light", "Auto" },
            type: SSDropdownSetting.DropdownEntryType.HybridLoop)
        
        // Slider
        .AddSlider("Brightness", 0f, 100f, defaultValue: 75f)
        
        // Keybind
        .AddKeybind("Open Menu", KeyCode.M)
        
        // Plaintext
        .AddPlaintext("Display Name", maxLength: 24)
        
        // Toggle (helper for TwoButtons)
        .AddToggle("Enable Sounds", defaultValue: true,
            (player, enabled) => SetSounds(player, enabled))
        
        // Two Buttons (manual)
        .AddTwoButtons("Gender", "Male", "Female",
            (player, isFemale) => SetGender(player, isFemale))
        
        // Text Area
        .AddTextArea("Notes", SSTextArea.FoldoutMode.CollapseOnEntry)
    .EndPage()
    .Build();
```

### Global Event Callbacks

Instead of individual callbacks per setting, you can use global callbacks:

```csharp
_menu = MenuBuilder.Create("My Menu")
    .OnButtonPressed((player, settingId, button) => {
        Log.Info($"{player.Nickname} pressed button {settingId}: {button.Label}");
    })
    .OnDropdownChanged((player, settingId, selectedIndex, dropdown) => {
        Log.Info($"{player.Nickname} changed dropdown {settingId} to index {selectedIndex}");
    })
    .OnSliderChanged((player, settingId, value, slider) => {
        Log.Info($"{player.Nickname} adjusted slider {settingId} to {value}");
    })
    .AddPage("Test")
        .AddButton("Button 1")
        .AddButton("Button 2")
        .AddDropdown("Dropdown 1", new[] { "A", "B", "C" })
    .EndPage()
    .Build();
```

### Dynamic Updates

Update menu content at runtime:

```csharp
// Store button SettingId when building
int ammoButtonId = 0;
_menu = MenuBuilder.Create("Weapon Info")
    .AddPage("Status")
        .AddButton("Ammo: 30/120", player => Reload(player))
    .EndPage()
    .Build();

// Later, update the button dynamically
SSSUtility.UpdateButtonLabel(Name, ammoButtonId, "Ammo: 15/120");

// Update for specific players only
SSSUtility.UpdateButton(Name, ammoButtonId, 
    "Ammo: 0/120", 
    "Out of ammo!",
    filter: hub => Player.Get(hub).CurrentItem == null);
```

### Player State Tracking

```csharp
// Check which menu/page a player is viewing
var state = SSSUtility.GetPlayerState(player);
if (state != null && state.CurrentMenuPlugin == Name)
{
    Log.Info($"{player.Nickname} is on page {state.CurrentPageIndex}");
}

// Check if ServerSpecific tab is open
if (state?.IsTabOpen == true)
{
    // Player has the menu open
}
```

### Manual Menu Sending

By default, menus are automatically sent to players. You can control this:

```csharp
// Send menu to specific player
SSSUtility.SendMenuToPlayer(player, "MyPlugin", pageIndex: 0);

// Send menu to all players
SSSUtility.SendMenuToAll("MyPlugin");

// Send specific page
SSSUtility.SendMenuToPlayer(player, "MyPlugin", pageIndex: 2);
```

### OnPlayerJoin Callback

Execute code when a player opens your menu:

```csharp
_menu = MenuBuilder.Create("Shop")
    .OnPlayerJoin(player => {
        player.ShowHint("Welcome to the shop!");
        Log.Info($"{player.Nickname} opened shop");
    })
    .AddPage("Items")
        .AddButton("Buy Health", player => BuyHealth(player))
    .EndPage()
    .Build();
```

### Collection IDs

Use different collection IDs for separate menu groups:

```csharp
var adminMenu = MenuBuilder.Create("Admin Menu")
    .WithCollectionId(1)  // Separate from default menus
    .AddPage("Admin")
        .AddButton("Kick Player")
        .AddButton("Ban Player")
    .EndPage()
    .Build();

var playerMenu = MenuBuilder.Create("Player Menu")
    .WithCollectionId(2)  // Different collection
    .AddPage("Player")
        .AddButton("Request Help")
    .EndPage()
    .Build();
```

## Best Practices

1. **Always Unregister**: Call `SSSUtility.UnregisterMenu(Name)` in `OnDisabled()`
2. **Store SettingIds**: If you need to update settings dynamically, store their IDs when building
3. **Use Hints**: Provide helpful hint descriptions for better UX
4. **Page Limits**: Keep pages under 20 entries for better performance
5. **Error Handling**: Wrap callbacks in try-catch to prevent plugin crashes
6. **Test Conflicts**: Test with multiple plugins using SSSUtility to ensure no conflicts

## Advanced: Persistence

SSSUtility automatically handles persistence, but you can access it directly:

```csharp
var settingsManager = Plugin.GetSettingsManager();

// Save a custom setting
settingsManager.SaveSetting(
    userId: player.UserId,
    pluginName: Name,
    settingId: mySettingId,
    value: "custom_value"
);

// Load a setting
string value = settingsManager.LoadSetting(player.UserId, Name, mySettingId);

// Load all settings for a player
var allSettings = settingsManager.LoadAllSettings(player.UserId, Name);
```

## Troubleshooting

### Menu Not Showing
- Ensure SSSUtility.dll is loaded (check server logs)
- Check that you called `RegisterMenu()` in `OnEnabled()`
- Verify your plugin is enabled

### Settings Conflicting
- Each plugin gets a unique SettingId range automatically
- If issues persist, check that no other plugin is manually setting DefinedSettings

### Callbacks Not Firing
- Ensure you're using the correct callback type (Button vs Dropdown, etc.)
- Check server logs for errors
- Verify the setting was added to the page correctly

### Player Kicked When Opening Menu
- This usually means DefinedSettings is incomplete
- SSSUtility handles this automatically - ensure it's loaded first

## API Reference

### MenuBuilder Methods
- `Create(string menuName)` - Creates a new menu builder
- `AddPage(string pageName)` - Adds a new page
- `OnPlayerJoin(Action<Player>)` - Set player join callback
- `WithCollectionId(byte)` - Set collection ID
- `OnButtonPressed(...)` - Set global button callback
- `OnDropdownChanged(...)` - Set global dropdown callback
- `OnSliderChanged(...)` - Set global slider callback
- `Build()` - Finalizes and returns the menu

### PageBuilder Methods
- `AddHeader(string, bool)` - Add group header
- `AddButton(string, Action<Player>, float, string)` - Add button
- `AddDropdown(string, string[], Action<Player, int>, DropdownType, string)` - Add dropdown
- `AddSlider(string, float, float, float, Action<Player, float>, string)` - Add slider
- `AddKeybind(string, KeyCode, Action<Player, KeyCode>, string)` - Add keybind
- `AddPlaintext(string, int, Action<Player, string>, string, string)` - Add text input
- `AddTextArea(string, FoldoutMode, Action<Player, string>, string, string)` - Add text area
- `AddTwoButtons(string, string, string, Action<Player, bool>, string)` - Add two buttons
- `AddToggle(string, bool, Action<Player, bool>, string)` - Add toggle (helper)
- `EndPage()` - Return to menu builder

### SSSUtility Static Methods
- `RegisterMenu(string, Menu)` - Register a menu
- `UnregisterMenu(string)` - Unregister a menu
- `GetMenu(string)` - Get a registered menu
- `GetPlayerState(Player)` - Get player's menu state
- `SendMenuToPlayer(Player, string, int)` - Send menu to player
- `SendMenuToAll(string, int)` - Send menu to all players
- `RefreshAllMenus()` - Rebuild and resend all menus
- `UpdateButtonLabel(...)` - Update button label
- `UpdateButtonHint(...)` - Update button hint
- `UpdateButton(...)` - Update both label and hint

## Support

For issues, suggestions, or contributions, please contact the development team.

## License

This library is provided as-is for use with Exiled plugins.

