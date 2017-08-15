using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace AdConta
{
    public class CustomException_Mapper : System.Exception
    {
        public CustomException_Mapper() : base() { }

        public CustomException_Mapper(string message) : base(message) { }

        public CustomException_Mapper(string message, Exception innerException) : base(message, innerException) { }

        public CustomException_Mapper(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
