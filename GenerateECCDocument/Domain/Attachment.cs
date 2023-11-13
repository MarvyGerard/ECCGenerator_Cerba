using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateECCDocument.Domain
{
    public class Attachment
    {
        public string Url { get; set; }
        public string FileName { get; set; }

        public string name { get; set; }
        public int id { get; set; }
        public int size { get; set; }
        public int iterationId { get; set; }
        public string actionPath { get; set; }
        public string Base64String { get; set; }
    }
}
