using System.Collections.Generic;
using System.Threading.Tasks;
using Meridian.Model;

namespace Meridian.Services.Music.Repositories
{
    public class LocalTracksRepository
    {
        public async Task<List<LocalAudio>> GetTracks()
        {
            return await ServiceLocator.DataBaseService.GetItems<LocalAudio>();
        }
    }
}
