using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateECCDocument.Domain
{
    public class TestSuite
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool HasChildren { get; set; }

        [JsonProperty("plan")]
        public TestPlan TestPlan { get; set; }

        public List<TestSuite> Children { get; set; }

        public List<TestCase> TestCases { get; set; }

        public override bool Equals(object obj)
        {
            var item = obj as TestSuite;
            if (item == null)
            {
                return false;
            }
            return Id.Equals(item.Id) && Name.Equals(item.Name);
        }
    }


}
