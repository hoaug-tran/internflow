namespace InternFlow.Shared.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message) { }
}

public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(message) { }
}

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message) { }
}
<<<<<<< HEAD

=======
>>>>>>> origin/main
public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message) { }
}
