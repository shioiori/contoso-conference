using Eventbox.Payment.Core.Dtos;
using Mapster;
using PaymentEntity = Eventbox.Payment.Core.Entities.Payment;

namespace Eventbox.Payment.Core.Mappings;

public class PaymentMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PaymentEntity, PaymentIntentDto>()
            .Map(dest => dest.PaymentIntentId, src => src.Id);
    }
}
