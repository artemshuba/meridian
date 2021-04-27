using Meridian.Services;
using System;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Meridian.View.Controls
{
    public sealed partial class PlayerControl : UserControl
    {
        public PlayerControl()
        {
            this.InitializeComponent();

            SetupComposition();
        }

        private void VolumeGridPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var mousePosition = e.GetCurrentPoint((UIElement)sender);
            var delta = mousePosition.Properties.MouseWheelDelta / 60f;

            var volume = (double)VolumeSlider.GetValue(Slider.ValueProperty);
            VolumeSlider.SetValue(Slider.ValueProperty, volume + delta);
        }

        private void VolumeMuteButtonClick(object sender, RoutedEventArgs e)
        {
            VolumeSlider.SetValue(Slider.ValueProperty, 0);
        }

        private void VolumeMaxButtonClick(object sender, RoutedEventArgs e)
        {
            var maxValue = (double)VolumeSlider.GetValue(Slider.MaximumProperty);
            VolumeSlider.SetValue(Slider.ValueProperty, maxValue);
        }

        #region Composition stuff

        private void SetupComposition()
        {
            var compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            // Create ImplicitAnimations Collection. 
            var elementImplicitAnimation = compositor.CreateImplicitAnimationCollection();

            //Define trigger and animation that should play when the trigger is triggered. 
            elementImplicitAnimation[nameof(Visual.Offset)] = CreateLayoutAnimation(compositor);

            foreach (var child in RootPanel.Children)
            {
                var elementVisual = ElementCompositionPreview.GetElementVisual(child);
                elementVisual.ImplicitAnimations = elementImplicitAnimation;
            }
        }

        private CompositionAnimationGroup CreateLayoutAnimation(Compositor compositor)
        {
            var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromSeconds(.2);

            //Define Animation Target for this animation to animate using definition. 
            offsetAnimation.Target = nameof(Visual.Offset);

            //Add Animations to Animation group. 
            CompositionAnimationGroup animationGroup = compositor.CreateAnimationGroup();
            animationGroup.Add(offsetAnimation);

            return animationGroup;
        }

        #endregion

        private void PlayerControlLoaded(object sender, RoutedEventArgs e)
        {
            AudioService.Instance.PlayStateChanged += Instance_PlayStateChanged;
        }
        
        private void PlayerControlUnloaded(object sender, RoutedEventArgs e)
        {
            AudioService.Instance.PlayStateChanged -= Instance_PlayStateChanged;
        }

        private void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            var audioService = (AudioService)sender;
            Storyboard anim;
            if (audioService.IsPlaying)
                anim = (Storyboard)Resources["PlayStartedAnim"];
            else
                anim = (Storyboard)Resources["PlayStoppedAnim"];

            anim.Begin();
        }

        private void PlayerControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            AlbumArtGrid.Height = e.NewSize.Width;
        }
    }
}