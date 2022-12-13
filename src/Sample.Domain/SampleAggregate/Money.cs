﻿// Copyright (C) 2022 Thrive. Version 1.0.

using ThriveEventFlow;

namespace Sample.Domain.SampleAggregate; 

public record Money {
    public float  Amount   { get; internal init; }
    public string Currency { get; internal init; }

    private static readonly string[] SupportedCurrencies = { "ZAR" };

    internal Money() {
    }

    public Money(float amount, string currency) {
        if (!SupportedCurrencies.Contains(currency)) throw new DomainException($"Unsupported currency: {currency}");

        Amount   = amount;
        Currency = currency;
    }

    public static implicit operator double(Money money) => money.Amount;
}