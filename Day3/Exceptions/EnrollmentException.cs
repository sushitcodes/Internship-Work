namespace SchoolManagementSystem.Exceptions
{
    // CUSTOM EXCEPTION — inherits from Exception so it plays nicely with
    // normal try/catch, but lets calling code catch THIS specific error
    // separately from generic exceptions if it wants to.
    public class EnrollmentException : Exception
    {
        public EnrollmentException(string message) : base(message) { }
    }
}
