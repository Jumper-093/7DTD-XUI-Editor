using System.Collections.Generic;
using System.Xml.Linq;
using XuiEditor.Models;

namespace XuiEditor.Export
{
    public static class XuiExporter
    {
        public static XDocument ExportWindow(
            string windowName,
            List<XuiElement> elements)
        {
            XElement window = new XElement("window",
                new XAttribute("name", windowName)
            );

            foreach (var el in elements)
            {
                XElement node = new XElement(el.Type,
                    new XAttribute("name", el.Name),
                    new XAttribute("pos", $"{el.X},{el.Y}"),
                    new XAttribute("size", $"{el.Width},{el.Height}")
                );

                if (!string.IsNullOrEmpty(el.Texture))
                    node.Add(new XAttribute("texture", el.Texture));

                if (!string.IsNullOrEmpty(el.Text))
                    node.Add(new XAttribute("text", el.Text));

                foreach (var kv in el.CustomAttributes)
                    node.Add(new XAttribute(kv.Key, kv.Value));

                window.Add(node);
            }

            return new XDocument(window);
        }
    }
}
