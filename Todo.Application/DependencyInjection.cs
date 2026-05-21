using Microsoft.Extensions.DependencyInjection;
using Todo.Application.Abstractions;
using Todo.Application.Services;

namespace Todo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITodoService, TodoService>();
        return services;
    }
}
