using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateECCDocument.Domain
{
    public class Parameter
    {
        public Parameter(string parameterName, string value)
        {
            ParameterName = parameterName;
            Value = value;
        }
        public int IterationId { get; set; }
        public string ActionPath { get; set; }
        public string ParameterName { get; set; }
        public string Value { get; set; }
    }
}
