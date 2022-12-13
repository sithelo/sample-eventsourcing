// Copyright (C) 2022 Thrive. Version 1.0.

using ThriveEventFlow;

namespace Sample.Domain.SampleAggregate; 

public record InvoiceStatus {
    public string Status { get; internal init; }

    public static readonly string[] SupportedStatuses = {
        "PAID", "OPEN", "CONFIRMED", "CLOSED", "PENDING", "PENDING_CONFIRMATION", "PENDING_AMENDMENT", "CANCELLED",
        "PENDING_CANCELLATION"
    };

    internal InvoiceStatus() {
    }

    public InvoiceStatus(string status) {
        if (!SupportedStatuses.Contains(status.ToUpper())) throw new DomainException($"Unsupported status: {status}");

        Status = status;
    }
}