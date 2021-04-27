using Meridian.Interfaces;
//using Microsoft.Toolkit.Uwp.Notifications;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Meridian.Services
{
    public static class TileHelper
    {
        private static string _lastTileXml;

        public static void UpdateIsPlaying(bool isPlaying)
        {
            //if (string.IsNullOrEmpty(_lastTileXml))
            //    return;

            //if (isPlaying)
            //    _lastTileXml = _lastTileXml.Replace("play-icon", "pause-icon");
            //else
            //    _lastTileXml = _lastTileXml.Replace("pause-icon", "play-icon");

            //var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            //var xml = new XmlDocument();
            //xml.LoadXml(_lastTileXml);

            //updater.Update(new TileNotification(xml));
        }

        public static async Task UpdateMainTile(IAudio currentTrack, bool isPlaying = false)
        {
            //var imageService = Ioc.Resolve<ImageService>();
            //var imageUri = await imageService.GetTileImageUri(currentTrack.Artist, currentTrack.Title);

            //var tileLargeackground = new TileBackgroundImage
            //{
            //    Source = imageUri?.OriginalString ?? "ms-appx:///Assets/LargeTile.scale-400.png",
            //    HintOverlay = 40
            //};

            //var tileBindingLarge = new TileBinding
            //{
            //    Branding = TileBranding.None,
            //    Content = new TileBindingContentAdaptive
            //    {
            //        BackgroundImage = tileLargeackground,

            //        TextStacking = TileTextStacking.Bottom,

            //        Children = {
            //            new AdaptiveGroup()
            //            {
            //                Children = {
            //                    new AdaptiveSubgroup()
            //                    {
            //                        Children =
            //                        {
            //                            new AdaptiveText()
            //                            {
            //                                Text = currentTrack.Artist,
            //                                HintStyle = AdaptiveTextStyle.Caption
            //                            },
            //                            new AdaptiveText()
            //                            {
            //                                Text = currentTrack.Title,
            //                                HintStyle = AdaptiveTextStyle.Base
            //                            }
            //                        }
            //                    },
            //                    new AdaptiveSubgroup()
            //                    {
            //                        HintWeight = 3,
            //                        Children =
            //                        {
            //                            new AdaptiveText(),
            //                            new AdaptiveImage
            //                            {
            //                                Source = string.Format("ms-appx:///Resources/Images/Player/{0}-icon.png", isPlaying ? "pause" : "play")
            //                            }
            //                        }
            //                    },
            //                }
            //            }
            //        }
            //    }
            //};


            //var tileWideBackground = new TileBackgroundImage
            //{
            //    Source = imageUri?.OriginalString ?? "ms-appx:///Assets/Wide310x150Logo.scale-400.png",
            //    HintOverlay = 40
            //};

            //var tileBindingWide = new TileBinding
            //{
            //    Branding = TileBranding.None,
            //    Content = new TileBindingContentAdaptive
            //    {
            //        BackgroundImage = tileWideBackground,

            //        TextStacking = TileTextStacking.Bottom,

            //        Children = {
            //            new AdaptiveGroup()
            //            {
            //                Children = {
            //                    new AdaptiveSubgroup()
            //                    {
            //                        Children =
            //                        {
            //                            new AdaptiveText()
            //                            {
            //                                Text = currentTrack.Artist,
            //                                HintStyle = AdaptiveTextStyle.Caption
            //                            },
            //                            new AdaptiveText()
            //                            {
            //                                Text = currentTrack.Title,
            //                                HintStyle = AdaptiveTextStyle.Base
            //                            }
            //                        }
            //                    },
            //                    new AdaptiveSubgroup()
            //                    {
            //                        HintWeight = 3,
            //                        Children =
            //                        {
            //                            new AdaptiveText(),
            //                            new AdaptiveImage
            //                            {
            //                                Source = string.Format("ms-appx:///Resources/Images/Player/{0}-icon.png", isPlaying ? "pause" : "play")
            //                            }
            //                        }
            //                    },
            //                }
            //            }
            //        }
            //    }
            //};

            //var tileMediumBackground = new TileBackgroundImage
            //{
            //    Source = imageUri?.OriginalString ?? "ms-appx:///Assets/Square150x150Logo.scale-400.png",
            //    HintOverlay = 60
            //};

            //var tileBindingMedium = new TileBinding
            //{
            //    Branding = TileBranding.None,
            //    Content = new TileBindingContentAdaptive
            //    {
            //        BackgroundImage = tileMediumBackground,

            //        TextStacking = TileTextStacking.Bottom,

            //        Children = {
            //            new AdaptiveText()
            //            {
            //                Text = currentTrack.Artist,
            //                HintStyle = AdaptiveTextStyle.CaptionSubtle
            //            },
            //            new AdaptiveText()
            //            {
            //                Text = currentTrack.Title,
            //                HintStyle = AdaptiveTextStyle.Body,
            //                HintMaxLines = 2,
            //                HintWrap = true
            //            }
            //        }
            //    }
            //};

            //var tileSmallBackground = new TileBackgroundImage
            //{
            //    Source = imageUri?.OriginalString ?? "ms-appx:///Assets/SmallTile.scale-400.png",
            //    HintOverlay = 40
            //};

            //var tileBindingSmall = new TileBinding
            //{
            //    Branding = TileBranding.Logo,
            //    Content = new TileBindingContentAdaptive
            //    {
            //        BackgroundImage = tileSmallBackground
            //    }
            //};

            //var tileContent = new TileContent()
            //{
            //    Visual = new TileVisual()
            //    {
            //        TileLarge = tileBindingLarge,
            //        TileMedium = tileBindingMedium,
            //        TileWide = tileBindingWide,
            //        TileSmall = tileBindingSmall
            //    }
            //};

            //var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            //var xml = tileContent.GetXml();
            //_lastTileXml = xml.GetXml();
            //updater.Update(new TileNotification(xml));
        }
    }
}