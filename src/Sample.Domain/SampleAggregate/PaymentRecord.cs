// Copyright (C) 2022 Thrive. Version 1.0.

namespace Sample.Domain.SampleAggregate; 


public record PaymentRecord(string PaymentId, Money PaidAmount);