using MorphSDK.Dto;
using MorphSDK.Dto.Commands;
using MorphSDK.Model;
using MorphSDK.Model.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphSDK.Mappers
{
    internal static class FieldErrorsMapper
    {
        public static List<FieldError> MapFromDto(Error error)
        {
            var result = error.detais.Select(x => new FieldError
            {
                Field = x.target,
                Message = x.message,
                FieldErrorType = ParseFieldErrorType(x.code)
            });
            return result.ToList();
        }

        private static FieldErrorType ParseFieldErrorType(string code)
        {
            FieldErrorType e;
            if (Enum.TryParse(code, out e))
            {
                return e;
            }
            else
            {
                return FieldErrorType.OtherError;
            }
            
        }
       
    }
}
