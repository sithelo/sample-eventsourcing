// Copyright (C) 2022 Thrive. Version 1.0.

using ThriveEventFlow;

namespace Sample.Domain.SampleAggregate; 

public record InvoiceId(string Value) : AggregateId(Value);