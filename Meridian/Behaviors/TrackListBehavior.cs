using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Jupiter.Utils.Extensions;
using Meridian.Services;
using Meridian.View.Controls;
using Microsoft.Xaml.Interactivity;

namespace Meridian.Behaviors
{
    public class TracksListBehavior : DependencyObject, IBehavior
    {
        public DependencyObject AssociatedObject { get; set; }

        public void Attach(DependencyObject associatedObject)
        {
            //listView.SelectionChanged += ListViewOnSelectionChanged;

            AudioService.Instance.CurrentAudioChanged += AudioServiceOnCurrentAudioChanged;

            AssociatedObject = associatedObject;
        }

        public void Detach()
        {
            AudioService.Instance.CurrentAudioChanged -= AudioServiceOnCurrentAudioChanged;

            //var listView = (ListView)AssociatedObject;
            //listView.SelectionChanged += ListViewOnSelectionChanged;
        }

        private void AudioServiceOnCurrentAudioChanged(object sender, EventArgs eventArgs)
        {
            var listView = (ListView)AssociatedObject;
            listView.ScrollIntoView(AudioService.Instance.CurrentPlaylist?.CurrentItem);

            if (listView.Items != null)
            {
                //update style for items
                for (int i = 0; i < listView.Items.Count; i++)
                {
                    //get item container
                    var itemContainer = listView.ContainerFromIndex(i) as ListViewItem;
                    if (itemContainer != null)
                    {
                        //update style
                        var trackControl =
                            itemContainer.GetVisualDescendents().OfType<TrackControl>().FirstOrDefault();
                        if (trackControl != null)
                        {
                            trackControl.IsPlaying = AudioService.Instance.CurrentPlaylist?.CurrentItem ==
                                                     trackControl.Track;
                        }
                    }
                }
            }
        }
    }
}
