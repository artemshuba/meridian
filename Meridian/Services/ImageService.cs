using Meridian.Interfaces;
using Meridian.Model;
using Meridian.Services.Images;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Meridian.Services
{
    public class ImageService
    {
        private IImageResolver _lastFmImageResolver;

        private CacheService _cacheService;

        public static CachedImage DefaultTrackCover = new CachedImage { Key = "default", Source = new BitmapImage(new Uri("ms-appx:///Resources/Images/Player/DefaultCover-Light.png")) };

        public ImageService()
        {
            _lastFmImageResolver = new LastFmImageResolver();

            _cacheService = Ioc.Resolve<CacheService>();
        }

        public async Task<CachedImage> GetTrackImage(IAudio track, int optimalImageWidth = 0)
        {
            try
            {
                var artist = track.Artist;
                var title = track.Title;
                var imageKey = $"{artist}_{title}";
                var cachedImage = await _cacheService.GetCachedImage(key: imageKey);
                if (cachedImage != null)
                    return cachedImage;

                if (track.AlbumCover != null)
                    cachedImage = await _cacheService.CacheImageFromUri(track.AlbumCover, imageKey, optimalImageWidth);

                if (cachedImage != null)
                    return cachedImage;

                var imageUri = await ResolveAlbumCoverUri(artist, title);
                if (imageUri != null)
                    cachedImage = await _cacheService.CacheImageFromUri(imageUri, key: imageKey, optimalImageWidth: optimalImageWidth);
                else
                {
                    cachedImage = await GetArtistImage(artist, big: false, optimalImageWidth: optimalImageWidth);

                    if (cachedImage == null)
                        return DefaultTrackCover; //TODO dark theme support
                }

                return cachedImage;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to get track image");
            }

            return null;
        }

        public async Task<CachedImage> GetArtistImage(string artist, bool big = true, int optimalImageWidth = 0)
        {
            try
            {
                var imageKey = $"{artist}";
                var cachedImage = await _cacheService.GetCachedImage(key: imageKey, optimalImageWidth: optimalImageWidth);
                if (cachedImage == null)
                {
                    var imageUri = await ResolveArtistImageUri(artist, big: big);
                    if (imageUri != null)
                        cachedImage = await _cacheService.CacheImageFromUri(imageUri, key: imageKey, optimalImageWidth: optimalImageWidth);
                }

                return cachedImage;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to get artist image");
            }

            return null;
        }

        public async Task<Uri> GetTileImageUri(string artist, string title)
        {
            try
            {
                var imageKey = $"{artist}_{title}";
                var tileImageKey = $"tile";

                var imageStream = await _cacheService.GetCachedImageStream(key: imageKey);

                if (imageStream == null)
                {
                    imageKey = $"{artist}";

                    imageStream = await _cacheService.GetCachedImageStream(key: imageKey);
                }

                if (imageStream != null)
                {
                    using (imageStream)
                    {
                        using (var resizeImageStream = await ResizeImage(imageStream, 300, 300))
                        {
                            if (resizeImageStream != null)
                            {
                                await _cacheService.SaveImageStream(tileImageKey, resizeImageStream);

                                return _cacheService.GetCachedImageUri(tileImageKey);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to get tile image uri");
            }

            return null;
        }

        public async Task UpdateTransportControlsImage(string artist, string title)
        {
            try
            {
                var imageKey = $"{artist}_{title}";
                var coverImageKey = $"cover";

                var imageStream = await _cacheService.GetCachedImageStream(key: imageKey);

                if (imageStream == null)
                {
                    imageKey = $"{artist}";

                    imageStream = await _cacheService.GetCachedImageStream(key: imageKey);
                }

                if (imageStream != null)
                {
                    using (imageStream)
                    {
                        var resizeImageStream = await ResizeImageWithCrop(imageStream, 150, 150);

                        AudioService.Instance.UpdateCover(RandomAccessStreamReference.CreateFromStream(resizeImageStream.AsRandomAccessStream()));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to update transport image");
            }
        }

        private async Task<Uri> ResolveAlbumCoverUri(string artist, string title)
        {
            var uri = await _lastFmImageResolver.GetAlbumCover(artist, title);

            return uri;
        }

        private async Task<Uri> ResolveArtistImageUri(string artist, bool big = true)
        {
            var uri = await _lastFmImageResolver.GetArtistImageUri(artist, big);

            return uri;
        }

        private async Task<Stream> ResizeImage(Stream inputStream, int width, int height)
        {
            var imageStream = inputStream.AsRandomAccessStream();
            var decoder = await BitmapDecoder.CreateAsync(imageStream);
            if (decoder.PixelHeight > height || decoder.PixelWidth > width)
            {
                var resizedStream = new InMemoryRandomAccessStream();

                var encoder = await BitmapEncoder.CreateForTranscodingAsync(resizedStream, decoder);

                double widthRatio = (double)width / decoder.PixelWidth;
                double heightRatio = (double)height / decoder.PixelHeight;

                double scaleRatio = Math.Min(widthRatio, heightRatio);

                if (width == 0)
                    scaleRatio = heightRatio;

                if (height == 0)
                    scaleRatio = widthRatio;

                var scaledHeight = Math.Floor(decoder.PixelHeight * scaleRatio);
                var scaledWidth = Math.Floor(decoder.PixelWidth * scaleRatio);

                encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;
                encoder.BitmapTransform.ScaledHeight = (uint)scaledHeight;
                encoder.BitmapTransform.ScaledWidth = (uint)scaledWidth;

                await encoder.FlushAsync();

                var outBuffer = new byte[resizedStream.Size];
                await resizedStream.ReadAsync(outBuffer.AsBuffer(), (uint)resizedStream.Size, InputStreamOptions.None);

                resizedStream.Seek(0);

                return resizedStream.AsStream();
            }
            else
            {
                await inputStream.CopyToAsync(imageStream.AsStreamForWrite());
                imageStream.Seek(0);
                return imageStream.AsStream();
            }
        }

        private async Task<Stream> ResizeImageWithCrop(Stream inputStream, int width, int height)
        {
            var imageStream = inputStream.AsRandomAccessStream();
            var decoder = await BitmapDecoder.CreateAsync(imageStream);
            var resizedStream = new InMemoryRandomAccessStream();

            var encoder = await BitmapEncoder.CreateForTranscodingAsync(resizedStream, decoder);

            double widthRatio = (double)width / decoder.PixelWidth;
            double heightRatio = (double)height / decoder.PixelHeight;

            double scaleRatio = Math.Max(widthRatio, heightRatio);

            if (width == 0)
                scaleRatio = heightRatio;

            if (height == 0)
                scaleRatio = widthRatio;

            var scaledHeight = Math.Floor(decoder.PixelHeight * scaleRatio);
            var scaledWidth = Math.Floor(decoder.PixelWidth * scaleRatio);

            encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;
            encoder.BitmapTransform.ScaledHeight = (uint)scaledHeight;
            encoder.BitmapTransform.ScaledWidth = (uint)scaledWidth;

            encoder.BitmapTransform.Bounds = new BitmapBounds() { X = (uint)((scaledWidth - width) / 2), Y = (uint)((scaledHeight - height) / 2), Height = (uint)height, Width = (uint)width };

            await encoder.FlushAsync();

            resizedStream.Seek(0);

            return resizedStream.AsStream();
        }
    }
}