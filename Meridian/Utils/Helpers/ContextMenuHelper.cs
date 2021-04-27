using Meridian.Interfaces;
using Meridian.Model;
using Meridian.ViewModel;
using System.Linq;
using Microsoft.UI.Xaml.Controls;

namespace Meridian.Utils.Helpers
{
    public enum ContextMenuContext
    {
        Common,
        NowPlaying,
        Player
    }

    public static class ContextMenuHelper
    {
        public static MenuFlyout GetTrackMenu(IAudio track, ContextMenuContext context)
        {
            var menu = new MenuFlyout();
            if (context != ContextMenuContext.NowPlaying && context != ContextMenuContext.Player)
            {
                menu.Items?.Add(new MenuFlyoutItem() { Text = Resources.GetStringByKey("ContextMenu_Play"), Command = ViewModelLocator.Main.PlaySingleTrackCommand, CommandParameter = track });

                menu.Items?.Add(new MenuFlyoutItem() { Text = Resources.GetStringByKey("ContextMenu_AddToNowPlaying"), Command = ViewModelLocator.Main.AddTrackToNowPlayingCommand, CommandParameter = track });
                menu.Items?.Add(new MenuFlyoutItem() { Text = Resources.GetStringByKey("ContextMenu_PlayNext"), Command = ViewModelLocator.Main.PlayTrackNextCommand, CommandParameter = track });

                menu.Items?.Add(new MenuFlyoutSeparator());
            }

            if (context != ContextMenuContext.NowPlaying)
            {
                if (track is AudioVk)
                {
                    var vkAudio = (AudioVk)track;
                    if (vkAudio.IsAddedByCurrentUser)
                    {
                        if (context != ContextMenuContext.Player)
                            menu.Items?.Add(new MenuFlyoutItem() { Text = Resources.GetStringByKey("ContextMenu_Edit"), Command = ViewModelLocator.Main.EditTrackCommand, CommandParameter = track });
                        menu.Items?.Add(new MenuFlyoutItem() { Text = Resources.GetStringByKey("ContextMenu_Remove"), Command = ViewModelLocator.Main.RemoveTrackFromMyMusicCommand, CommandParameter = track });
                        menu.Items?.Add(new MenuFlyoutItem() { Text = Resources.GetStringByKey("ContextMenu_AddToPlaylist"), Command = ViewModelLocator.Main.AddTrackToPlaylistCommand, CommandParameter = track });
                    }
                    else
                        menu.Items?.Add(new MenuFlyoutItem() { Text = Resources.GetStringByKey("ContextMenu_AddToMyMusic"), Command = ViewModelLocator.Main.AddTrackToMyMusicCommand, CommandParameter = track });
                }
            }
            else
            {
                if (context != ContextMenuContext.Player)
                    menu.Items?.Add(new MenuFlyoutItem() { Text = Resources.GetStringByKey("ContextMenu_RemoveFromNowPlaying"), Command = ViewModelLocator.Main.RemoveTrackFromNowPlayingCommand, CommandParameter = track });
            }

            if (track is AudioVk)
                menu.Items?.Add(new MenuFlyoutItem() { Text = Resources.GetStringByKey("ContextMenu_Lyrics"), Command = ViewModelLocator.Main.ShowTrackLyricsCommand, CommandParameter = track, IsEnabled = ((AudioVk)track).LyricsId != 0 });

            if (!(menu.Items?.LastOrDefault() is MenuFlyoutSeparator))
                menu.Items?.Add(new MenuFlyoutSeparator());

            menu.Items?.Add(new MenuFlyoutItem() { Text = Resources.GetStringByKey("ContextMenu_FindMore"), Command = ViewModelLocator.Main.FindMoreForTrackCommand, CommandParameter = track });
            menu.Items?.Add(new MenuFlyoutItem() { Text = Resources.GetStringByKey("ContextMenu_CopyTitle"), Command = ViewModelLocator.Main.CopyTrackTitleCommand, CommandParameter = track });

            return menu;
        }

        public static MenuFlyout GetPlaylistMenu(PlaylistVk playlist)
        {
            var menu = new MenuFlyout();

            menu.Items?.Add(new MenuFlyoutItem() { Text = Resources.GetStringByKey("ContextMenu_Play"), Command = ViewModelLocator.Main.PlayPlaylistCommand, CommandParameter = playlist });

            if (playlist.IsAddedByCurrentUser)
            {
                if (playlist.IsEditable)
                    menu.Items?.Add(new MenuFlyoutItem() { Text = Resources.GetStringByKey("ContextMenu_Edit"), Command = ViewModelLocator.Main.EditPlaylistCommand, CommandParameter = playlist });
                menu.Items?.Add(new MenuFlyoutItem() { Text = Resources.GetStringByKey("ContextMenu_Remove"), Command = ViewModelLocator.Main.DeletePlaylistCommand, CommandParameter = playlist });
            }
            else
            {
                menu.Items?.Add(new MenuFlyoutItem() { Text = Resources.GetStringByKey("ContextMenu_FollowPlaylist"), Command = ViewModelLocator.Main.SavePlaylistCommand, CommandParameter = playlist });
            }

            return menu;
        }
    }
}