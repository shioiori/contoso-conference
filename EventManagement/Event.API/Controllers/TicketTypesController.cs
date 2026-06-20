using Eventbox.EventManagement.EventApi.Application.Abstractions.Repositories;
using Eventbox.EventManagement.EventApi.Dtos;
using Eventbox.EventManagement.EventApi.Domains;
using Eventbox.EventManagement.EventApi.Requests;
using Eventbox.EventManagement.EventApi.Services.Abstractions;
using Eventbox.Shared.Exceptions;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Eventbox.EventManagement.EventApi.Controllers
{
    [ApiController]
    [Authorize(Policy = "RequireOrganizerAccount")]
    [Route("api/organizations/{organizationId:guid}/events/{eventId:guid}/ticket-types")]
    public class TicketTypesController : ControllerBase
    {
        private readonly ITicketTypeService _ticketTypeService;
        private readonly IOrganizationMemberRepository _memberRepository;

        public TicketTypesController(ITicketTypeService ticketTypeService, IOrganizationMemberRepository memberRepository)
        {
            _ticketTypeService = ticketTypeService;
            _memberRepository = memberRepository;
        }

        private static string? HashAccessCode(string? accessCode)
        {
            if (string.IsNullOrWhiteSpace(accessCode)) return null;
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(accessCode.Trim()));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private async Task EnsureMemberAsync(Guid organizationId, CancellationToken cancellationToken)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                throw new ForbiddenApiException("Access denied.");

            var isMember = await _memberRepository.ExistsAsync(organizationId, userId, cancellationToken);
            if (!isMember)
                throw new ForbiddenApiException("You are not a member of this organization.");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketTypeDto>>> GetByEvent(
            Guid organizationId,
            Guid eventId,
            CancellationToken cancellationToken)
        {
            await EnsureMemberAsync(organizationId, cancellationToken);
            var ticketTypes = await _ticketTypeService.GetByEventIdAsync(organizationId, eventId, cancellationToken);
            return Ok(ticketTypes.Adapt<IEnumerable<TicketTypeDto>>());
        }

        [HttpPost]
        public async Task<ActionResult<TicketTypeDto>> Create(
            Guid organizationId,
            Guid eventId,
            [FromBody] CreateTicketTypeRequest request,
            CancellationToken cancellationToken)
        {
            await EnsureMemberAsync(organizationId, cancellationToken);
            var ticketType = new TicketType(
                request.Name,
                eventId,
                request.Quota,
                request.Description,
                request.Currency,
                request.MinPerOrder,
                request.MaxPerOrder,
                request.Visibility,
                HashAccessCode(request.AccessCode),
                request.PricingPhases.Select(phase => new PricingPhase(
                    phase.Name,
                    phase.Price,
                    phase.StartTime,
                    phase.EndTime)));

            ticketType = await _ticketTypeService.CreateAsync(organizationId, ticketType, cancellationToken);
            return CreatedAtAction(nameof(GetByEvent), new { organizationId, eventId = ticketType.EventId }, ticketType.Adapt<TicketTypeDto>());
        }

        [HttpPut("{ticketTypeId:guid}")]
        public async Task<ActionResult<TicketTypeDto>> Update(
            Guid organizationId,
            Guid eventId,
            Guid ticketTypeId,
            [FromBody] UpdateTicketTypeRequest request,
            CancellationToken cancellationToken)
        {
            await EnsureMemberAsync(organizationId, cancellationToken);

            var stub = new TicketType(
                request.Name,
                eventId,
                request.Quota,
                request.Description,
                request.Currency,
                request.MinPerOrder,
                request.MaxPerOrder,
                request.Visibility,
                HashAccessCode(request.AccessCode),
                request.PricingPhases.Select(p => new PricingPhase(p.Name, p.Price, p.StartTime, p.EndTime)));

            var ticketType = await _ticketTypeService.UpdateAsync(organizationId, eventId, ticketTypeId, stub, cancellationToken);
            return Ok(ticketType.Adapt<TicketTypeDto>());
        }

        [HttpPost("{ticketTypeId:guid}/capacity")]
        public async Task<ActionResult<TicketTypeDto>> AddCapacity(
            Guid organizationId,
            Guid eventId,
            Guid ticketTypeId,
            [FromBody] AddCapacityRequest request,
            CancellationToken cancellationToken)
        {
            await EnsureMemberAsync(organizationId, cancellationToken);
            var ticketType = await _ticketTypeService.AddCapacityAsync(organizationId, eventId, ticketTypeId, request.Quantity, cancellationToken);
            return Ok(ticketType.Adapt<TicketTypeDto>());
        }

        [HttpGet("{ticketTypeId:guid}/availability")]
        public async Task<ActionResult<TicketTypeDto>> GetAvailability(
            Guid organizationId,
            Guid eventId,
            Guid ticketTypeId,
            CancellationToken cancellationToken)
        {
            await EnsureMemberAsync(organizationId, cancellationToken);
            var ticketType = await _ticketTypeService.GetByIdAsync(organizationId, eventId, ticketTypeId, cancellationToken);
            if (ticketType is null)
                throw new NotFoundException("TicketType", ticketTypeId);

            return Ok(ticketType.Adapt<TicketTypeDto>());
        }
    }
}
