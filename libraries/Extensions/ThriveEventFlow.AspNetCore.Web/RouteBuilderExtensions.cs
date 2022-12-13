using System.Reflection;
using JetBrains.Annotations;
using ThriveEventFlow.AspNetCore.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ThriveEventFlow;

// ReSharper disable CheckNamespace

namespace Microsoft.AspNetCore.Routing;

public delegate TCommand EnrichCommandFromHttpContext<TCommand>(TCommand command, HttpContext httpContext);

public static class RouteBuilderExtensions {
   
    [PublicAPI]
    public static RouteHandlerBuilder MapCommand<TCommand, TAggregate>(
        this IEndpointRouteBuilder              builder,
        EnrichCommandFromHttpContext<TCommand>? enrichCommand = null
    ) where TAggregate : Aggregate where TCommand : class {
        var attr  = typeof(TCommand).GetAttribute<HttpCommandAttribute>();
        var route = GetRoute<TCommand>(attr?.Route);
        return builder.MapCommand<TCommand, TAggregate>(route, enrichCommand);
    }

    
    [PublicAPI]
    public static RouteHandlerBuilder MapCommand<TCommand, TAggregate>(
        this IEndpointRouteBuilder              builder,
        string                                  route,
        EnrichCommandFromHttpContext<TCommand>? enrichCommand = null
    ) where TAggregate : Aggregate where TCommand : class
        => Map<TAggregate, TCommand>(builder, route, enrichCommand);

    
    [PublicAPI]
    public static ApplicationServiceRouteBuilder<TAggregate> MapAggregateCommands<TAggregate>(
        this IEndpointRouteBuilder builder
    ) where TAggregate : Aggregate
        => new(builder);

   
    [PublicAPI]
    public static IEndpointRouteBuilder MapDiscoveredCommands<TAggregate>(
        this   IEndpointRouteBuilder builder,
        params Assembly[]            assemblies
    ) where TAggregate : Aggregate {
        var assembliesToScan = assemblies.Length == 0 ? AppDomain.CurrentDomain.GetAssemblies() : assemblies;

        var attributeType = typeof(HttpCommandAttribute);

        foreach (var assembly in assembliesToScan) {
            MapAssemblyCommands(assembly);
        }

        void MapAssemblyCommands(Assembly assembly) {
            var decoratedTypes = assembly.DefinedTypes.Where(
                x => x.IsClass && x.CustomAttributes.Any(a => a.AttributeType == attributeType)
            );

            var method = typeof(RouteBuilderExtensions).GetMethod(
                nameof(Map),
                BindingFlags.Static | BindingFlags.NonPublic
            )!;

            foreach (var type in decoratedTypes) {
                var attr = type.GetAttribute<HttpCommandAttribute>()!;

                if (attr.AggregateType != null && attr.AggregateType != typeof(TAggregate))
                    throw new InvalidOperationException(
                        $"Command aggregate is {attr.AggregateType.Name} but expected to be {typeof(TAggregate).Name}"
                    );

                var genericMethod = method.MakeGenericMethod(typeof(TAggregate), type);
                genericMethod.Invoke(null, new object?[] { builder, attr.Route, null });
            }
        }

        return builder;
    }

    static RouteHandlerBuilder Map<TAggregate, TCommand>(
        IEndpointRouteBuilder                   builder,
        string?                                 route,
        EnrichCommandFromHttpContext<TCommand>? enrichCommand = null
    ) where TAggregate : Aggregate where TCommand : notnull
        => builder
            .MapPost(
                GetRoute<TCommand>(route),
                async Task<IResult>(HttpContext context, IApplicationService<TAggregate> service) => {
                    var cmd = await context.Request.ReadFromJsonAsync<TCommand>(context.RequestAborted);

                    if (cmd == null) throw new InvalidOperationException("Failed to deserialize the command");

                    if (enrichCommand != null) cmd = enrichCommand(cmd, context);

                    var result = await service.Handle(cmd, context.RequestAborted);
                    return result.AsResult();
                }
            )
            .Accepts<TCommand>(false, "application/json")
            .Produces<Result>()
            .Produces<ErrorResult>(StatusCodes.Status404NotFound)
            .Produces<ErrorResult>(StatusCodes.Status409Conflict)
            .Produces<ErrorResult>(StatusCodes.Status400BadRequest);

    [PublicAPI]
    public static IEndpointRouteBuilder MapDiscoveredCommands(
        this   IEndpointRouteBuilder builder,
        params Assembly[]            assemblies
    ) {
        var assembliesToScan = assemblies.Length == 0 ? AppDomain.CurrentDomain.GetAssemblies() : assemblies;

        var attributeType = typeof(HttpCommandAttribute);

        foreach (var assembly in assembliesToScan) {
            MapAssemblyCommands(assembly);
        }

        return builder;

        void MapAssemblyCommands(Assembly assembly) {
            var decoratedTypes = assembly.DefinedTypes.Where(
                x => x.IsClass && x.CustomAttributes.Any(a => a.AttributeType == attributeType)
            );

            foreach (var type in decoratedTypes) {
                var attr            = type.GetAttribute<HttpCommandAttribute>()!;
                var parentAttribute = type.DeclaringType?.GetAttribute<AggregateCommands>();
                if (parentAttribute == null) continue;

                LocalMap(parentAttribute.AggregateType, type, attr.Route);
            }
        }

        void LocalMap(Type aggregateType, Type type, string? route) {
            var appServiceBase = typeof(IApplicationService<>);
            var appServiceType = appServiceBase.MakeGenericType(aggregateType);

            builder
                .MapPost(
                    GetRoute(type, route),
                    async Task<IResult>(HttpContext context) => {
                        var cmd = await context.Request.ReadFromJsonAsync(type, context.RequestAborted);

                        if (cmd == null) throw new InvalidOperationException("Failed to deserialize the command");

                        if (context.RequestServices.GetRequiredService(appServiceType) is not IApplicationService
                            service) throw new InvalidOperationException("Unable to resolve the application service");

                        var result = await service.Handle(cmd, context.RequestAborted);

                        return result.AsResult();
                    }
                )
                .Accepts(type, false, "application/json")
                .Produces<Result>()
                .Produces<ErrorResult>(StatusCodes.Status404NotFound)
                .Produces<ErrorResult>(StatusCodes.Status409Conflict)
                .Produces<ErrorResult>(StatusCodes.Status400BadRequest);
        }
    }

    static string GetRoute<TCommand>(string? route) => GetRoute(typeof(TCommand), route);

    static string GetRoute(MemberInfo type, string? route) {
        return route ?? Generate();

        string Generate() {
            var gen = type.Name;
            return char.ToLowerInvariant(gen[0]) + gen[1..];
        }
    }
}
