// Copyright (C) 2022 Thrive. Version 1.0.

using ThriveEventFlow;

namespace Sample.Domain.SampleAggregate; 

public record InvoiceState : AggregateState<InvoiceState> {
    public string         InvoiceNumber { get; private set; }
    public DateTimeOffset InvoiceDate   { get; private set; }
    public string         InvoiceUri    { get; private set; }
    public Money          GrossAmount   { get; private set; }
    public Money          Tax           { get; private set; }
    public Money          NetAmount     { get; private set; }
    public string         ThriveBrandId { get; private set; }
    public string         ThriveId      { get; private set; }
    public InvoiceStatus  Status        { get; private set; }
};