using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateECCDocument.Domain
{
    public class WorkItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<WorkItemFields> workItemFields { get; set; }

    }
}
