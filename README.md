# SSSUtility

A specialized utility plugin for SCP: Secret Laboratory servers using Exiled that provides a clean, conflict-free API for the game's Server-Specific Settings menu system.

## What is SSSUtility?

SSSUtility is **not** a wrapper around Exiled - it's a parallel utility that works directly with the game's native `ServerSpecificSettingsSync` system to provide a better developer experience.

### Why SSSUtility?

The native `ServerSpecificSettingsSync` is complex and error-prone:
- Manual ID assignment causes conflicts between plugins
- No automatic page management
- Complex event handling without proper abstraction
- No validation or debugging tools

SSSUtility solves these problems with:
- ✅ **Automatic ID conflict resolution** - Each menu gets a unique ID range automatically
- ✅ **Clean menu builder API** - Fluent builder pattern for easy menu creation
- ✅ **Built-in page navigation** - Multi-page menus with automatic page selector
- ✅ **Event handling and callbacks** - Simple callback system for all setting types
- ✅ **Menu validation and diagnostics** - Built-in validation and debugging tools

## Architecture

```
┌─────────────────────────────────────────┐
│   SCP: Secret Laboratory (Game Code)   │
│   - ReferenceHub                        │
│   - ServerSpecificSettingsSync          │
│   - Mirror Networking                   │
└─────────────────────────────────────────┘
           ↓                    ↓
    ┌──────────┐        ┌──────────────┐
    │  Exiled  │        │  SSSUtility  │  ← Both hook game directly
    │  (API)   │        │   (Menus)    │
    └──────────┘        └──────────────┘
           ↓                    ↓
    ┌──────────────────────────────────┐
    │           Plugin                 │  ← Uses both
    │  - Uses Exiled for game features │
    │  - Uses SSSUtility for menus     │
    └──────────────────────────────────┘
```

## Quick Start

### Installation

1. Download the latest SSSUtility.dll
2. Place it in your `Exiled/Plugins` folder
3. Restart your server

### Basic Example

```csharp
using SSSUtility.API;
using SSSUtility.Models;
using Exiled.API.Features;

public class MyPlugin : Plugin<Config>
{
    public override void OnEnabled()
    {
        // Create a menu using the builder
        var builder = MenuBuilder.Create("My Settings");
        
        builder.AddPage("General Settings")
            .AddHeader("Player Preferences")
            .AddButton("Toggle Night Vision", 
                (player) => {
                    player.EnableEffect(EffectType.NightVision, 9999);
                    player.ShowHint("Night vision toggled!", 3f);
                },
                hint: "Toggle night vision on/off"
            )
            .AddSlider("Master Volume", 
                1.0f, 0f, 1f, 
                (player, value) => {
                    // Apply volume setting
                }
            )
            .AddKeybind("Radio Frequency Switch", 
                KeyCode.R, 
                (player, key) => {
                    // Handle keybind change
                }
            )
        .EndPage();
        
        // Build and register the menu
        var menu = builder.Build();
        SSSUtilityAPI.RegisterMenu("MyPlugin", menu);
    }
    
    public override void OnDisabled()
    {
        // Clean up
        SSSUtilityAPI.UnregisterMenu("MyPlugin");
    }
}
```

### Sending Menus to Players

```csharp
using Exiled.Events.EventArgs.Player;
using SSSUtility.API;
using MEC;

public override void OnEnabled()
{
    // ... menu creation code ...
    
    // Register event handler to send menu to players when they join
    Exiled.Events.Handlers.Player.Verified += OnPlayerVerified;
}

public override void OnDisabled()
{
    Exiled.Events.Handlers.Player.Verified -= OnPlayerVerified;
}

private void OnPlayerVerified(VerifiedEventArgs ev)
{
    // Send menu after a short delay to ensure player is fully loaded
    Timing.CallDelayed(0.5f, () => 
    {
        if (ev.Player?.ReferenceHub != null && ev.Player.IsConnected)
        {
            SSSUtilityAPI.SendMenuToPlayer(ev.Player, "MyPlugin", 0);
        }
    });
}
```

## Complete API Reference

### SSSUtilityAPI (Public API)

The main entry point for interacting with SSSUtility:

