using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GenerateECCDocument.Domain
{
    [XmlRoot("parameterizedString")]
    public class ParameterizedString
    {
        [XmlAttribute("isformatted")]
        public bool IsFormatted { get; set; }
        [XmlText()]
        public string Content { get; set; }
    }
}
