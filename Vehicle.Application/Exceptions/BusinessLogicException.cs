namespace Vehicle.Application.Exceptions
{
    public class BusinessLogicException : Exception
    {
        public BusinessLogicException() : base("Business logic validation failed")
        {
        }

        public BusinessLogicException(string message) : base(message)
        {
        }

        public BusinessLogicException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
