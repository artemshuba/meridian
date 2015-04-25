using System;
using System.Windows.Controls;
using System.Windows.Interactivity;
using GalaSoft.MvvmLight.Messaging;
using Meridian.Services;
using Meridian.ViewModel.Messages;

namespace Meridian.Behaviours
{
    public class AutoScrollToCurrentItemBehaviour : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            Messenger.Default.Register<CurrentAudioChangedMessage>(this, OnCurrentAudioChanged);

            if (AudioService.CurrentAudio != null)
                AssociatedObject.ScrollIntoView(AudioService.CurrentAudio);
        }

        protected override void OnDetaching()
        {
            Messenger.Default.Unregister<CurrentAudioChangedMessage>(this, OnCurrentAudioChanged);
        }

        private void OnCurrentAudioChanged(CurrentAudioChangedMessage message)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (AssociatedObject != null && message.NewAudio != null)
                {
                    AssociatedObject.ScrollIntoView(message.NewAudio);
                }
            }));
        }
    }
}
