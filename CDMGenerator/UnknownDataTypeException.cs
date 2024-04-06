using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDMGenerator
{
    public class UnknownDataTypeException: Exception
    {
        public UnknownDataTypeException() { }
        
        public UnknownDataTypeException(string message) : base(message) { }

    }
}
