namespace VkLib.Core.Users
{
    /// <summary>
    /// Base class for user or group profile
    /// <seealso cref="http://vk.com/dev/fields"/>
    /// </summary>
    public class VkProfileBase
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Full name
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// 50x50px width photo
        /// </summary>
        public string Photo { get; set; }

        /// <summary>
        /// 100x100px width photo
        /// </summary>
        public string PhotoMedium { get; set; }

        /// <summary>
        /// 200px width photo
        /// </summary>
        public string PhotoBig { get; set; }

        /// <summary>
        /// 200x200px photo
        /// </summary>
        public string PhotoBigSquare { get; set; }

        /// <summary>
        /// 400px width photo
        /// </summary>
        public string PhotoLarge { get; set; }

        /// <summary>
        /// Max width photo. Can have 400px or 200px width
        /// </summary>
        public string PhotoMax { get; set; }

        /// <summary>
        /// Max size square photo. Can be 200x200 or 100x100.
        /// </summary>
        public string PhotoMaxSquare { get; set; }
    }
}
