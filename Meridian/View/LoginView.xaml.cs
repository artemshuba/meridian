using Meridian.ViewModel;
using Windows.System;
using Microsoft.UI.Composition;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Hosting;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Meridian.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginView : Page
    {
        public LoginViewModel ViewModel => (LoginViewModel)DataContext;

        public LoginView()
        {
            this.InitializeComponent();

            //SetupComposition(MainGrid);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;

            base.OnNavigatingFrom(e);
        }

        private void CaptchaBox_OnKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            //if (e.Key == VirtualKey.Enter)
            //{
            //    if (!string.IsNullOrWhiteSpace(CaptchaBox.Text))
            //    {
            //        if (string.IsNullOrWhiteSpace(LoginBox.Text))
            //        {
            //            LoginBox.Focus(FocusState.Keyboard);
            //        }
            //        else if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            //        {
            //            PasswordBox.Focus(FocusState.Keyboard);
            //        }
            //        else
            //        {
            //            ViewModel.LoginCommand.Execute(null);
            //        }
            //    }
            //}
        }

        private void PasswordBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            //if (e.Key == VirtualKey.Enter)
            //{
            //    //for unknown reason calls twice (on Enter)
            //    if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
            //    {
            //        if (string.IsNullOrWhiteSpace(LoginBox.Text))
            //        {
            //            LoginBox.Focus(FocusState.Keyboard);
            //        }
            //        else if (CaptchaForm.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(CaptchaBox.Text))
            //        {
            //            CaptchaBox.Focus(FocusState.Keyboard);
            //        }
            //        else
            //        {
            //            ViewModel.LoginCommand.Execute(null);
            //        }
            //    }
            //}
        }

        private void LoginBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            //if (e.Key == VirtualKey.Enter)
            //{
            //    if (!string.IsNullOrWhiteSpace(LoginBox.Text))
            //    {
            //        if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            //        {
            //            PasswordBox.Focus(FocusState.Keyboard);
            //        }
            //        else if (CaptchaForm.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(CaptchaBox.Text))
            //        {
            //            CaptchaBox.Focus(FocusState.Keyboard);
            //        }
            //        else
            //        {
            //            ViewModel.LoginCommand.Execute(null);
            //        }
            //    }
            //}
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsLoginFormVisible":
                    if (ViewModel.IsLoginFormVisible)
                        ((Storyboard)Resources["ShowLoginAnim"]).Begin();
                    else
                        ((Storyboard)Resources["HideLoginAnim"]).Begin();
                    break;
            }
        }

        private void ImageBrush_ImageOpened(object sender, RoutedEventArgs e)
        {
            ((Storyboard)Resources["ShowUserPhotoAnim"]).Begin();
        }

        #region Composition stuff

        private void SetupComposition(Panel rootPanel)
        {
            var compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            // Create ImplicitAnimations Collection. 
            var elementImplicitAnimation = compositor.CreateImplicitAnimationCollection();

            //Define trigger and animation that should play when the trigger is triggered. 
            elementImplicitAnimation[nameof(Visual.Offset)] = CreateLayoutAnimation(compositor);

            foreach (var child in rootPanel.Children)
            {
                var elementVisual = ElementCompositionPreview.GetElementVisual(child);
                elementVisual.ImplicitAnimations = elementImplicitAnimation;
            }
        }

        private CompositionAnimationGroup CreateLayoutAnimation(Compositor compositor)
        {
            var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromSeconds(0.5);

            //Define Animation Target for this animation to animate using definition. 
            offsetAnimation.Target = nameof(Visual.Offset);

            //Add Animations to Animation group. 
            CompositionAnimationGroup animationGroup = compositor.CreateAnimationGroup();
            animationGroup.Add(offsetAnimation);

            return animationGroup;
        }

        #endregion
    }
}