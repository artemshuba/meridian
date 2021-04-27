using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Jupiter.Services.Navigation;

namespace Jupiter.Application
{
    public enum StartKind
    {
        Launch,
        Activate
    }

    public abstract class JupiterApp : Microsoft.UI.Xaml.Application
    {
        private bool _isMinimized = false;
        private bool _isActive = true;

        public new static JupiterApp Current { get; private set; }

        public NavigationService NavigationService => NavigationServices.Default;

        public NavigationServiceList NavigationServices { get; } = new NavigationServiceList();

        public bool IsMinimized => _isMinimized;

        public bool IsActive => _isActive;

        public bool IsBackButtonEnabled { get; set; }

        public IStateItems SessionState { get; set; } = new StateItems();

        protected JupiterApp()
        {
            Current = this;
            this.UnhandledException += BootStrapper_UnhandledException;
            //this.Suspending += BootStrapper_Suspending;
        }

        #region Dependency injection

        public virtual INavigable ResolveForPage(Type page, NavigationService navigationService) => null;

        #endregion

        /// <summary>
        /// OnStartAsync is the one-stop-show override to handle when your app starts
        /// OnStartAsync will not be called if the app is restored from state.
        /// An app restores from state when the app was suspended and then terminated (PreviousExecutionState terminated).
        /// </summary>
        public abstract void OnStart(StartKind startKind, IActivatedEventArgs args);

        //protected override void OnWindowCreated(Microsoft.UI.Xaml.WindowCreatedEventArgs args)
        //{
        //    var window = new WindowWrapper(args.Window);
        //    window.Window.VisibilityChanged += Window_VisibilityChanged;
        //    window.Window.Activated += Window_Activated;
        //    base.OnWindowCreated(args);
        //}

        private void Window_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs e)
        {
            _isActive = e.WindowActivationState != WindowActivationState.Deactivated;
        }

        private void Window_VisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            _isMinimized = !e.Visible;
        }

        /// <summary>
        /// OnInitializeAsync is where your app will do must-have up-front operations
        /// OnInitializeAsync will be called even if the application is restoring from state.
        /// An app restores from state when the app was suspended and then terminated (PreviousExecutionState terminated).
        /// </summary>
        public virtual void OnInitialize(IActivatedEventArgs args)
        {
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs e)
        {
            InternalLaunch(e.UWPLaunchActivatedEventArgs);
        }

        //protected override void OnActivated(IActivatedEventArgs e)
        //{
        //    base.OnActivated(e);

        //    InternalActivatedAsync(e);
        //}

        public virtual void OnUnhandledException(Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {

        }

        public virtual void OnSuspending(SuspendingEventArgs e)
        {

        }

        /// <summary>
        /// This handles all the prelimimary stuff unique to Activated before calling OnStartAsync()
        /// This is private because it is a specialized prelude to OnStartAsync().
        /// OnStartAsync will not be called if state restore is determined.
        /// </summary>
        protected void InternalActivatedAsync(IActivatedEventArgs e)
        {
            // sometimes activate requires a frame to be built
            if (Window.Current.Content == null)
            {
                InitializeFrame(e);
            }

            //adding a default navigation service
            if (!NavigationServices.IsRegistered("Default"))
                NavigationServices.Register("Default", WindowWrapper.Current().NavigationService);

            // onstart is shared with activate and launch
            OnStart(StartKind.Activate, e);

            // ensure active (this will hide any custom splashscreen)
            Window.Current.Activate();
        }

        protected void InternalLaunch(ILaunchActivatedEventArgs e)
        {
            if (e.PreviousExecutionState != ApplicationExecutionState.Running)
            {
                InitializeFrame(e);
            }

            //adding a default navigation service
            if (!NavigationServices.IsRegistered("Default"))
                NavigationServices.Register("Default", WindowWrapper.Current().NavigationService);

            // okay, now handle launch
            switch (e.PreviousExecutionState)
            {
                case ApplicationExecutionState.Terminated:
                    {
                        OnStart(StartKind.Launch, e);

                        SubscribeBackButton();

                        break;
                    }
                case ApplicationExecutionState.ClosedByUser:
                case ApplicationExecutionState.NotRunning:
                    // launch if not restored
                    OnStart(StartKind.Launch, e);

                    SubscribeBackButton();

                    break;

                case ApplicationExecutionState.Running:
                    OnStart(StartKind.Activate, e);
                    break;

                default:
                    {
                        // launch if not restored
                        OnStart(StartKind.Launch, e);
                        break;
                    }
            }
        }

        protected void InitializeFrame(IActivatedEventArgs e)
        {
            // allow the user to do things, even when restoring
            OnInitialize(e);

            // create the default frame only if there's nothing already there
            // if it is not null, by the way, then the developer injected something & they win
            if (Window.Current?.Content == null)
            {
                // build the default frame
                var frame = CreateRootFrame(e);
                var navigationService = new NavigationService(frame);
                navigationService.IsBackButtonEnabled = IsBackButtonEnabled;
                if (Window.Current != null)
                {
                    Window.Current.Content = navigationService.Frame;
                }
                WindowWrapper.Current().NavigationService = navigationService;
            }

            Window.Current?.Activate();
        }

        protected virtual Frame CreateRootFrame(IActivatedEventArgs e)
        {
            return new Frame();
        }

        protected void SubscribeBackButton()
        {
            // Hook up the default Back handler
            //SystemNavigationManager.GetForCurrentView().BackRequested += (s, args) =>
            //{
            //    var handled = false;
            //    if (NavigationService.CanGoBack)
            //    {
            //        NavigationService.GoBack();
            //        handled = true;
            //    }

            //    args.Handled = handled;
            //};
        }

        private void BootStrapper_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            OnUnhandledException(e);
        }

        private void BootStrapper_Suspending(object sender, SuspendingEventArgs e)
        {
            OnSuspending(e);
        }
    }
}