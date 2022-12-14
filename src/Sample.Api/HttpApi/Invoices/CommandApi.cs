// Copyright (C) 2022 Thrive. Version 1.0.

using Microsoft.AspNetCore.Mvc;
using static Sample.Api.Application.Writes.InvoiceCommands;
using Sample.Domain.SampleAggregate;
using ThriveEventFlow;
using ThriveEventFlow.AspNetCore.Web;

namespace Sample.Api.HttpApi.Invoices; 

[Route("/invoice")]
public class CommandApi : CommandHttpApiBase<Invoice> {
    public CommandApi(IApplicationService<Invoice> service) : base(service) { }

    [HttpPost]
    [Route("")]
    public Task<ActionResult<Result>> CreateInvoice([FromBody] CreateInvoice cmd, CancellationToken cancellationToken)
        => Handle(cmd, cancellationToken);


    [HttpPost]
    [Route("record-payment")]
    public Task<ActionResult<Result>> RecordPayment(
        [FromBody] RecordPayment cmd, CancellationToken cancellationToken
    )
        => Handle(cmd, cancellationToken);
}
