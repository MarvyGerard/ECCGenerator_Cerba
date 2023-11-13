using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GenerateECCDocument.Domain
{
    [XmlRoot("step")]
    public class Step
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
        [XmlAttribute("type")]
        public string Type { get; set; }
        [XmlAttribute("description")]
        public string Description { get; set; }
        [XmlElement("parameterizedString")]
        public List<ParameterizedString> ParameterizedString { get; set; }
    }
}
