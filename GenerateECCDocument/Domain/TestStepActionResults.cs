using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateECCDocument.Domain
{
    public class TestStepActionResults
    {
        public string actionPath { get; set; }
        public int iterationId { get; set; }
        public int stepIdentifier { get; set; }
        public string outcome { get; set; }
        public string errorMessage { get; set; }
        public DateTime startedDate { get; set; }
        public DateTime completedDate { get; set; }
    }
}
