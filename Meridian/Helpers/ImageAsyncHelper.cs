using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Meridian.Helpers
{
    public class NullImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(((Uri)value).OriginalString))
                return DependencyProperty.UnsetValue;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ImageAsyncHelper : DependencyObject
    {
        Uri _givenUri;

        private static NullImageConverter _converter;

        private static NullImageConverter Converter
        {
            get
            {
                if (_converter == null)
                    _converter = new NullImageConverter();

                return _converter;
            }
        }

        public static Uri GetSourceUri(DependencyObject obj) { return (Uri)obj.GetValue(SourceUriProperty); }
        public static void SetSourceUri(DependencyObject obj, Uri value) { obj.SetValue(SourceUriProperty, value); }
        public static readonly DependencyProperty SourceUriProperty = DependencyProperty.RegisterAttached("SourceUri", typeof(Uri), typeof(ImageAsyncHelper), new PropertyMetadata(SourceUriChanged));

        private static void SourceUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Image)d).SetBinding(Image.SourceProperty,
                new Binding("VerifiedUri")
                {
                    Source = new ImageAsyncHelper { _givenUri = e.NewValue as Uri },
                    IsAsync = true,
                    Converter = Converter
                });
        }

        public Uri VerifiedUri
        {
            get
            {
                try
                {
                    if (_givenUri != null && !string.IsNullOrEmpty(_givenUri.OriginalString))
                    {
                        Dns.GetHostEntry(_givenUri.DnsSafeHost);
                        return _givenUri;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                return _givenUri;
            }
        }
    }
}
