using Eventbox.Payment.Core.Responses;
using Mapster;
using PaymentEntity = Eventbox.Payment.Core.Entities.Payment;

namespace Eventbox.Payment.Core.Mappings;

public class PaymentMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PaymentEntity, PaymentIntentResponse>()
            .Map(dest => dest.PaymentIntentId, src => src.Id);
    }
}
