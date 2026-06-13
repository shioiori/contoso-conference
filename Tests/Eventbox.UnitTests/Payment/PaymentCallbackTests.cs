using Eventbox.Payment.Core.Abstractions;
using Eventbox.Payment.Core.Commands;
using Eventbox.Payment.Core.Enums;
using PaymentEntity = Eventbox.Payment.Core.Entities.Payment;

namespace Eventbox.UnitTests.Payment;

public class PaymentCallbackTests
{
    [Fact]
    public async Task SimulatePaymentSucceeded_WhenProviderEventIsDuplicate_DoesNotProcessOrPublishTwice()
    {
        var repository = new InMemoryPaymentRepository();
        var publisher = new RecordingPaymentEventPublisher();
        var payment = PaymentEntity.CreateIntent(Guid.NewGuid(), 25m, "usd", null, null, "checkout-1");
        await repository.AddAsync(payment, CancellationToken.None);
        var handler = new SimulatePaymentSucceededCommandHandler(repository, publisher);
        var command = new SimulatePaymentSucceededCommand(
            payment.Id,
            ProviderEventId: "evt_123",
            payment.OrderId,
            payment.Amount,
            payment.Currency,
            DateTimeOffset.UtcNow);

        var first = await handler.Handle(command, CancellationToken.None);
        var duplicate = await handler.Handle(command, CancellationToken.None);

        Assert.True(first.Processed);
        Assert.False(duplicate.Processed);
        Assert.Equal(PaymentStatus.Succeeded, duplicate.Status);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
        Assert.Equal(1, repository.SaveChangesCount);
        Assert.Single(publisher.PublishedProviderEventIds);
    }

    [Fact]
    public void MarkSucceeded_WhenSameCallbackIsAppliedTwice_IsIdempotent()
    {
        var paidAt = DateTimeOffset.UtcNow;
        var payment = PaymentEntity.CreateIntent(Guid.NewGuid(), 25m, "usd", null, null, null);

        var first = payment.MarkSucceeded("evt_123", payment.OrderId, 25m, "USD", paidAt);
        var duplicate = payment.MarkSucceeded("evt_123", payment.OrderId, 25m, "USD", paidAt.AddSeconds(1));

        Assert.True(first);
        Assert.False(duplicate);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
        Assert.Equal(paidAt, payment.CompletedAt);
    }

    [Fact]
    public void MarkSucceeded_WhenAmountDiffers_RejectsCallback()
    {
        var payment = PaymentEntity.CreateIntent(Guid.NewGuid(), 25m, "usd", null, null, null);

        var exception = Assert.Throws<InvalidOperationException>(
            () => payment.MarkSucceeded("evt_123", payment.OrderId, 30m, "USD", DateTimeOffset.UtcNow));

        Assert.Equal("Payment callback amount does not match the payment intent.", exception.Message);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    private sealed class InMemoryPaymentRepository : IPaymentRepository
    {
        private readonly List<PaymentEntity> _payments = new();

        public int SaveChangesCount { get; private set; }

        public Task<PaymentEntity?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken)
            => Task.FromResult(_payments.FirstOrDefault(payment => payment.Id == paymentId));

        public Task<PaymentEntity?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken)
            => Task.FromResult(_payments.FirstOrDefault(payment => payment.IdempotencyKey == idempotencyKey));

        public Task<PaymentEntity?> GetByProviderEventIdAsync(string providerEventId, CancellationToken cancellationToken)
            => Task.FromResult(_payments.FirstOrDefault(payment => payment.ProviderEventId == providerEventId));

        public Task AddAsync(PaymentEntity payment, CancellationToken cancellationToken)
        {
            _payments.Add(payment);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingPaymentEventPublisher : IPaymentEventPublisher
    {
        private readonly List<string> _publishedProviderEventIds = new();

        public IReadOnlyCollection<string> PublishedProviderEventIds => _publishedProviderEventIds.AsReadOnly();

        public Task PublishPaymentConfirmedAsync(
            PaymentEntity payment,
            string providerEventId,
            DateTimeOffset paidAt,
            CancellationToken cancellationToken)
        {
            _publishedProviderEventIds.Add(providerEventId);
            return Task.CompletedTask;
        }
    }
}
