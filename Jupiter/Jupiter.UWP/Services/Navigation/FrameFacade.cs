using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace Jupiter.Services.Navigation
{
    public class FrameFacade
    {
        public Frame Frame { get; }

        public object Content => Frame.Content;

        public bool CanGoBack => Frame.CanGoBack;

        public NavigationMode NavigationModeHint = NavigationMode.New;

        public Type CurrentPageType { get; internal set; }

        public object CurrentPageParam { get; internal set; }

        internal FrameFacade(Frame frame)
        {
            Frame = frame;
            frame.Navigated += FacadeNavigatedEventHandler;
            frame.Navigating += FacadeNavigatingCancelEventHandler;

            if (Frame.ContentTransitions == null)
            {
                // setup animations
                var c = new TransitionCollection { };
                var t = new NavigationThemeTransition { };
                var i = new EntranceNavigationTransitionInfo();
                t.DefaultNavigationTransitionInfo = i;
                c.Add(t);
                Frame.ContentTransitions = c;
            }
        }

        internal void GoBack()
        {
            NavigationModeHint = NavigationMode.Back;
            Frame.GoBack();
        }

        readonly List<EventHandler<NavigatedEventArgs>> _navigatedEventHandlers = new List<EventHandler<NavigatedEventArgs>>();
        public event EventHandler<NavigatedEventArgs> Navigated
        {
            add { if (!_navigatedEventHandlers.Contains(value)) _navigatedEventHandlers.Add(value); }
            remove { if (_navigatedEventHandlers.Contains(value)) _navigatedEventHandlers.Remove(value); }
        }
        void FacadeNavigatedEventHandler(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            CurrentPageType = e.SourcePageType;
            CurrentPageParam = e.Parameter;
            var args = new NavigatedEventArgs(e, Content as Page);
            if (NavigationModeHint != NavigationMode.New)
                args.NavigationMode = NavigationModeHint;
            NavigationModeHint = NavigationMode.New;
            foreach (var handler in _navigatedEventHandlers)
            {
                handler(this, args);
            }
        }

        readonly List<EventHandler<NavigatingEventArgs>> _navigatingEventHandlers = new List<EventHandler<NavigatingEventArgs>>();
        public event EventHandler<NavigatingEventArgs> Navigating
        {
            add { if (!_navigatingEventHandlers.Contains(value)) _navigatingEventHandlers.Add(value); }
            remove { if (_navigatingEventHandlers.Contains(value)) _navigatingEventHandlers.Remove(value); }
        }
        private void FacadeNavigatingCancelEventHandler(object sender, NavigatingCancelEventArgs e)
        {
            var args = new NavigatingEventArgs(e, Content as Page);
            if (NavigationModeHint != NavigationMode.New)
                args.NavigationMode = NavigationModeHint;
            //NavigationModeHint = NavigationMode.New;
            foreach (var handler in _navigatingEventHandlers)
            {
                handler(this, args);
            }
            e.Cancel = args.Cancel;
        }
    }
}