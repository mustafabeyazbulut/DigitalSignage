namespace DigitalSignage.Exceptions
{
    /// <summary>
    /// İş mantığı hatalarını temsil eden exception sınıfı
    /// </summary>
    public class BusinessLogicException : Exception
    {
        public BusinessLogicException(string message) : base(message)
        {
        }

        public BusinessLogicException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
