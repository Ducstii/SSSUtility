namespace SSSUtility.Core;

using SSSUtility.Models;

public static class UpdateManager
{
    public static void UpdateButtonLabel(Menu menu, int settingId, string newLabel, Func<ReferenceHub, bool> filter = null)
    {
        if (!menu.SettingMap.TryGetValue(settingId, out var setting) || setting is not SSButton button)
        {
            Log.Warn($"[SSSUtility] Setting {settingId} is not a button in menu '{menu.Name}'");
            return;
        }

        button.SendLabelUpdate(newLabel, applyOverride: true, receiveFilter: filter);
    }

    public static void UpdateButtonHint(Menu menu, int settingId, string newHint, Func<ReferenceHub, bool> filter = null)
    {
        if (!menu.SettingMap.TryGetValue(settingId, out var setting) || setting is not SSButton button)
        {
            Log.Warn($"[SSSUtility] Setting {settingId} is not a button in menu '{menu.Name}'");
            return;
        }

        button.SendHintUpdate(newHint, applyOverride: true, receiveFilter: filter);
    }

    public static void UpdateButton(Menu menu, int settingId, string newLabel, string newHint = null, Func<ReferenceHub, bool> filter = null)
    {
        if (!menu.SettingMap.TryGetValue(settingId, out var setting) || setting is not SSButton button)
        {
            Log.Warn($"[SSSUtility] Setting {settingId} is not a button in menu '{menu.Name}'");
            return;
        }

        button.SendUpdate(newLabel, newHint ?? newLabel, applyOverride: true, receiveFilter: filter);
    }

    public static void UpdateLabel(Menu menu, int settingId, string newLabel, Func<ReferenceHub, bool> filter = null)
    {
        if (!menu.SettingMap.TryGetValue(settingId, out var setting))
        {
            Log.Warn($"[SSSUtility] Setting {settingId} not found in menu '{menu.Name}'");
            return;
        }

        setting.SendLabelUpdate(newLabel, applyOverride: true, receiveFilter: filter);
    }

    public static void UpdateHint(Menu menu, int settingId, string newHint, Func<ReferenceHub, bool> filter = null)
    {
        if (!menu.SettingMap.TryGetValue(settingId, out var setting))
        {
            Log.Warn($"[SSSUtility] Setting {settingId} not found in menu '{menu.Name}'");
            return;
        }

        setting.SendHintUpdate(newHint, applyOverride: true, receiveFilter: filter);
    }

    public static void RefreshPlayerPage(ReferenceHub hub, Menu menu)
    {
        var state = PageManager.GetState(hub);
        if (state == null || state.CurrentMenuPlugin != menu.PluginName)
        {
            Log.Warn($"[SSSUtility] Player {hub.nicknameSync.MyNick} is not viewing menu '{menu.Name}'");
            return;
        }

        PageManager.SendMenu(hub, menu, state.CurrentPageIndex);
    }

    public static void RefreshAllPlayers(Menu menu)
    {
        foreach (var hub in ReferenceHub.AllHubs)
        {
            var state = PageManager.GetState(hub);
            if (state != null && state.CurrentMenuPlugin == menu.PluginName)
            {
                PageManager.SendMenu(hub, menu, state.CurrentPageIndex);
            }
        }
    }
}

