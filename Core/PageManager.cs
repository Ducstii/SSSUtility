namespace SSSUtility.Core;

using SSSUtility.Models;

public static class PageManager
{
    private static readonly Dictionary<ReferenceHub, PlayerMenuState> _playerStates = new();
    private static readonly object _lock = new();

    public static PlayerMenuState GetOrCreateState(ReferenceHub hub)
    {
        lock (_lock)
        {
            if (!_playerStates.TryGetValue(hub, out var state))
            {
                state = new PlayerMenuState
                {
                    Hub = hub,
                    CurrentPageIndex = 0
                };
                _playerStates[hub] = state;
            }
            return state;
        }
    }

    public static PlayerMenuState GetState(ReferenceHub hub)
    {
        lock (_lock)
        {
            return _playerStates.TryGetValue(hub, out var state) ? state : null;
        }
    }

    public static void SwitchPage(ReferenceHub hub, Menu menu, int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= menu.Pages.Count)
        {
            Log.Warn($"[SSSUtility] Invalid page index {pageIndex} for menu '{menu.Name}'");
            return;
        }

        var state = GetOrCreateState(hub);
        int oldPageIndex = state.CurrentPageIndex;
        state.CurrentPageIndex = pageIndex;
        state.CurrentMenuPlugin = menu.PluginName;

        // Call page callbacks
        if (oldPageIndex >= 0 && oldPageIndex < menu.Pages.Count)
        {
            var player = Player.Get(hub);
            menu.Pages[oldPageIndex].OnPageExit?.Invoke(player);
        }

        var newPage = menu.Pages[pageIndex];
        if (newPage.OnPageEnter != null)
        {
            var player = Player.Get(hub);
            newPage.OnPageEnter?.Invoke(player);
        }

        // Send new page content
        ServerSpecificSettingsSync.SendToPlayer(hub, newPage.CombinedEntries);

        Log.Debug($"[SSSUtility] Switched player {hub.nicknameSync.MyNick} to page {pageIndex} ({newPage.Name})");
    }

    public static void SendMenu(ReferenceHub hub, Menu menu, int pageIndex = 0)
    {
        if (menu.Pages.Count == 0)
        {
            Log.Warn($"[SSSUtility] Menu '{menu.Name}' has no pages");
            return;
        }

        if (pageIndex < 0 || pageIndex >= menu.Pages.Count)
            pageIndex = 0;

        var state = GetOrCreateState(hub);
        state.CurrentMenuPlugin = menu.PluginName;
        state.CurrentPageIndex = pageIndex;

        var page = menu.Pages[pageIndex];
        ServerSpecificSettingsSync.SendToPlayer(hub, page.CombinedEntries);

        // Invoke OnPlayerJoin callback
        if (menu.OnPlayerJoin != null)
        {
            var player = Player.Get(hub);
            menu.OnPlayerJoin?.Invoke(player);
        }

        Log.Debug($"[SSSUtility] Sent menu '{menu.Name}' page {pageIndex} to {hub.nicknameSync.MyNick}");
    }

    public static void RemoveState(ReferenceHub hub)
    {
        lock (_lock)
        {
            _playerStates.Remove(hub);
        }
    }

    public static void Clear()
    {
        lock (_lock)
        {
            _playerStates.Clear();
        }
    }
}

