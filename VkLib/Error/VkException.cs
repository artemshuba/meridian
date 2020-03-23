using System;

namespace VkLib.Error
{
    public class VkException : Exception
    {
        /// <summary>
        /// Error
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        public VkException()
        {
            
        }

        public VkException(string error, string description)
        {
            Error = error;
            Description = description;
        }
    }
}
