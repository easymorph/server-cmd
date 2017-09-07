using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphSDK.Model.Errors
{
    public class FieldError
    {
        public string Field { get; set; }
        public string Message { get; set; }
        public FieldErrorType FieldErrorType { get; set; }
        public FieldError()
        {

        }
        public FieldError(string field, string message, FieldErrorType fieldErrorType)
        {
            Field = field;
            Message = message;
            FieldErrorType = fieldErrorType;
        }
    }

    public enum FieldErrorType
    {
        OtherError,
        Required,
        MalformedValue,
        
    }
}
