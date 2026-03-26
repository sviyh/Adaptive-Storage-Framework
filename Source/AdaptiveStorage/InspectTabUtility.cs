// Copyright (c) 2023 bradson
// This Source Code Form is subject to the terms of the MIT license.
// If a copy of the license was not distributed with this file,
// You can obtain one at https://opensource.org/licenses/MIT/.

using AdaptiveStorage.Fishery;

namespace AdaptiveStorage;

public static class InspectTabUtility
{
	public static IEnumerable<InspectTabBase> Modify(IEnumerable<InspectTabBase>? tabs)
	{
		var hasUnknownTab = false;

		if (tabs != null)
		{
			foreach (var tab in tabs)
			{
				if (tab == null)
					continue;

				if (tab.labelKey.Translate() == ContentsITab.LabelTranslated)
				{
					if (AdaptiveStorageFrameworkSettings.KnownLoadedContentsTabs.Contains(tab))
						continue;
					else
						hasUnknownTab = true;
				}

				yield return tab;
			}
		}

		var settingsContentsTab = AdaptiveStorageFrameworkSettings.ContentsTab;
		if (!hasUnknownTab && settingsContentsTab is { } selectedContentsTab)
			yield return selectedContentsTab;
		
		if (settingsContentsTab is ContentsITab)
			yield return GroupContentsITab.Instance;
	}

	public static void TryOpen(ISelectable selectable) // for automatic tab opening, similar to LWM's
		// https://github.com/lilwhitemouse/RimWorld-LWM.DeepStorage/blob/master/DeepStorage/Deep_Storage_ITab.cs#L237-L306
	{
		if (!AdaptiveStorageFrameworkSettings.AutomaticallyOpenContentsTab || selectable.GetInspectTabs() is not { } inspectTabs)
			return;

		using var tabs = inspectTabs.ToPooledList();

		if (tabs.Count == 0)
			return;

		// IsVisible means we only consider tabs that are actually visible; i.e. present in the tab list of the newly clicked selectable.
		// This is relevant for example when you have a grouped storage building selected and click onto an individual, ungrouped one.
		// In that scenario, without the IsVisible check, we'd return here and not display any tabs.
		var tabAlreadyOpened = tabs.Exists(static tab => tab.IsVisible
			&& InspectPaneUtility.IsOpen(tab, (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow));

		if (tabAlreadyOpened)
			return;

		var groupTab = AdaptiveStorageFrameworkSettings.PreferGroupTabWhenGrouped
			? tabs.Find(static tab => tab is GroupContentsITab)
			: null;

		if (groupTab != null && groupTab.IsVisible)
		{
			InspectPaneUtility.OpenTab(groupTab.GetType());
			return;
		}

		var selectedContentsTab = AdaptiveStorageFrameworkSettings.ContentsTab;
		var selectedContentsTabExists = selectedContentsTab is not null && tabs.Contains(selectedContentsTab) && selectedContentsTab.IsVisible;

		var tab = selectedContentsTabExists
					? selectedContentsTab
					: tabs.Find(static tab => tab is ITab_Storage);

		if (tab is null)
			return;

		InspectPaneUtility.OpenTab(tab.GetType());
	}
}
