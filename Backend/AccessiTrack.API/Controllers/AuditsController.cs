using AccessiTrack.Application.Audits.Commands.CompleteAudit;
using AccessiTrack.Application.Audits.Commands.StartAudit;
using AccessiTrack.Application.Audits.Commands.StartAutomaticAudit;
using AccessiTrack.Application.Audits.Queries.GetAuditsByProject;
using AccessiTrack.Application.Audits.Queries.GetAuditById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccessiTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("project/{projectId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<AuditDto>), 200)]
    public async Task<IActionResult> GetByProject(
        Guid projectId, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetAuditsByProjectQuery(projectId), ct);
        return Ok(result);
    }

    [HttpGet("{auditId:guid}")]
    [ProducesResponseType(typeof(AuditDetailsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(
        Guid auditId, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetAuditByIdQuery(auditId), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    public async Task<IActionResult> Start(
        [FromBody] StartAuditCommand command,
        CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetByProject),
            new { projectId = command.ProjectId }, id);
    }

    /// <summary>
    /// Starts an automatic accessibility audit for a project.
    /// Fetches the project's TargetUrl and scans for WCAG violations.
    /// </summary>
    [HttpPost("automatic")]
    [ProducesResponseType(typeof(StartAutomaticAuditResult), 200)]
    public async Task<IActionResult> StartAutomatic(
        [FromBody] StartAutomaticAuditCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPatch("{auditId:guid}/complete")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Complete(
    Guid auditId, CancellationToken ct)
    {
        await _mediator.Send(new CompleteAuditCommand(auditId), ct);
        return NoContent();
    }
}
