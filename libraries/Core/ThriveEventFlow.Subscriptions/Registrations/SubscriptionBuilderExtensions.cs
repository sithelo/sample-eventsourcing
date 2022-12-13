// Copyright (C) 2021-2022 Ubiquitous AS. All rights reserved
// Licensed under the Apache License, Version 2.0.

using ThriveEventFlow.Subscriptions.Filters;
using ThriveEventFlow.Subscriptions.Filters.Partitioning;
using JetBrains.Annotations;
using ThriveEventFlow.Subscriptions.Registrations;

namespace ThriveEventFlow.Subscriptions.Registrations;

public static class SubscriptionBuilderExtensions {
    
    [PublicAPI]
    public static SubscriptionBuilder WithPartitioning(
        this SubscriptionBuilder    builder,
        int                         partitionsCount,
        Partitioner.GetPartitionKey getPartitionKey
    )
        => builder.AddConsumeFilterFirst(new PartitioningFilter(partitionsCount, getPartitionKey));

   
    public static SubscriptionBuilder WithPartitioningByStream(
        this SubscriptionBuilder builder,
        int                      partitionsCount
    )
        => builder.WithPartitioning(partitionsCount, ctx => ctx.Stream);
}
