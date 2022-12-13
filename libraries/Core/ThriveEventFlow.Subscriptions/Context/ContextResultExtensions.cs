// Copyright (C) 2021-2022 Ubiquitous AS. All rights reserved
// Licensed under the Apache License, Version 2.0.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using ThriveEventFlow.Diagnostics;
using ThriveEventFlow.Subscriptions.Logging;
using ActivityStatus = ThriveEventFlow.Diagnostics.ActivityStatus;

namespace ThriveEventFlow.Subscriptions.Context;

public static class ContextResultExtensions {
    
    public static void Ack(this IBaseConsumeContext context, string handlerType) {
        context.HandlingResults.Add(EventHandlingResult.Succeeded(handlerType));
        context.LogContext.MessageHandled(handlerType, context);
    }

   
    public static void Nack(this IBaseConsumeContext context, string handlerType, Exception? exception) {
        context.HandlingResults.Add(EventHandlingResult.Failed(handlerType, exception));
        if (exception is not TaskCanceledException)
            context.LogContext.MessageHandlingFailed(handlerType, context, exception);

        if (Activity.Current != null && Activity.Current.Status != ActivityStatusCode.Error) {
            Activity.Current.SetActivityStatus(
                ActivityStatus.Error(exception, $"Error handling {context.MessageType}")
            );
        }
    }

   
    public static void Ignore(this IBaseConsumeContext context, string handlerType) {
        context.HandlingResults.Add(EventHandlingResult.Ignored(handlerType));
        context.LogContext.MessageIgnored(handlerType, context);
    }

    
    public static void Ack<T>(this IBaseConsumeContext context) => context.Ack(typeof(T).Name);

   
    public static void Ignore<T>(this IBaseConsumeContext context) => context.Ignore(typeof(T).Name);

    
    public static void Nack<T>(this IBaseConsumeContext context, Exception? exception)
        => context.Nack(typeof(T).Name, exception);

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool WasIgnored(this IBaseConsumeContext context) {
        var status       = context.HandlingResults.GetIgnoreStatus();
        var handleStatus = context.HandlingResults.GetFailureStatus();

        return (status & EventHandlingStatus.Ignored) == EventHandlingStatus.Ignored && handleStatus == 0;
    }

    /// <summary>
    /// Returns true if any of the handlers reported a failure
    /// </summary>
    /// <param name="context">Consume context</param>
    /// <returns></returns>
    public static bool HasFailed(this IBaseConsumeContext context)
        => context.HandlingResults.GetFailureStatus() == EventHandlingStatus.Failure;
}