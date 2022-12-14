// Copyright (C) 2022 Thrive. Version 1.0.

using NodaTime;
using Sample.Domain.SampleAggregate;
using Sample.Domain.Services;
using ThriveEventFlow;

namespace Sample.Api.Application.Writes; 

public class InvoiceCommandService : ApplicationService<Invoice, InvoiceState, InvoiceId> {
    public InvoiceCommandService(IAggregateStore store, ServiceExtensions.IsInvoiceAvailable isInvoiceAvailable) : base(store) {
        OnNewAsync<InvoiceCommands.CreateInvoice>(
            cmd => new InvoiceId(cmd.InvoiceNumber),
            (inv, cmd, _) => inv.CreateInvoice(
                new InvoiceId(cmd.InvoiceNumber),
                LocalDate.FromDateTime(cmd.InvoiceDate),
                cmd.InvoiceUri,
                new Money(cmd.TotalPrice, cmd.Currency),
                new Money(cmd.PrepaidAmount, cmd.Currency),
                cmd.ThriveId,
                DateTimeOffset.Now,
                isInvoiceAvailable
            )
        );

        OnExisting<InvoiceCommands.RecordPayment>(
            cmd => new InvoiceId(cmd.InvoiceNumber),
            (booking, cmd) => booking.RecordPayment(
                new Money(cmd.PaidAmount, cmd.Currency),
                cmd.PaymentId,
                cmd.PaidBy,
                DateTimeOffset.Now
            )
        );
    }
}