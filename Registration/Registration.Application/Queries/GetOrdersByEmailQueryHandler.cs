using Eventbox.Registration.Application.Abstractions;
using Eventbox.Registration.Application.Dtos;
using Mapster;
using MediatR;

namespace Eventbox.Registration.Application.Queries;

public class GetOrdersByEmailQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetOrdersByEmailQuery, IEnumerable<OrderDto>>
{
    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByEmailQuery request, CancellationToken cancellationToken)
        => (await unitOfWork.Orders.GetByEmailAsync(request.Email, cancellationToken)).Adapt<IEnumerable<OrderDto>>();
}
