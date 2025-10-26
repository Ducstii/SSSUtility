# SSSUtility

A utility plugin for Exiled that simplifies ServerSpecific Settings menu

## Quick Start

```csharp
// Create a menu
var builder = MenuBuilder.Create("My Plugin Menu");

builder.AddPage("labubu")
    .AddHeader("labubu")
    .AddButton("labubu Me", (player) => player.ShowHint("labubu!"))
    .AddKeybind("nobro", KeyCode.R, (player, key) => HandleKeybind(player, key))
    .AddSlider("labubu noise", 0.5f, 0f, 1f, (player, value) => SetVolume(player, value))
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

## Requirements

- Exiled Framework

## License

MIT License - see LICENSE file for details.