```csharp
// Register a menu (calls ConflictResolver automatically)
SSSUtilityAPI.RegisterMenu(string pluginName, Menu menu);

// Unregister a menu
SSSUtilityAPI.UnregisterMenu(string pluginName);

// Get a registered menu
Menu menu = SSSUtilityAPI.GetMenu(string pluginName);

// Get player's menu state
PlayerMenuState state = SSSUtilityAPI.GetPlayerState(Player player);

// Send menu to a specific player
SSSUtilityAPI.SendMenuToPlayer(Player player, string pluginName, int pageIndex = 0);

// Send menu to all players
SSSUtilityAPI.SendMenuToAll(string pluginName, int pageIndex = 0);

// Refresh all players with updated menu data
SSSUtilityAPI.RefreshAllMenus();

// Update button label dynamically
SSSUtilityAPI.UpdateButtonLabel(string pluginName, int settingId, string newLabel, Func<ReferenceHub, bool> filter = null);

// Update button hint dynamically
SSSUtilityAPI.UpdateButtonHint(string pluginName, int settingId, string newHint, Func<ReferenceHub, bool> filter = null);

// Update button label and hint
SSSUtilityAPI.UpdateButton(string pluginName, int settingId, string newLabel, string newHint = null, Func<ReferenceHub, bool> filter = null);
```

### MenuBuilder

Creates menus using a fluent builder pattern:

```csharp
var builder = MenuBuilder.Create(string menuName);
Menu menu = builder.Build();
```

### PageBuilder

Add pages and settings to your menu. **All methods return the PageBuilder for chaining.**

#### Headers and Labels

```csharp
page.AddHeader(string text);           // Section header
page.AddPlaintext(string text);        // Read-only text
page.AddTextArea(string text, int lines);  // Read-only text area
```

#### Buttons

```csharp
page.AddButton(
    string label, 
    Action<Player> callback,
    string hint = null
);

// Example
page.AddButton("Enable Spectator Chat", 
    (player) => player.ShowHint("Spectator chat enabled!"),
    hint: "Toggle spectator chat on/off"
);
```

#### Toggles (Two Buttons)

```csharp
page.AddTwoButtons(
    string label,
    string optionA,
    string optionB,
    bool defaultIsB,
    Action<Player, bool> callback  // true if option B selected
);

// Example
page.AddTwoButtons("Chat Filter",
    "Enabled",
    "Disabled", 
    false,  // Start with "Enabled"
    (player, isB) => {
        var status = isB ? "Disabled" : "Enabled";
        player.ShowHint($"Chat filter: {status}");
    }
);
```

#### Sliders

```csharp
page.AddSlider(
    string label,
    float defaultValue,
    float minValue,
    float maxValue,
    Action<Player, float> callback
);

// Example
page.AddSlider("Brightness",
    0.8f,
    0f, 
    1f,
    (player, value) => {
        // Apply brightness setting
    }
);
```

#### Dropdowns

```csharp
page.AddDropdown(
    string label,
    string[] options,
    int defaultIndex,
    Action<Player, int> callback  // selected index
);

// Example
page.AddDropdown("Language",
    new[] { "English", "Spanish", "French" },
    0,  // Default to English
    (player, index) => {
        var language = options[index];
        player.ShowHint($"Language set to: {language}");
    }
);
```

#### Keybinds

```csharp
page.AddKeybind(
    string label,
    KeyCode defaultKey,
    Action<Player, KeyCode> callback  // new keycode
);

// Example
page.AddKeybind("Toggle Flashlight",
    KeyCode.F,
    (player, key) => {
        player.ShowHint($"Flashlight key set to: {key}");
    }
);
```

### PageManager

Manage per-player menu states and page navigation:

```csharp
// Get or create player's menu state
PlayerMenuState state = PageManager.GetOrCreateState(ReferenceHub hub);

// Get existing state (returns null if doesn't exist)
PlayerMenuState state = PageManager.GetState(ReferenceHub hub);

// Switch player to a different page in current menu
PageManager.SwitchPage(ReferenceHub hub, Menu menu, int pageIndex);

// Send a menu to a player
PageManager.SendMenu(ReferenceHub hub, Menu menu, int pageIndex = 0);

// Remove player's menu state (call on disconnect)
PageManager.RemoveState(ReferenceHub hub);
```

### SettingQueries

Query player settings and menu state:

```csharp
// Get a setting value for a player
T setting = SettingQueries.GetPlayerSettingValue<T>(Player player, int settingId);
bool got = SettingQueries.TryGetPlayerSettingValue<T>(Player player, int settingId, out T value);

// Check if player's Server-Specific Settings tab is open
bool isOpen = SettingQueries.IsPlayerTabOpen(Player player);

// Get player's menu version
int version = SettingQueries.GetPlayerVersion(Player player);

// Check if player needs menu update
bool outdated = SettingQueries.IsPlayerMenuOutdated(Player player);

// Get player's current page in a menu
int page = SettingQueries.GetPlayerCurrentPage(Player player, string menuPluginName);

// Check if player is viewing a menu
bool viewing = SettingQueries.IsPlayerViewingMenu(Player player, string menuPluginName);
```

## Multi-Page Menus

SSSUtility automatically creates a page selector dropdown when you have multiple pages:

