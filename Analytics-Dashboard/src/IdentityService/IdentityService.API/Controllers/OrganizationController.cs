using IdentityService.Application.Dtos.Common;
using IdentityService.Application.Dtos.Organizations;
using IdentityService.Application.Features.Organizations.Commands;
using IdentityService.Application.Features.Organizations.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Authorization.Attributes;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("organization")]
/// <summary>
/// Handles HTTP requests for organization CRUD operations.
/// </summary>
public  class OrganizationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrganizationController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrganizationController"/> class.
    /// </summary>
    public OrganizationController(IMediator mediator, ILogger<OrganizationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all organizations.
    /// </summary>
    [HttpGet]
    [Authorize]
    [FeatureAuthorize("ORG_MANAGE")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all organizations");
        List<OrganizationResponseDto> result = await _mediator.Send(new GetAllOrganizationsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves an organization by its unique identifier.
    /// </summary>
    [HttpGet("{id:long}")]
    [Authorize]
    [FeatureAuthorize("ORG_MANAGE")]
    public async Task<IActionResult> GetById(Int64 id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting organization by id {OrganizationId}", id);
        OrganizationResponseDto result = await _mediator.Send(new GetOrganizationQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new organization along with its admin user account.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateOrganizationRequestDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating organization with name {OrganizationName}", dto.OrganizationName);
        IdResponseDto result = await _mediator.Send(new CreateOrganizationCommand(dto), cancellationToken);
        return Created(String.Empty, result);
    }

    /// <summary>
    /// Updates an existing organization's details.
    /// </summary>
    [HttpPut("{id:long}")]
    [Authorize]
    [FeatureAuthorize("ORG_MANAGE")]
    public async Task<IActionResult> Update(Int64 id, [FromBody] CreateOrganizationRequestDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating organization {OrganizationId}", id);
        IdResponseDto result = await _mediator.Send(new UpdateOrganizationCommand(id, dto), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes an organization by its unique identifier.
    /// </summary>
    [HttpDelete("{id:long}")]
    [Authorize]
    [FeatureAuthorize("ORG_MANAGE")]
    public async Task<IActionResult> Delete(Int64 id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting organization {OrganizationId}", id);
        String result = await _mediator.Send(new DeleteOrganizationCommand(id), cancellationToken);
        return Ok(result);
    }
}
