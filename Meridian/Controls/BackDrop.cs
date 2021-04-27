using System;
using System.Numerics;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.UI;

namespace Meridian.Controls
{
    public class BackDrop : Control
    {
        private Compositor _compositor;
        private Visual _rootVisual;

        private SpriteVisual _blurVisual;
        private CompositionEffectBrush _blurBrush;

        // Using a DependencyProperty as the backing store for BlurAmount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BlurAmountProperty =
            DependencyProperty.Register("BlurAmount", typeof(double), typeof(BackDrop), new PropertyMetadata(default(double), (d, e) =>
            {
                var control = (BackDrop)d;
                if (e.NewValue != null && control._blurBrush != null)
                {
                    var amount = Convert.ToSingle(e.NewValue);
                    control._blurBrush.Properties.InsertScalar("Blur.BlurAmount", amount);
                }
            }));

        public double BlurAmount
        {
            get { return (double)GetValue(BlurAmountProperty); }
            set { SetValue(BlurAmountProperty, value); }
        }


        // Using a DependencyProperty as the backing store for UseHostBackDrop.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UseHostBackDropProperty =
            DependencyProperty.Register("UseHostBackDrop", typeof(bool), typeof(BackDrop), new PropertyMetadata(false, (d, e) =>
            {
                //if (!ApiInformation.IsMethodPresent(typeof(Compositor).FullName, nameof(Compositor.CreateHostBackdropBrush)))
                //    return;

                //var control = (BackDrop)d;
                //if (e.NewValue != null && control._blurBrush != null)
                //{
                //    var value = Convert.ToBoolean(e.NewValue);
                //    if (value)
                //        control._blurBrush.SetSourceParameter("Source", control._compositor.CreateHostBackdropBrush());
                //    else
                //        control._blurBrush.SetSourceParameter("Source", control._compositor.CreateBackdropBrush());
                //}
            }));


        public bool UseHostBackDrop
        {
            get { return (bool)GetValue(UseHostBackDropProperty); }
            set { SetValue(UseHostBackDropProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TintColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TintColorProperty =
            DependencyProperty.Register("TintColor", typeof(Color), typeof(BackDrop), new PropertyMetadata(Colors.Transparent, (d, e) =>
            {
                var control = (BackDrop)d;
                if (e.NewValue != null && control._blurBrush != null)
                {
                    var color = (Color)e.NewValue;
                    control._blurBrush.Properties.InsertColor("Color.Color", color);
                }
            }));

        public Color TintColor
        {
            get { return (Color)GetValue(TintColorProperty); }
            set { SetValue(TintColorProperty, value); }
        }


        public BackDrop()
        {
            //if (DesignMode.DesignModeEnabled)
            //    return;

            //_rootVisual = ElementCompositionPreview.GetElementVisual(this);
            //_compositor = _rootVisual.Compositor;

            //_blurVisual = _compositor.CreateSpriteVisual();

            //_blurBrush = CreateBlurBrush();
            //_blurBrush.SetSourceParameter("Source", _compositor.CreateBackdropBrush());
            //_blurVisual.Brush = _blurBrush;

            //ElementCompositionPreview.SetElementChildVisual(this, _blurVisual);

            //this.Loading += ControlLoading;
            //this.Unloaded += ControlUnloaded;
        }

        private void ControlLoading(FrameworkElement sender, object args)
        {
            this.SizeChanged += ControlSizeChanged;
        }

        private void ControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_blurVisual != null)
                _blurVisual.Size = new Vector2((float)this.ActualWidth, (float)this.ActualHeight);
        }

        private void ControlUnloaded(object sender, RoutedEventArgs e)
        {
            this.Loading -= ControlLoading;
            this.SizeChanged -= ControlSizeChanged;
            this.Unloaded -= ControlUnloaded;
        }

        //private CompositionEffectBrush CreateBlurBrush()
        //{
        //    var blurEffect = new GaussianBlurEffect
        //    {
        //        Name = "Blur",
        //        BorderMode = EffectBorderMode.Hard,
        //        Optimization = EffectOptimization.Balanced,
        //        Source = new CompositionEffectSourceParameter("Source"),
        //        BlurAmount = 0
        //    };

        //    var blendEffect = new BlendEffect
        //    {
        //        Background = blurEffect,
        //        Foreground = new ColorSourceEffect { Name = "Color", Color = Colors.Transparent },
        //        Mode = BlendEffectMode.LighterColor
        //    };

        //    var saturationEffect = new SaturationEffect
        //    {
        //        Source = blendEffect,
        //        Saturation = 1.75f
        //    };

        //    var effectFactory = _compositor.CreateEffectFactory(saturationEffect, new[] { "Blur.BlurAmount", "Color.Color" });
        //    var effectBrush = effectFactory.CreateBrush();

        //    return effectBrush;
        //}
    }
}