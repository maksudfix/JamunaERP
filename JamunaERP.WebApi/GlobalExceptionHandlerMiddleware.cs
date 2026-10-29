using FluentValidation;
using JamunaERP.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace JamunaERP.WebApi.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var (status, title, detail, errors) = Map(ex);

                if (status >= 500)
                    _logger.LogError(ex, "Unhandled exception");
                else
                    _logger.LogWarning("Request failed with {Status}: {Detail}", status, detail);

                if (context.Response.HasStarted) throw;

                var problem = new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail = detail,
                    Instance = context.Request.Path
                };
                if (errors is not null) problem.Extensions["errors"] = errors;

                context.Response.Clear();
                context.Response.StatusCode = status;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(problem,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            }
        }

        private static (int Status, string Title, string Detail, object? Errors) Map(Exception ex) => ex switch
        {
            ValidationException v => (400, "Validation failed", "One or more validation errors occurred.",
                v.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })),

            KeyNotFoundException => (404, "Not found", ex.Message, null),

            InsufficientStockException => (409, "Insufficient stock", ex.Message, null),
            BusinessRuleException => (422, "Business rule violated", ex.Message, null),

            // Must come BEFORE DbUpdateException (it is a subclass)
            DbUpdateConcurrencyException => (409, "Concurrency conflict",
                "The record was modified by someone else. Reload and try again.", null),

            DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } } =>
                (409, "Duplicate value", "A record with the same unique value already exists.", null),

            DbUpdateException { InnerException: SqlException { Number: 547 } } =>
                (409, "Related record conflict",
                 "The operation conflicts with a related record (missing or still referenced).", null),

            DbUpdateException => (500, "Database error", "A database error occurred.", null),

            _ => (500, "Server error", "An unexpected error occurred.", null)
        };
    }
}