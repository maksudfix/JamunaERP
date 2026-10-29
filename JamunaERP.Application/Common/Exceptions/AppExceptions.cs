namespace JamunaERP.Application.Common.Exceptions;

// Inherits KeyNotFoundException so your existing handlers keep working
public class NotFoundException : KeyNotFoundException
{
    public NotFoundException(string entity, object key)
        : base($"{entity} with ID {key} was not found.") { }
}

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

public class InsufficientStockException : BusinessRuleException
{
    public InsufficientStockException(int productId, string productName, int requested, int available)
        : base($"Insufficient stock for '{productName}' (ID {productId}): requested {requested}, available {available}.") { }
}