```csharp
var builder = MenuBuilder.Create("Roleplay Menu");

// Page 1: General
builder.AddPage("General")
    .AddHeader("General Settings")
    .AddButton("Respawn Settings", ...)
.EndPage();

// Page 2: Roles
builder.AddPage("Roles")
    .AddHeader("Role Selection")
    .AddButton("Security Officer", ...)
    .AddButton("Scientist", ...)
.EndPage();

// Page 3: Keybinds
builder.AddPage("Keybinds")
    .AddHeader("Keybinds")
    .AddKeybind("Radio", KeyCode.R, ...)
.EndPage();

var menu = builder.Build();
SSSUtilityAPI.RegisterMenu("MyPlugin", menu);

// When player opens menu, they'll see a dropdown at the top to switch between pages
```

**Note:** The page selector is automatically added by SSSUtility. You don't need to create it manually.

## Advanced Features

### Dynamic Menu Updates

You can update menu elements without rebuilding the entire menu:

```csharp
// Update a button's label for all players
SSSUtilityAPI.UpdateButtonLabel("MyPlugin", buttonSettingId, "New Label");

// Update only for specific players
SSSUtilityAPI.UpdateButtonLabel("MyPlugin", buttonSettingId, "VIP Only", 
    (hub) => hub.isLocalPlayer.Vip();  // Only for VIP players
);

// Refresh all players viewing the menu
SSSUtilityAPI.RefreshAllMenus();
```

### Per-Player State

Each player has a menu state that tracks their current menu and page:

```csharp
var state = SSSUtilityAPI.GetPlayerState(player);
if (state != null)
{
    Log.Info($"Player is on menu '{state.CurrentMenuPlugin}', page {state.CurrentPageIndex}");
    Log.Info($"Tab is {(state.IsTabOpen ? "open" : "closed")}");
}
```

### Menu Validation

SSSUtility validates menus automatically, but you can also validate manually:

```csharp
using SSSUtility.API;

var validationResult = MenuValidator.ValidateMenu(menu, "MyPlugin");
if (!validationResult.IsValid)
{
    foreach (var error in validationResult.Errors)
    {
        Log.Error($"Validation error: {error}");
    }
}

// Or use the simpler version that logs to console
bool isValid = MenuValidator.ValidateAndLog(menu, "MyPlugin");
```

## Best Practices

### Timing

Always send menus to players after a short delay to ensure they're fully loaded:

```csharp
    Timing.CallDelayed(0.5f, () => {
        SSSUtilityAPI.SendMenuToPlayer(ev.Player, "MyPlugin", 0);
    });
```

### Memory Management

Clean up on player disconnect:

```csharp
public override void OnDisabled()
{
    Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;
}

private void OnPlayerLeft(LeftEventArgs ev)
{
    // PageManager.RemoveState is called automatically by SSSUtility
    // But you can also manually remove if needed
}
```

### Error Handling

SSSUtility uses automatic ID resolution, so you don't need to worry about ID conflicts. Just build your menu and register it:

```csharp
try
{
    var menu = builder.Build();
    
    // Validation is automatic, but you can double-check
    if (MenuValidator.ValidateAndLog(menu, "MyPlugin"))
    {
        SSSUtilityAPI.RegisterMenu("MyPlugin", menu);
    }
}
catch (Exception ex)
{
    Log.Error($"Failed to create menu: {ex.Message}");
}
```

## Troubleshooting

### Common Issues

#### Menu doesn't appear

1. **Check registration**: Ensure `SSSUtilityAPI.RegisterMenu()` was called successfully
2. **Check timing**: Use `Timing.CallDelayed(0.5f, ...)` to send menu after player loads
3. **Check permissions**: Player must be on the server (Verified event fired)

#### Settings don't work

1. **Validate callbacks**: Ensure all callbacks are non-null
2. **Check setting types**: Use correct callback signature for each setting type
3. **Check logs**: Enable debug mode for detailed logging

### Debug Mode

Enable debug mode in your SSSUtility config:

```yaml
sssutility:
  is_enabled: true
  debug: true  # Enable detailed logging
```

### Diagnostics

SSSUtility provides diagnostic tools:

```csharp
using SSSUtility.Core;

// Log complete diagnostics
DiagnosticManager.LogDiagnostics();

// Perform health check
var health = DiagnosticManager.PerformHealthCheck();
if (!health.IsHealthy)
{
    foreach (var issue in health.Issues)
    {
        Log.Warn($"SSSUtility issue: {issue}");
    }
}

// Validate all registered menus
bool allValid = DiagnosticManager.ValidateAllMenus();
```

## Requirements

- SCP: Secret Laboratory
- Exiled Framework 9.0.0+
- .NET Framework 4.8.1

## License

MIT License - see LICENSE file for details.
