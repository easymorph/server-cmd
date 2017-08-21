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
        
        public MorphApiConflictException(string message) : base("Conflict",message)
        {
            
        }
    }

    public class MorphApiNotFountException : MorphClientGeneralException
    {

        public MorphApiNotFountException(string message) : base("NotFound", message)
        {

        }
    }
}
