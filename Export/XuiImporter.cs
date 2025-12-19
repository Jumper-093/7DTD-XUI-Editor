using System.Collections.Generic;
using System.Xml.Linq;
using XuiEditor.Models;

namespace XuiEditor.Export
{
    public static class XuiImporter
    {
        public static List<XuiElement> ImportWindow(string filePath)
        {
            List<XuiElement> elements = new List<XuiElement>();

            XDocument doc = XDocument.Load(filePath);
            XElement window = doc.Root;

            if (window == null || window.Name != "window")
                return elements;

            foreach (var node in window.Elements())
            {
                XuiElement el = new XuiElement();

                el.Type = node.Name.LocalName;
                el.Name = node.Attribute("name")?.Value ?? "Unnamed";

                // Position
                var pos = node.Attribute("pos")?.Value?.Split(',');
                if (pos?.Length == 2)
                {
                    double.TryParse(pos[0], out double x);
                    double.TryParse(pos[1], out double y);
                    el.X = x;
                    el.Y = y;
                }

                // Size
                var size = node.Attribute("size")?.Value?.Split(',');
                if (size?.Length == 2)
                {
                    double.TryParse(size[0], out double w);
                    double.TryParse(size[1], out double h);
                    el.Width = w;
                    el.Height = h;
                }

                // Optional
                el.Texture = node.Attribute("texture")?.Value;
                el.Text = node.Attribute("text")?.Value;

                // Alle anderen Attribute (modded!)
                foreach (var attr in node.Attributes())
                {
                    if (attr.Name == "name" ||
                        attr.Name == "pos" ||
                        attr.Name == "size" ||
                        attr.Name == "texture" ||
                        attr.Name == "text")
                        continue;

                    el.CustomAttributes[attr.Name.LocalName] = attr.Value;
                }

                elements.Add(el);
            }

            return elements;
        }
    }
}
