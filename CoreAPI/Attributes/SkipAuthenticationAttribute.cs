namespace CoreAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class SkipAuthenticationAttribute : Attribute
    {
    }
}
