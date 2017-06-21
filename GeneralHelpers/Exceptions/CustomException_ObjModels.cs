using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace AdConta
{
    public class CustomException_ObjModels : System.Exception
    {
        public CustomException_ObjModels() : base() { }

        public CustomException_ObjModels(string message) : base(message) { }

        public CustomException_ObjModels(string message, Exception innerException) : base(message, innerException) { }

        public CustomException_ObjModels(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class CustomException_Publica_ObjModels : System.Exception
    {
        public CustomException_Publica_ObjModels() : base() { }

        public CustomException_Publica_ObjModels(string message) : base(message) { }

        public CustomException_Publica_ObjModels(string message, Exception innerException) : base(message, innerException) { }

        public CustomException_Publica_ObjModels(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class CustomException_DataListObjects : System.Exception
    {
        public CustomException_DataListObjects() : 
            base("To create DataListObject don't use parameterless void SetProperties method, use overloaded version with parameters instead.")
        { }

        public CustomException_DataListObjects(string message) : base(message) { }

        public CustomException_DataListObjects(string message, Exception innerException) : base(message, innerException) { }

        public CustomException_DataListObjects(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
