using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GenerateECCDocument.Domain
{
    public class WorkItemFields
    {
        [JsonProperty("Microsoft.VSTS.TCM.Steps")]
        public string Steps { get; set; }

        [JsonProperty("Microsoft.VSTS.TCM.LocalDataSource")]
        public string Parameters { get; set; }
    }
}
