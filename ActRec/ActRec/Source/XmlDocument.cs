using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

// This XmlDocument class is a custom implementation of the WPF XmlDocument class, because Silverlight doesn't have it but TextConverter.cs wants one
namespace ActivityRecommendation
{
    class XmlDocument : XmlNode
    {
        public void LoadXml(String xml)
        {
            XmlReader reader = XmlReader.Create(new StringReader(xml));
            List<XmlNode> nodeStack = new List<XmlNode>();
            XmlNode newNode = this;
            nodeStack.Add(newNode);
            try
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            // found a new element, and now we're reading the nodes inside it
                            newNode = new XmlNode();
                            newNode.Name = reader.Name;
                            newNode.Value = reader.Value;
                            nodeStack[nodeStack.Count - 1].ChildNodes.Add(newNode);
                            nodeStack.Add(newNode);
                            break;
                        case XmlNodeType.Text:
                            // found a new element that doesn't have any more nodes inside it
                            newNode = new XmlNode();
                            newNode.Name = reader.Name;
                            newNode.Value = reader.Value;
                            nodeStack[nodeStack.Count - 1].ChildNodes.Add(newNode);
                            break;
                        case XmlNodeType.EndElement:
                            // found the end of a previous element
                            nodeStack.RemoveAt(nodeStack.Count - 1);
                            break;
                        case XmlNodeType.Whitespace:
                            break;
                        default:
                            throw new Exception("Unrecognized node type: " + reader.NodeType);
                    }
                }
            }
            catch (XmlException e)
            {
                String text = e.ToString();
                System.Diagnostics.Debug.WriteLine(e);
                throw e;
            }
        }
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
