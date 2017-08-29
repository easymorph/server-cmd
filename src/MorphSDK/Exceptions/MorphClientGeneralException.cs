using MorphSDK.Dto;
using System;

namespace MorphSDK.Exceptions
{

    public abstract class MorphClientBaseException : Exception
    {

        public MorphClientBaseException(string message) : base(message)
        {

        }
    }
    public class MorphClientCommunicationException : MorphClientBaseException
    {

        public MorphClientCommunicationException(string message) : base(message)
        {

        }
    }


    public class MorphClientGeneralException : MorphClientBaseException
    {
        public string Code { get; protected set; }
        public MorphClientGeneralException(string code, string message) : base(message)
        {
            Code = code;
        }
    }

    public class MorphApiConflictException : MorphClientGeneralException
    {
        
        public MorphApiConflictException(string message) : base(ReadableErrorTopCode.Conflict, message)
        {
            
        }
    }

    public class MorphApiNotFoundException : MorphClientGeneralException
    {

        public MorphApiNotFoundException(string message) : base(ReadableErrorTopCode.NotFound, message)
        {

        }
    }
    public class MorphApiForbiddenException : MorphClientGeneralException
    {

        public MorphApiForbiddenException(string message) : base(ReadableErrorTopCode.Forbidden, message)
        {

        }
    }
}
