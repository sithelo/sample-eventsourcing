// Copyright (C) 2022 Thrive. Version 1.0.

namespace Sample.Api.Application.Writes;

public class InvoiceCommands {
    public record CreateInvoice(
        string   InvoiceNumber,
        string   InvoiceUri,
        DateTime InvoiceDate,
        string   ThriveId,
        float    TotalPrice,
        float    PrepaidAmount,
        string   Currency);

    public record RecordPayment(string InvoiceNumber,
        float                          PaidAmount,
        string                         Currency,
        string                         PaymentId,
        string                         PaidBy);
}