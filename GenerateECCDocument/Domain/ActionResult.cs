using System;

namespace GenerateECCDocument.Domain
{
    public class ActionResult
    {
        public string ActionPath { get; set; }
        public int IterationId { get; set; }
        public int StepIdentifier { get; set; }
        public string Outcome { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime StartedDate { get; set; }
        public DateTime CompletedDate { get; set; }
    }
}