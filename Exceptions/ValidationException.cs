namespace DigitalSignage.Exceptions
{
    /// <summary>
    /// Validation hatalarını temsil eden exception sınıfı
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
