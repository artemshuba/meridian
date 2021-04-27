using GalaSoft.MvvmLight.Messaging;
using Meridian.Utils.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VkLib.Core.Friends;
using VkLib.Core.Groups;
using VkLib.Core.Users;
using VkLib.Error;

namespace Meridian.Services.VK
{
    public class VkUserService
    {
        private readonly VkLib.Vk _vk;

        public VkUserService(VkLib.Vk vk)
        {
            _vk = vk;
        }

        public async Task<VkProfile> GetUser(long userId = 0)
        {
            try
            {
                var result = await _vk.Users.Get(userId != 0 ? userId : _vk.AccessToken.UserId, fields: "photo,photo_100,photo_400_orig");

                return result;
            }
            catch (VkInvalidTokenException)
            {
                Messenger.Default.Send(new MessageUserAuthChanged { IsLoggedIn = false });
            }

            return null;
        }

        public async Task<(List<VkProfile> Friends, int TotalCount)> GetFriends(int count = 0, int offset = 0, long userId = 0)
        {
            (List<VkProfile> Friends, int TotalCount) result = (null, 0);

            var response = await _vk.Friends.Get(userId, "photo,photo_100,photo_400_orig", null, count, offset, FriendsOrder.ByRating);
            if (response.Items != null)
            {
                result = (response.Items, response.TotalCount);
            }

            return result;
        }

        public async Task<(List<VkGroup> Societies, int TotalCount)> GetSocieties(int count = 0, int offset = 0, long userId = 0)
        {
            (List<VkGroup> Societies, int TotalCount) result = (null, 0);

            var response = await _vk.Groups.Get(userId, "photo,photo_100,photo_400_orig", null, count, offset);
            if (response.Items != null)
            {
                result = (response.Items, response.TotalCount);
            }

            return result;
        }
    }
}
