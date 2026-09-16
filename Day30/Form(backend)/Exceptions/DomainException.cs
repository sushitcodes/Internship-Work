// Form/Exceptions/DomainExceptions.cs
namespace Form.Exceptions;

public abstract class DomainException(string message) : Exception(message);

public class ValidateException(string message) : DomainException(message);
public class NotFoundException(string message) : DomainException(message);
public class ConflictException(string message) : DomainException(message);