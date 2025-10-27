namespace SSSUtility.Models;

public class Menu
{
    public string Name { get; set; }
    public string PluginName { get; set; }
    public List<MenuPage> Pages { get; set; } = new();
    public byte CollectionId { get; set; } = 255;
    public int IdRangeStart { get; set; }
    public int IdRangeEnd { get; set; }
    
    // Page selector dropdown
    public SSDropdownSetting PageSelectorDropdown { get; set; }
    public ServerSpecificSettingBase[] PinnedSection { get; set; } = Array.Empty<ServerSpecificSettingBase>();
    
    // Callbacks
    public Action<Player> OnPlayerJoin { get; set; }
    public Action<Player, int, SSButton> OnButtonPressed { get; set; }
    public Action<Player, int, int, SSDropdownSetting> OnDropdownChanged { get; set; }
    public Action<Player, int, float, SSSliderSetting> OnSliderChanged { get; set; }
    public Action<Player, int, KeyCode, SSKeybindSetting> OnKeybindChanged { get; set; }
    public Action<Player, int, string, SSPlaintextSetting> OnPlaintextChanged { get; set; }
    public Action<Player, int, bool, SSTwoButtonsSetting> OnTwoButtonsPressed { get; set; }
    public Action<Player, int, string, SSTextArea> OnTextAreaChanged { get; set; }
    
    // Mapping of original SettingId to Setting
    public Dictionary<int, ServerSpecificSettingBase> SettingMap { get; set; } = new();
    
    // Mapping of SettingId to callbacks (for individual setting callbacks)
    public Dictionary<int, Action<Player>> ButtonCallbacks { get; set; } = new();
    public Dictionary<int, Action<Player, int>> DropdownCallbacks { get; set; } = new();
    public Dictionary<int, Action<Player, float>> SliderCallbacks { get; set; } = new();
    public Dictionary<int, Action<Player, KeyCode>> KeybindCallbacks { get; set; } = new();
    public Dictionary<int, Action<Player, string>> PlaintextCallbacks { get; set; } = new();
    public Dictionary<int, Action<Player, bool>> TwoButtonsCallbacks { get; set; } = new();
    public Dictionary<int, Action<Player, string>> TextAreaCallbacks { get; set; } = new();

    public Menu(string name)
    {
        Name = name;
    }

    public void RemapIds(int startId)
    {
        IdRangeStart = startId;
        int currentId = startId;
        var oldToNewMap = new Dictionary<int, int>();

        // Remap page selector if exists
        if (PageSelectorDropdown != null)
        {
            int oldId = PageSelectorDropdown.SettingId;
            PageSelectorDropdown.SetId(currentId, PageSelectorDropdown.Label);
            oldToNewMap[oldId] = currentId;
            currentId++;
        }

        // Remap all page entries
        foreach (var page in Pages)
        {
            foreach (var entry in page.OwnEntries)
            {
                int oldId = entry.SettingId;
                entry.SetId(currentId, entry.Label);
                oldToNewMap[oldId] = currentId;
                SettingMap[currentId] = entry;
                currentId++;
            }
        }

        IdRangeEnd = currentId - 1;

        // Remap all callback dictionaries
        RemapCallbackDictionary(ButtonCallbacks, oldToNewMap);
        RemapCallbackDictionary(DropdownCallbacks, oldToNewMap);
        RemapCallbackDictionary(SliderCallbacks, oldToNewMap);
        RemapCallbackDictionary(KeybindCallbacks, oldToNewMap);
        RemapCallbackDictionary(PlaintextCallbacks, oldToNewMap);
        RemapCallbackDictionary(TwoButtonsCallbacks, oldToNewMap);
        RemapCallbackDictionary(TextAreaCallbacks, oldToNewMap);
    }

    private void RemapCallbackDictionary<T>(Dictionary<int, T> dict, Dictionary<int, int> oldToNewMap)
    {
        var entries = dict.ToList();
        dict.Clear();
        foreach (var entry in entries)
        {
            if (oldToNewMap.TryGetValue(entry.Key, out int newId))
            {
                dict[newId] = entry.Value;
            }
        }
    }

    public IEnumerable<ServerSpecificSettingBase> GetAllSettings()
    {
        if (PageSelectorDropdown != null)
            yield return PageSelectorDropdown;

        foreach (var page in Pages)
        {
            foreach (var entry in page.OwnEntries)
            {
                yield return entry;
            }
        }
    }
}

