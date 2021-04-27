using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Windows.UI.Core;
using Jupiter.Application;

namespace Jupiter.Services.Navigation
{
    public class NavigationService
    {
        public FrameFacade FrameFacade { get; }

        public Frame Frame => FrameFacade.Frame;

        public virtual bool CanGoBack => FrameFacade.CanGoBack;

        public virtual bool IsBackButtonEnabled { get; set; }

        public NavigationService(Frame frame)
        {
            FrameFacade = new FrameFacade(frame);

            FrameFacade.Frame.Navigating += (sender, args) =>
            {
                OnNavigating(args);
            };

            FrameFacade.Frame.Navigated += (sender, args) =>
            {
                OnNavigated(args.NavigationMode, args.Parameter);

                UpdateBackButton();
            };
        }

        public virtual bool Navigate(Type page, object parameter = null, bool clearHistory = false, NavigationTransitionInfo infoOverride = null)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var result = Frame.Navigate(page, parameter, infoOverride);

            if (result && clearHistory)
            {
                Frame.BackStack.Clear();

                UpdateBackButton();
            }

            return result;
        }

        public virtual void GoBack()
        {
            FrameFacade.GoBack();
        }

        public virtual void RemoveBackEntry()
        {
            Frame.BackStack.Remove(Frame.BackStack.LastOrDefault());
        }

        protected virtual bool OnNavigating(NavigatingCancelEventArgs e)
        {
            var page = FrameFacade.Content as Page;
            if (page != null)
            {
                // force (x:bind) page bindings to update
                var fields = page.GetType().GetRuntimeFields();
                var bindings = fields.FirstOrDefault(x => x.Name.Equals("Bindings"));
                if (bindings != null)
                {
                    var update = bindings.GetType().GetTypeInfo().GetDeclaredMethod("Update");
                    update?.Invoke(bindings, null);
                }

                // call navagable override (navigating)
                var dataContext = page.DataContext as INavigable;
                if (dataContext != null)
                {
                    var args = new NavigatingEventArgs
                    {
                        NavigationMode = FrameFacade.NavigationModeHint,
                        PageType = FrameFacade.CurrentPageType,
                        Parameter = FrameFacade.CurrentPageParam,
                        TargetPageType = e.SourcePageType
                        //Suspending = suspending,
                    };
                    //dataContext.SessionState = JupiterApp.Current.SessionState;
                    dataContext.OnNavigatingFrom(args);
                    return !args.Cancel;
                }
            }
            return true;
        }

        protected virtual void OnNavigated(NavigationMode mode, object parameter)
        {
            var page = FrameFacade.Content as Page;
            if (page != null)
            {
                if (page.DataContext == null)
                {
                    // to support dependency injection, but keeping it optional.
                    var viewmodel = JupiterApp.Current.ResolveForPage(page.GetType(), this);
                    if (viewmodel != null)
                        page.DataContext = viewmodel;
                }

                // call viewmodel
                var dataContext = page.DataContext as INavigable;
                if (dataContext != null)
                {
                    // prepare for state load
                    dataContext.NavigationService = this;
                    //dataContext.SessionState = JupiterApp.Current.SessionState;

                    var parameters = parameter as Dictionary<string, object>;
                    dataContext.OnNavigatedTo(parameters, mode);
                }
            }
        }

        protected virtual void UpdateBackButton()
        {
            //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = (Frame.CanGoBack && IsBackButtonEnabled) ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }
    }
}