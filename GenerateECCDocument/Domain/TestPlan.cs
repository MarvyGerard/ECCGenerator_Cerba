using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateECCDocument.Domain
{
    public class TestPlan
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<TestSuite> SuitesList { get; set; }
        public TestSuite RootSuite { get; set; }

        public List<TestRun> TestRuns { get; set; }
        public List<Result> Results { get; set; }
    }
}
