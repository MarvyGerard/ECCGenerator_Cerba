using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateECCDocument.Domain
{
    public class TestRun
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public List<Result> Results { get; set; }
    }
}
