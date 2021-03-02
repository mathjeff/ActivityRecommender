using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

// This XmlDocument class is a custom implementation of the WPF XmlDocument class, because Silverlight doesn't have it but TextConverter.cs wants one
namespace ActivityRecommendation
{
    class XmlDocument
    {
        public XmlDocument(TextReader text)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            this.xmlReader = XmlReader.Create(text, settings);
        }
        public XmlNode Next()
        {
            XmlNode newNode;
            try
            {
                while (xmlReader.Read())
                {
                    switch (xmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            // found a new element, and now we're reading the nodes inside it
                            newNode = new XmlNode();
                            newNode.Name = xmlReader.Name;
                            newNode.Value = xmlReader.Value;
                            if (nodeStack.Count > 0)
                                nodeStack[nodeStack.Count - 1].ChildNodes.Add(newNode);
                            nodeStack.Add(newNode);
                            break;
                        case XmlNodeType.Text:
                            // found a new element that doesn't have any more nodes inside it
                            newNode = new XmlNode();
                            newNode.Name = xmlReader.Name;
                            newNode.Value = xmlReader.Value;
                            if (nodeStack.Count > 0)
                                nodeStack[nodeStack.Count - 1].ChildNodes.Add(newNode);
                            break;
                        case XmlNodeType.EndElement:
                            // found the end of a previous element
                            XmlNode lastNode = nodeStack[nodeStack.Count - 1];
                            nodeStack.RemoveAt(nodeStack.Count - 1);
                            if (nodeStack.Count == 0)
                            {
                                // We completed a node; return it
                                return lastNode;
                            }
                            break;
                        case XmlNodeType.Whitespace:
                            break;
                        default:
                            throw new Exception("Unrecognized node type: " + xmlReader.NodeType);
                    }
                }
            }
            catch (XmlException e)
            {
                String text = e.ToString();
                System.Diagnostics.Debug.WriteLine(e);
                throw e;
            }
            this.xmlReader.Close();
            this.xmlReader.Dispose();
            this.xmlReader = null;
            return null;
        }

        private XmlReader xmlReader;
        private List<XmlNode> nodeStack = new List<XmlNode>();
    }

    public class XmlNode
    {
        public XmlNode()
        {
            this.ChildNodes = new List<XmlNode>();
        }

        public List<XmlNode> ChildNodes { get; set; }
        public String Name { get; set; }
        public String Value { get; set; }
        public XmlNode FirstChild
        {
            get
            {
                if (this.ChildNodes.Count > 0)
                    return this.ChildNodes[0];
                return null;
            }
        }
    }
}
