using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GenerateECCDocument.Domain
{
    [XmlRoot("steps")]
    public class Steps
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
        [XmlAttribute("last")]
        public int Last { get; set; }
        [XmlElement("step")]
        public List<Step> Step { get; set; }
    }
}
