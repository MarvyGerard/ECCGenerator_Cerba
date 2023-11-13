using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateECCDocument.Domain
{
    public class Result
    {
        

        public int Id { get; set; }
        public string Outcome { get; set; }
        public DateTime StartedDate { get; set; }
        public DateTime CompletedDate { get; set; }

        public TestCase TestCase { get; set; }

        public string url { get; set; }

        public List<WorkItem> associatedBugs { get; set; }
        public string FailureType { get; set; }
        public string ResolutionState { get; set; }
        public string Comment { get; set; }
        public int testRunId { get; set; }

        public List<Attachment> Attachments { get; set; }
        public List<Iteration> Iterations { get; set; }

    }
}
