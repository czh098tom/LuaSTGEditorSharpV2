using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;

namespace LuaSTGEditorSharpV2.Core.Model.DocumentFormat
{
    internal class XMLFormat : DocumentFormatBase
    {
        private static readonly XmlReaderSettings _readerSettings = new XmlReaderSettings()
        {
        };

        private static readonly XmlWriterSettings _writerSettings = new XmlWriterSettings()
        {
            Indent = true
        };

        private static bool IsProperty(string elementName)
        {
            return elementName.StartsWith("_.");
        }

        public override NodeData CreateFromStream(TextReader streamReader)
        {
            using XmlReader xmlReader = XmlReader.Create(streamReader, _readerSettings);
            NodeData curr = new NodeData();
            string? key = null;
            StringBuilder? value = null;
            while (xmlReader.Read())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (!IsProperty(xmlReader.Name))
                        {
                            if (key != null)
                                throw new FormatException("Invalid occurance of ELEMENT xml node.");
                            var node = new NodeData(xmlReader.Name);
                            curr.Add(node);
                            curr = node;
                            if (xmlReader.HasAttributes)
                            {
                                while (xmlReader.MoveToNextAttribute())
                                {
                                    curr.Properties.Add(xmlReader.Name, xmlReader.Value ?? string.Empty);
                                }
                                xmlReader.MoveToElement();
                            }
                            if (xmlReader.IsEmptyElement)
                            {
                                curr = curr.PhysicalParent
                                    ?? throw new FormatException("Current stream is not a well defined XML document");
                            }
                        }
                        else
                        {
                            if (key != null)
                                throw new FormatException("Invalid occurance of ELEMENT xml node.");
                            key = xmlReader.Name[2..];
                            value = new StringBuilder();
                            if (xmlReader.IsEmptyElement)
                            {
                                curr.Properties.Add(key, value?.ToString() ?? string.Empty);
                                key = null;
                                value = null;
                            }
                        }
                        break;
                    case XmlNodeType.Attribute:
                        curr.Properties.Add(xmlReader.Name, xmlReader.Value ?? string.Empty);
                        break;
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        if (value == null) 
                            throw new FormatException("Invalid occurance of TEXT xml node.");
                        value.Append(xmlReader.Value ?? string.Empty);
                        break;
                    case XmlNodeType.EndElement:
                        if (!IsProperty(xmlReader.Name))
                        {
                            curr = curr.PhysicalParent
                                ?? throw new FormatException("Current stream is not a well defined XML document");
                        }
                        else
                        {
                            if (key != xmlReader.Name[2..])
                                throw new FormatException("Invalid occurance of END_OF_ELEMENT xml node.");
                            curr.Properties.Add(key, value?.ToString() ?? string.Empty);
                            key = null;
                            value = null;
                        }
                        break;
                }
            }
            return curr.PhysicalChildren[0];
        }

        public override void SaveToStream(NodeData root, TextWriter streamWriter)
        {
            using XmlWriter xmlWriter = XmlWriter.Create(streamWriter, _writerSettings);
            xmlWriter.WriteStartDocument();
            WriteNode(root, xmlWriter);
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }

        private void WriteNode(NodeData curr, XmlWriter xw)
        {
            xw.WriteStartElement(curr.TypeUID);
            List<(string, string)>? extraProperties = null;
            foreach (var kvp in curr.Properties)
            {
                if (!kvp.Value.Contains('\n'))
                {
                    xw.WriteStartAttribute(kvp.Key);
                    xw.WriteValue(kvp.Value);
                }
                else
                {
                    extraProperties ??= new List<(string, string)>();
                    extraProperties.Add((kvp.Key, kvp.Value));
                }
            }
            if (extraProperties != null)
            {
                foreach (var kvp in extraProperties)
                {
                    xw.WriteStartElement($"_.{kvp.Item1}");
                    xw.WriteCData(kvp.Item2);
                    xw.WriteEndElement();
                }
            }
            for (int i = 0; i < curr.PhysicalChildren.Count; i++)
            {
                WriteNode(curr.PhysicalChildren[i], xw);
            }
            xw.WriteEndElement();
        }
    }
}
