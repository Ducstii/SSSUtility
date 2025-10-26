# SSSUtility

A utility plugin for Exiled that simplifies ServerSpecific Settings menu creation and management.

## Features

- **Fluent API**: Easy-to-use MenuBuilder for constructing complex menus
- **Automatic ID Management**: Centralized SettingId allocation to prevent conflicts
- **Multi-Page Navigation**: Built-in page management and navigation
- **Event Routing**: Centralized event handling for all menu interactions
- **Player Keybinds**: Support for player-configurable keybinds
- **Settings Persistence**: Automatic saving and loading of player settings

## Quick Start

```csharp
// Create a menu
var builder = MenuBuilder.Create("My Plugin Menu");

builder.AddPage("Main Page")
    .AddHeader("Welcome")
    .AddButton("Click Me", (player) => player.ShowHint("Hello!"))
    .AddKeybind("My Keybind", KeyCode.R, (player, key) => HandleKeybind(player, key))
    .AddSlider("Volume", 0.5f, 0f, 1f, (player, value) => SetVolume(player, value))
.EndPage();

// Register the menu
var menu = builder.Build();
MenuRegistry.Register("MyPlugin", menu);
```

## API Reference

### MenuBuilder

- `Create(string name)` - Create a new menu builder
- `AddPage(string name)` - Add a new page to the menu
- `Build()` - Build the final menu

### PageBuilder

- `AddHeader(string text)` - Add a header to the page
- `AddButton(string text, Action<Player> callback)` - Add a button
- `AddKeybind(string text, KeyCode defaultKey, Action<Player, KeyCode> callback)` - Add a keybind setting
- `AddSlider(string text, float defaultValue, float min, float max, Action<Player, float> callback)` - Add a slider
- `AddDropdown(string text, string[] options, int defaultIndex, Action<Player, int> callback)` - Add a dropdown
- `EndPage()` - Finish the current page

### MenuRegistry

- `Register(string pluginName, Menu menu)` - Register a menu
- `Unregister(string pluginName)` - Unregister a menu
- `GetMenu(string pluginName)` - Get a registered menu

## Architecture

SSSUtility provides a clean abstraction layer over Exiled's ServerSpecific Settings system:

- **MenuBuilder**: Fluent API for menu construction
- **MenuRegistry**: Centralized menu management
- **EventManager**: Handles all SSS events and routes them to appropriate callbacks
- **PageManager**: Manages page navigation and state
- **SettingIdRegistry**: Prevents ID conflicts between plugins

## Requirements

- Exiled Framework
- .NET Framework 4.8.1
- SCP: Secret Laboratory

## License

MIT License - see LICENSE file for details.