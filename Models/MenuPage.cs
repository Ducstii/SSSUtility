namespace SSSUtility.Models;

public class MenuPage
{
    public string Name { get; set; }
    public List<ServerSpecificSettingBase> OwnEntries { get; set; } = new();
    public ServerSpecificSettingBase[] CombinedEntries { get; set; }
    public Action<Player> OnPageEnter { get; set; }
    public Action<Player> OnPageExit { get; set; }

    public MenuPage(string name)
    {
        Name = name;
    }

    public void GenerateCombinedEntries(ServerSpecificSettingBase[] pageSelectorSection)
    {
        int combinedLength = pageSelectorSection.Length + OwnEntries.Count + 1; // +1 for auto-generated name header
        CombinedEntries = new ServerSpecificSettingBase[combinedLength];

        int nextIndex = 0;

        // Include page selector section
        foreach (var entry in pageSelectorSection)
            CombinedEntries[nextIndex++] = entry;

        // Add auto-generated name header
        CombinedEntries[nextIndex++] = new SSGroupHeader(Name);

        // Include own entries
        foreach (var entry in OwnEntries)
            CombinedEntries[nextIndex++] = entry;
    }
}

