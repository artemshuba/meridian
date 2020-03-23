namespace VkLib.Core.Attachments
{
    public class VkLinkAttachment : VkAttachment
    {
        public string Url { get; set; }

        public override string ToString()
        {
            return Url;
        }
    }
}
