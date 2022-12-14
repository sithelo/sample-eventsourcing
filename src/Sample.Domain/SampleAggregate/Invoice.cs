// Copyright (C) 2022 Thrive. Version 1.0.

using NodaTime;
using Sample.Domain.Events;
using static Sample.Domain.Services.ServiceExtensions;
using static Sample.Domain.Events.InvoiceEvents;
using ThriveEventFlow;

namespace Sample.Domain.SampleAggregate;

public class Invoice : Aggregate<InvoiceState> {
    public async Task CreateInvoice(
        InvoiceId          invoiceNumber,
        LocalDate          invoiceDate,
        string             invoiceUri,
        Money              grossAmount,
        Money              prepaid,
        string             thriveId,
        DateTimeOffset     createdAt,
        IsInvoiceAvailable isInvoiceAvailable
    ) {
        EnsureDoesntExist();
        await EnsureInvoiceAvailable(invoiceNumber, thriveId, isInvoiceAvailable);

        var outstanding = grossAmount - prepaid;

        Apply(
            new InvoiceEvents.V1.InvoiceCreated(
                invoiceNumber,
                invoiceUri,
                invoiceDate,
                thriveId,
                grossAmount.Amount,
                prepaid.Amount,
                outstanding.Amount,
                grossAmount.Currency,
                createdAt
            )
        );

        MarkFullyPaidIfNecessary(createdAt);
    }
    public void RecordPayment(
        Money          paid,
        string         paymentId,
        string         paidBy,
        DateTimeOffset paidAt
    ) {
        EnsureExists();

        if (State.HasPaymentBeenRecorded(paymentId)) return;
            
        var outstanding = State.Outstanding - paid;

        Apply(
            new V1.PaymentRecorded(
                paid.Amount,
                outstanding.Amount,
                paid.Currency,
                paymentId,
                paidBy,
                paidAt
            )
        );
            
        MarkFullyPaidIfNecessary(paidAt);
    }

    void MarkFullyPaidIfNecessary(DateTimeOffset when) {
        if (State.Outstanding.Amount != 0) return;

        Apply(new V1.InvoiceFullyPaid(when));
    }
    static async Task EnsureInvoiceAvailable(InvoiceId invoiceNumber, string thriveId, IsInvoiceAvailable isInvoiceAvailable) {
        var invoiceAvailable = await isInvoiceAvailable(invoiceNumber, thriveId);
        if (invoiceAvailable) throw new DomainException("Invoice already created");
    }

}