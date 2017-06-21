using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace AdConta
{
    public class CustomException_Repository : System.Exception
    {
        public CustomException_Repository() : base() { }

        public CustomException_Repository(string message) : base(message) { }

        public CustomException_Repository(string message, Exception innerException) : base(message, innerException) { }

        public CustomException_Repository(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
