using FluentValidation;
using JamunaERP.Application.Common.Behaviours;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace JamunaERP.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services, string? mediatrLicenseKey = null)
        {
            // Register MediatR handlers and register the Validation Pipeline Behavior
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                if (!string.IsNullOrWhiteSpace(mediatrLicenseKey))
                    cfg.LicenseKey = mediatrLicenseKey;
            });

            // Register FluentValidation validators from this assembly
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}