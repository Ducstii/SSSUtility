namespace SSSUtility.Models;

public class PlayerMenuState
{
    public ReferenceHub Hub { get; set; }
    public string CurrentMenuPlugin { get; set; }
    public int CurrentPageIndex { get; set; }
    public bool IsTabOpen { get; set; }
    public Dictionary<int, object> CurrentSelections { get; set; } = new();
}

