// Copyright (C) 2022 Thrive. Version 1.0.

using Sample.Domain.SampleAggregate;

namespace Sample.Domain.Services;

public static class ServiceExtensions {
    public delegate ValueTask<bool> IsInvoiceAvailable(InvoiceId roomId, string thriveId);

    public delegate Money ConvertCurrency(Money from, string targetCurrency);
}