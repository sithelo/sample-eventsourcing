// Copyright (C) 2022 Thrive. Version 1.0.

using ThriveEventFlow;
using NodaTime;

namespace Sample.Domain.Events;

public static class InvoiceEvents {
    public static class V1 {
        [EventType("V1.InvoiceCreated")]
        public record InvoiceCreated(
            string         InvoiceNumber,
            string         InvoiceUri,
            LocalDate      InvoiceDate,
            string         ThriveId,
            float          GrossAmount,
            float          PrepaidAmount,
            float          OutstandingAmount,
            string         Currency,
            DateTimeOffset CreatedDate
        );

        [EventType("V1.PaymentRecorded")]
        public record PaymentRecorded(
            float          PaidAmount,
            float          Outstanding,
            string         Currency,
            string         PaymentId,
            string         PaidBy,
            DateTimeOffset PaidAt
        );

        [EventType("V1.FullyPaid")]
        public record InvoiceFullyPaid(DateTimeOffset FullyPaidAt);

        [EventType("V1.InvoiceCancelled")]
        public record InvoiceCancelled(string CancelledBy, DateTimeOffset CancelledAt);
    }
}