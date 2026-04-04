using AccessiTrack.Application.Audits.Commands.StartAudit;
using AccessiTrack.Application.Audits.Queries.GetAuditsByProject;
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
}
