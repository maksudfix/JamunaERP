using System.Text.Json;
using FluentValidation;
using JamunaERP.Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.WebApi.Middleware;

public sealed class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
                throw;

            var (status, title, detail, errors) = MapException(ex);

            if (status >= 500)
                logger.LogError(ex, "Unhandled exception");
            else
                logger.LogWarning("Request failed with {Status}: {Detail}", status, detail);

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            if (errors is not null)
                problem.Extensions["errors"] = errors;

            context.Response.Clear();
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";

            await JsonSerializer.SerializeAsync(
                context.Response.Body,
                problem,
                JsonOptions,
                context.RequestAborted);
        }
    }

    private static (int Status, string Title, string Detail, object? Errors) MapException(Exception ex)
    {
        return ex switch
        {
            ValidationException validation => (
                400,
                "Validation failed",
                "One or more validation errors occurred.",
                validation.Errors.Select(error => new
                {
                    field = error.PropertyName,
                    message = error.ErrorMessage
                }).ToArray()),
            ForbiddenException => (403, "Forbidden", ex.Message, null),
            KeyNotFoundException => (404, "Not found", ex.Message, null),
            InsufficientStockException => (409, "Insufficient stock", ex.Message, null),
            BusinessRuleException => (422, "Business rule violated", ex.Message, null),
            DbUpdateConcurrencyException => (
                409,
                "Concurrency conflict",
                "The record was modified by someone else. Reload and try again.",
                null),
            DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } } => (
                409,
                "Duplicate value",
                "A record with the same unique value already exists.",
                null),
            DbUpdateException { InnerException: SqlException { Number: 547 } } => (
                409,
                "Related record conflict",
                "The operation conflicts with a related record.",
                null),
            DbUpdateException => (500, "Database error", "A database error occurred.", null),
            _ => (500, "Server error", "An unexpected error occurred.", null)
        };
    }
}