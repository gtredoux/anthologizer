using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace com.renoster.Anthologizer.Media
{
    public enum NormalizeEnum
    {
        Flatten,
        Deepen
    };

    [XmlRoot("item")]
    public class Item : IItem
    {
        private bool anthologized = false;
        private string digest=null;
        private int pathCS=0;
        private string location = null;

        private ItemTypeEnum itemType;

        [XmlElement("ItemType")]
        public ItemTypeEnum ItemType
        {
            get { return itemType; }
            set { itemType = value; }
        }

        private string name;

        [XmlElement("Name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string id;

         [XmlElement("Id")]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        private DateTime lastModified;

        private long size;


        [XmlElement("Size")]
        public long Size
        {
            get { return size; }
            set { size = value; }
        }
        private string mimetype;

        [XmlElement("MimeType")]
        public string Mimetype
        {
            get { return mimetype; }
            set { mimetype = value; }
        }

        [XmlElement("LastModified")]
        public DateTime LastModified
        {
            get { return lastModified; }
            set { lastModified = value; }
        }

        [XmlElement("Digest")]
        public string Digest
        {
            get { return digest; }
            set { digest = value; }
        }

        [XmlElement("Anthologized")]
        public bool Anthologized
        {
            get { return anthologized; }
            set { anthologized = value; }
        }

        [XmlElement("PathCS")]
        public int PathCS
        {
            get { return pathCS; }
            set { pathCS=value; }        }

        [XmlElement("Location")]
        public string Location
        {
            get { return location; }
            set { location = value; }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}