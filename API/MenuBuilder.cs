namespace SSSUtility.API;

using SSSUtility.Models;

public class MenuBuilder
{
    private readonly Menu _menu;
    private readonly List<PageBuilder> _pageBuilders = new();

    private MenuBuilder(string menuName)
    {
        _menu = new Menu(menuName);
    }

    public static MenuBuilder Create(string menuName)
    {
        return new MenuBuilder(menuName);
    }

    public PageBuilder AddPage(string pageName)
    {
        var pageBuilder = new PageBuilder(this, pageName);
        _pageBuilders.Add(pageBuilder);
        return pageBuilder;
    }

    public MenuBuilder OnPlayerJoin(Action<Player> callback)
    {
        _menu.OnPlayerJoin = callback;
        return this;
    }

    public MenuBuilder WithCollectionId(byte collectionId)
    {
        _menu.CollectionId = collectionId;
        return this;
    }

    public MenuBuilder OnButtonPressed(Action<Player, int, SSButton> callback)
    {
        _menu.OnButtonPressed = callback;
        return this;
    }

    public MenuBuilder OnDropdownChanged(Action<Player, int, int, SSDropdownSetting> callback)
    {
        _menu.OnDropdownChanged = callback;
        return this;
    }

    public MenuBuilder OnSliderChanged(Action<Player, int, float, SSSliderSetting> callback)
    {
        _menu.OnSliderChanged = callback;
        return this;
    }

    public MenuBuilder OnKeybindChanged(Action<Player, int, KeyCode, SSKeybindSetting> callback)
    {
        _menu.OnKeybindChanged = callback;
        return this;
    }

    public MenuBuilder OnPlaintextChanged(Action<Player, int, string, SSPlaintextSetting> callback)
    {
        _menu.OnPlaintextChanged = callback;
        return this;
    }

    public Menu Build()
    {
        // Build all pages
        foreach (var pageBuilder in _pageBuilders)
        {
            _menu.Pages.Add(pageBuilder.Build());
        }

        // Create page selector if multiple pages
        if (_menu.Pages.Count > 1)
        {
            string[] pageOptions = _menu.Pages
                .Select((p, i) => $"{p.Name} ({i + 1} of {_menu.Pages.Count})")
                .ToArray();

            _menu.PageSelectorDropdown = new SSDropdownSetting(null, "Page", pageOptions, 
                entryType: SSDropdownSetting.DropdownEntryType.HybridLoop);

            _menu.PinnedSection = new ServerSpecificSettingBase[] { _menu.PageSelectorDropdown };
        }
        else
        {
            _menu.PinnedSection = Array.Empty<ServerSpecificSettingBase>();
        }

        // Generate combined entries for all pages
        foreach (var page in _menu.Pages)
        {
            page.GenerateCombinedEntries(_menu.PinnedSection);
        }

        return _menu;
    }

    internal void RegisterButtonCallback(int settingId, Action<Player> callback)
    {
        _menu.ButtonCallbacks[settingId] = callback;
    }

    internal void RegisterDropdownCallback(int settingId, Action<Player, int> callback)
    {
        _menu.DropdownCallbacks[settingId] = callback;
    }

    internal void RegisterSliderCallback(int settingId, Action<Player, float> callback)
    {
        _menu.SliderCallbacks[settingId] = callback;
    }

    internal void RegisterKeybindCallback(int settingId, Action<Player, KeyCode> callback)
    {
        _menu.KeybindCallbacks[settingId] = callback;
    }

    internal void RegisterPlaintextCallback(int settingId, Action<Player, string> callback)
    {
        _menu.PlaintextCallbacks[settingId] = callback;
    }

    internal void RegisterTwoButtonsCallback(int settingId, Action<Player, bool> callback)
    {
        _menu.TwoButtonsCallbacks[settingId] = callback;
    }

    internal void RegisterTextAreaCallback(int settingId, Action<Player, string> callback)
    {
        _menu.TextAreaCallbacks[settingId] = callback;
    }
}

