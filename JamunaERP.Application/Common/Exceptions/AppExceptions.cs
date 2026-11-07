namespace JamunaERP.Application.Common.Exceptions;

// Inherits KeyNotFoundException for compatibility with existing handlers.
public class NotFoundException : KeyNotFoundException
{
    public NotFoundException(string entity, object key)
        : base($"{entity} with ID {key} was not found.")
    {
    }
}

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message)
        : base(message)
    {
    }
}

// The caller is authenticated but is not allowed to perform this action.
public class ForbiddenException : Exception
{
    public ForbiddenException(string message)
        : base(message)
    {
    }
}

public class InsufficientStockException : BusinessRuleException
{
    public InsufficientStockException(
        int productId,
        string productName,
        int requested,
        int available)
        : base(
            $"Insufficient stock for '{productName}' " +
            $"(ID {productId}): requested {requested}, available {available}.")
    {
    }
}