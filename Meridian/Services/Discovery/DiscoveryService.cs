using DeezerLib;
using DeezerLib.Data;
using Meridian.Model.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkLib;

namespace Meridian.Services.Discovery
{
    public class DiscoveryService
    {
        private Vk _vk;
        private Deezer _deezer;

        public DiscoveryService(Vk vk, Deezer deezer)
        {
            _vk = vk;
            _deezer = deezer;
        }

        public async Task<List<DiscoveryArtist>> SearchArtists(string query)
        {
            var artists = await _deezer.SearchArtists(query);
            if (artists != null)
            {
                return artists.Select(ToDiscoveryArtist).ToList();
            }

            return null;
        }

        public async Task<List<DiscoveryAlbum>> SearchAlbums(string query)
        {
            var artists = await _deezer.SearchAlbums(query);
            if (artists != null)
            {
                return artists.Select(ToDiscoveryAlbum).ToList();
            }

            return null;
        }

        public async Task<List<DiscoveryTrack>> GetAlbumTracks(string albumId)
        {
            var tracks = await _deezer.GetAlbumTracks(albumId);
            if (tracks != null)
            {
                return tracks.Select(ToDiscoveryTrack).ToList();
            }

            return null;
        }

        public async Task<List<DiscoveryTrack>> GetArtistTopTracks(string artistId, int count = 0, int offset = 0)
        {
            var tracks = await _deezer.GetArtistTopTracks(artistId, limit: count, index: offset);
            if (tracks != null)
            {
                return tracks.Select(ToDiscoveryTrack).ToList();
            }

            return null;
        }

        public async Task<List<DiscoveryAlbum>> GetArtistAlbums(string artistId)
        {
            var albums = await _deezer.GetArtistAlbums(artistId);
            if (albums != null)
            {
                return albums.Select(ToDiscoveryAlbum).ToList();
            }

            return null;
        }

        public async Task<List<DiscoveryArtist>> GetArtistRelated(string artistId)
        {
            var artists = await _deezer.GetArtistRelated(artistId);
            if (artists != null)
            {
                return artists.Select(ToDiscoveryArtist).ToList();
            }

            return null;
        }

        private DiscoveryArtist ToDiscoveryArtist(DeezerArtist artist)
        {
            var discoveryArtist = new DiscoveryArtist();

            discoveryArtist.Id = artist.Id;
            discoveryArtist.Name = artist.Name;

            if (!string.IsNullOrEmpty(artist.PictureMedium))
                discoveryArtist.Image = new Uri(artist.PictureMedium);

            if (!string.IsNullOrEmpty(artist.PictureXl))
                discoveryArtist.ImageLarge = new Uri(artist.PictureXl);

            return discoveryArtist;
        }

        private DiscoveryAlbum ToDiscoveryAlbum(DeezerAlbum album)
        {
            var discoveryAlbum = new DiscoveryAlbum();

            discoveryAlbum.Id = album.Id;
            discoveryAlbum.Title = album.Title;
            if (album.Artist != null)
                discoveryAlbum.Artist = ToDiscoveryArtist(album.Artist);
            discoveryAlbum.ReleaseDate = album.ReleaseDate;
            discoveryAlbum.TracksCount = album.NumberOfTracks;

            if (!string.IsNullOrEmpty(album.CoverMedium))
                discoveryAlbum.Cover = new Uri(album.CoverMedium);

            if (!string.IsNullOrEmpty(album.CoverXl))
                discoveryAlbum.CoverLarge = new Uri(album.CoverXl);

            return discoveryAlbum;
        }

        private DiscoveryTrack ToDiscoveryTrack(DeezerTrack track)
        {
            var discoveryTrack = new DiscoveryTrack();

            discoveryTrack.Id = track.Id;
            discoveryTrack.Title = track.Title;
            discoveryTrack.Artist = track.Artist?.Name;
            discoveryTrack.Duration = TimeSpan.FromSeconds(track.Duration);

            return discoveryTrack;
        }
    }
}
