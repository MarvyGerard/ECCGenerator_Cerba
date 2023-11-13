using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GenerateECCDocument.Domain
{
    public class TestCase
    {
        public Steps Steps { get; set; }
        public List<ParameterCollectionPerIteration> ParameterCollectionPerIteration { get; set; }
        public WorkItem WorkItem { get; set; }
        public int Id { get; set; }

        public string Name { get; set; }

        public List<Result> Results { get; set; }
        public Result LatestResults { get; set; }

        public int ResultPrintedForTestCaseCount { get; set; } = 0;

        public override bool Equals(object obj)
        {
            var item = obj as TestCase;
            if (item == null)
            {
                return false;
            }
            return Id.Equals(item.Id) && Name.Equals(item.Name);
        }
    }
}
