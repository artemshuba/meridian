using Meridian.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Meridian.Converters.TemplateSelectors
{
    public class CatalogBlockTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SpecialBlockTemplate { get; set; }

        public DataTemplate RecentBlockTemplate { get; set; }

        public DataTemplate NewBlockTemplate { get; set; }

        public DataTemplate PlaylistsBlockTemplate { get; set; }

        public DataTemplate PopularBlockTemplate { get; set; }

        public DataTemplate NewAlbumsBlockTemplate { get; set; }

        public DataTemplate SimilarToBlockTemplate { get; set; }

        public DataTemplate FriendsBlockTemplate { get; set; }

        public DataTemplate SocietiesBlockTemplate { get; set; }

        public DataTemplate NewArtistsBlockTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var block = item as CatalogBlock;
            if (block == null)
                return null;

            switch (block.Source)
            {
                case "recoms_recoms":
                    return SpecialBlockTemplate;

                case "recoms_playlists":
                    return PlaylistsBlockTemplate;

                case "recoms_recent_audios":
                    return RecentBlockTemplate;

                case "recoms_new_audios":
                    return NewBlockTemplate;

                case "recoms_top_audios_global":
                    return PopularBlockTemplate;

                case "recoms_new_albums":
                    return NewAlbumsBlockTemplate;

                case "recoms_recent_recommendation":
                    return SimilarToBlockTemplate;

                case "recoms_friends":
                    return FriendsBlockTemplate;

                case "recoms_communities":
                    return SocietiesBlockTemplate;

                case "recoms_new_artists":
                    return NewArtistsBlockTemplate;

                default:
                    return new DataTemplate();
            }
        }
    }
}