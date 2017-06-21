using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace AdConta
{
    public class CustomException_DBWrapper : System.Exception
    {
        public CustomException_DBWrapper() : base() { }

        public CustomException_DBWrapper(string message) : base(message) { }
        
        public CustomException_DBWrapper(string message, Exception innerException) : base(message, innerException) { }

        public CustomException_DBWrapper(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
