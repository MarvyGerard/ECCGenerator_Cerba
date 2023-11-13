using System;
using System.Collections.Generic;
using System.Xml;

namespace GenerateECCDocument.Domain
{
    public class Iteration
    {
        public int Id { get; set; }
        public string Outcome { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime StartedDate { get; set; }
        public DateTime CompletedDate { get; set; }
        //public string DurationInMs { get; set; }
        public List<ActionResult> ActionResults { get; set; }
        public List<Attachment> Attachments { get; set; }
        //public XmlDocument Parameters { get; set; }

    }
}