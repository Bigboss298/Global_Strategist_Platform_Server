namespace Global_Strategist_Platform_Server.Model.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() : base() { }

        public UnauthorizedException(string message) : base(message) { }

        public UnauthorizedException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
