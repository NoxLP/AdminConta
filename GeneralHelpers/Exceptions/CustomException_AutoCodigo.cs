using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace AdConta
{
    public class CustomException_AutoCodigo : System.Exception
    {
        public CustomException_AutoCodigo() : base() { }

        public CustomException_AutoCodigo(string message) : base(message) { }

        public CustomException_AutoCodigo(string message, Exception innerException) : base(message, innerException) { }

        public CustomException_AutoCodigo(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
