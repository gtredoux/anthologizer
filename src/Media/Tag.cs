using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace com.renoster.Anthologizer.Media
{
    [XmlRoot("Tag")]
    public class Tag : ITag
    {
        private string tagname;

        [XmlElement("Tagname")]
        public string Tagname
        {
            get { return tagname; }
            set { tagname = value; }
        }

        private string tagvalue;

        [XmlElement("Tagvalue")]
        public string Tagvalue
        {
            get { return tagvalue; }
            set { tagvalue = value; }
        }
    }
}