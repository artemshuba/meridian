namespace Meridian.Model
{
    public class ColorScheme
    {
        public string Name { get; set; }

        public string Color { get; set; }

        public ColorScheme(string name, string color)
        {
            Name = name;
            Color = color;
        }
    }
}
