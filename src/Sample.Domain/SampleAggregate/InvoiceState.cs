// Copyright (C) 2022 Thrive. Version 1.0.

using System.Collections.Immutable;
using NodaTime;
using static Sample.Domain.Events.InvoiceEvents;
using ThriveEventFlow;


namespace Sample.Domain.SampleAggregate;

public record InvoiceState : AggregateState<InvoiceState> {
    public InvoiceId InvoiceNumber { get; init; }
    public LocalDate InvoiceDate   { get; init; }
    public string    InvoiceUri    { get; init; }
    public string    ThriveId      { get; init; }
    public Money     GrossAmount   { get; init; }
    public Money     Tax           { get; init; }
    public Money     NetAmount     { get; init; }
    public Money     Outstanding   { get; init; }
    public bool      Paid          { get; init; }

    public ImmutableList<PaymentRecord> PaymentRecords { get; init; } = ImmutableList<PaymentRecord>.Empty;

    internal bool HasPaymentBeenRecorded(string paymentId)
        => PaymentRecords.Any(x => x.PaymentId == paymentId);

    public InvoiceState() {
        On<V1.InvoiceCreated>(HandleCreated);
        On<V1.PaymentRecorded>(HandlePayment);
        On<V1.InvoiceFullyPaid>((state, paid) => state with { Paid = true });
    }

    static InvoiceState HandlePayment(InvoiceState state, V1.PaymentRecorded e)
        => state with {
            Outstanding = new Money { Amount = e.Outstanding, Currency = e.Currency },
            PaymentRecords = state.PaymentRecords.Add(
                new PaymentRecord(e.PaymentId, new Money { Amount = e.PaidAmount, Currency = e.Currency })
            )
        };

    static InvoiceState HandleCreated(InvoiceState state, V1.InvoiceCreated invoiced)
        => state with {
            InvoiceNumber = new InvoiceId(invoiced.InvoiceNumber),
            InvoiceDate = invoiced.InvoiceDate,
            ThriveId = invoiced.ThriveId,
            GrossAmount = new Money { Amount = invoiced.GrossAmount, Currency       = invoiced.Currency },
            Outstanding = new Money { Amount = invoiced.OutstandingAmount, Currency = invoiced.Currency }
        };
};