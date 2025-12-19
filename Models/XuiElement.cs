using System.Collections.Generic;

namespace XuiEditor.Models
{
    public class XuiElement
    {
        public string Name { get; set; }

        public string Type { get; set; } = "rect";

        public double X { get; set; }
        public double Y { get; set; }

        public double Width { get; set; }
        public double Height { get; set; }

        // XUI-spezifisch (später wichtig)
        public string Texture { get; set; }
        public string Text { get; set; }

        // Für Modded-UI / extra Attribute
        public Dictionary<string, string> CustomAttributes { get; set; }
            = new Dictionary<string, string>();
    }
}