namespace SSSUtility.API;

using SSSUtility.Models;

/// <summary>
/// Fluent API for building a single page with various setting types.
/// </summary>
public class PageBuilder
{
    private readonly MenuBuilder _menuBuilder;
    private readonly MenuPage _page;
    private int _nextLocalId = 0;

    internal PageBuilder(MenuBuilder menuBuilder, string pageName)
    {
        _menuBuilder = menuBuilder;
        _page = new MenuPage(pageName);
    }

    internal MenuPage Build() => _page;

    /// <summary>
    /// Adds a group header/separator.
    /// </summary>
    public PageBuilder AddHeader(string text, bool reducedPadding = false)
    {
        var header = new SSGroupHeader(_nextLocalId++, text) { ReducedPadding = reducedPadding };
        _page.OwnEntries.Add(header);
        return this;
    }

    /// <summary>
    /// Adds a clickable button.
    /// </summary>
    public PageBuilder AddButton(string label, Action<Player> onPress = null, float holdTime = 0f, string hint = null)
    {
        var button = new SSButton(_nextLocalId++, label, hint ?? label, holdTime);
        _page.OwnEntries.Add(button);

        if (onPress != null)
        {
            _menuBuilder.RegisterButtonCallback(button.SettingId, onPress);
        }

        return this;
    }

    /// <summary>
    /// Adds a dropdown selector.
    /// </summary>
    public PageBuilder AddDropdown(string label, string[] options, Action<Player, int> onChange = null, 
        SSDropdownSetting.DropdownEntryType type = SSDropdownSetting.DropdownEntryType.Regular, string hint = null)
    {
        var dropdown = new SSDropdownSetting(_nextLocalId++, label, options, entryType: type) 
        { 
            HintDescription = hint ?? label 
        };
        _page.OwnEntries.Add(dropdown);

        if (onChange != null)
        {
            _menuBuilder.RegisterDropdownCallback(dropdown.SettingId, onChange);
        }

        return this;
    }

    /// <summary>
    /// Adds a numeric slider.
    /// </summary>
    public PageBuilder AddSlider(string label, float min, float max, float defaultValue = 0f, 
        Action<Player, float> onChange = null, string hint = null)
    {
        var slider = new SSSliderSetting(_nextLocalId++, label, min, max) 
        { 
            HintDescription = hint ?? label,
            SyncFloatValue = defaultValue
        };
        _page.OwnEntries.Add(slider);

        if (onChange != null)
        {
            _menuBuilder.RegisterSliderCallback(slider.SettingId, onChange);
        }

        return this;
    }

    /// <summary>
    /// Adds a keybind input.
    /// </summary>
    public PageBuilder AddKeybind(string label, KeyCode defaultKey = KeyCode.None, 
        Action<Player, KeyCode> onChange = null, string hint = null)
    {
        var keybind = new SSKeybindSetting(_nextLocalId++, label, defaultKey) 
        { 
            HintDescription = hint ?? label 
        };
        _page.OwnEntries.Add(keybind);

        if (onChange != null)
        {
            _menuBuilder.RegisterKeybindCallback(keybind.SettingId, onChange);
        }

        return this;
    }

    /// <summary>
    /// Adds a text input field. Note: Requires Assembly-CSharp with TMPro types.
    /// </summary>
    public PageBuilder AddPlaintext(string label, int maxLength = 64, Action<Player, string> onChange = null, 
        string placeholder = "...", string hint = null)
    {
        // Use reflection to avoid direct TMPro dependency
        // ContentType.Standard = 0
        var contentTypeEnum = typeof(SSPlaintextSetting).GetConstructors()[0].GetParameters()[4].ParameterType;
        var contentType = Enum.ToObject(contentTypeEnum, 0);
        
        var plaintext = (SSPlaintextSetting)Activator.CreateInstance(
            typeof(SSPlaintextSetting),
            _nextLocalId++, label, placeholder, maxLength, contentType);
        
        plaintext.HintDescription = hint ?? label;
        _page.OwnEntries.Add(plaintext);

        if (onChange != null)
        {
            _menuBuilder.RegisterPlaintextCallback(plaintext.SettingId, onChange);
        }

        return this;
    }

    /// <summary>
    /// Adds a multi-line text area. Note: Requires Assembly-CSharp with TMPro types.
    /// </summary>
    public PageBuilder AddTextArea(string label, SSTextArea.FoldoutMode mode = SSTextArea.FoldoutMode.NotCollapsable, 
        Action<Player, string> onChange = null, string placeholder = null, string hint = null)
    {
        // Use reflection to avoid direct TMPro dependency
        // TextAlignmentOptions.TopLeft = 257
        var alignmentEnum = typeof(SSTextArea).GetConstructors()[0].GetParameters()[4].ParameterType;
        var alignment = Enum.ToObject(alignmentEnum, 257);
        
        var textArea = (SSTextArea)Activator.CreateInstance(
            typeof(SSTextArea),
            _nextLocalId++, label, mode, placeholder, alignment);
        
        textArea.HintDescription = hint ?? label;
        _page.OwnEntries.Add(textArea);

        if (onChange != null)
        {
            _menuBuilder.RegisterTextAreaCallback(textArea.SettingId, onChange);
        }

        return this;
    }

    /// <summary>
    /// Adds a two-button control.
    /// </summary>
    public PageBuilder AddTwoButtons(string label, string leftLabel, string rightLabel, 
        Action<Player, bool> onPress = null, string hint = null)
    {
        var twoButtons = new SSTwoButtonsSetting(_nextLocalId++, label, leftLabel, rightLabel) 
        { 
            HintDescription = hint ?? label 
        };
        _page.OwnEntries.Add(twoButtons);

        if (onPress != null)
        {
            _menuBuilder.RegisterTwoButtonsCallback(twoButtons.SettingId, onPress);
        }

        return this;
    }

    /// <summary>
    /// Helper: Adds a toggle (uses TwoButtons with "Off"/"On").
    /// </summary>
    public PageBuilder AddToggle(string label, bool defaultValue = false, Action<Player, bool> onChange = null, string hint = null)
    {
        return AddTwoButtons(label, "Off", "On", onChange, hint);
    }

    /// <summary>
    /// Completes this page and returns to menu builder.
    /// </summary>
    public MenuBuilder EndPage()
    {
        return _menuBuilder;
    }
}

