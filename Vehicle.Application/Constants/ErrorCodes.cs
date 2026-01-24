namespace Vehicle.Application.Constants
{
    public static class ErrorCodes
    {
        public const string AUTH_INVALID_CREDENTIALS = "AUTH_INVALID_CREDENTIALS";
        public const string AUTH_TOKEN_EXPIRED = "AUTH_TOKEN_EXPIRED";
        public const string AUTH_TOKEN_INVALID = "AUTH_TOKEN_INVALID";
        public const string UNAUTHORIZED_ACCESS = "UNAUTHORIZED_ACCESS";

        public const string VALIDATION_FAILED = "VALIDATION_FAILED";
        public const string INVALID_ARGUMENT = "INVALID_ARGUMENT";
        public const string INVALID_OPERATION = "INVALID_OPERATION";

        public const string DATABASE_ERROR = "DATABASE_ERROR";
        public const string ENTITY_NOT_FOUND = "ENTITY_NOT_FOUND";

        public const string INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR";
        public const string UNHANDLED_EXCEPTION = "UNHANDLED_EXCEPTION";
        public const string NULL_REFERENCE_ERROR = "NULL_REFERENCE_ERROR";
        public const string INDEX_OUT_OF_RANGE_ERROR = "INDEX_OUT_OF_RANGE_ERROR";
        public const string BUSINESS_LOGIC_ERROR = "BUSINESS_LOGIC_ERROR";
    }
}
