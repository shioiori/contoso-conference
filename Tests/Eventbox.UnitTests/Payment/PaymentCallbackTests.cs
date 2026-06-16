using Eventbox.Payment.Core.Abstractions;
using Eventbox.Payment.Core.Commands;
using Eventbox.Payment.Core.Enums;
using Eventbox.Shared.Outbox;
using PaymentEntity = Eventbox.Payment.Core.Entities.Payment;

namespace Eventbox.UnitTests.Payment;

public class PaymentCallbackTests
{
    [Fact]
    public async Task SimulatePaymentSucceeded_WhenProviderEventIsDuplicate_DoesNotProcessOrPublishTwice()
    {
        var repository = new InMemoryPaymentRepository();
        var unitOfWork = new InMemoryPaymentUnitOfWork(repository);
        var outbox = new InMemoryOutbox();
        var publisher = new RecordingPaymentEventPublisher();
        var payment = PaymentEntity.CreateIntent(Guid.NewGuid(), 25m, "usd", null, null, "checkout-1");
        await repository.AddAsync(payment, CancellationToken.None);
        var handler = new SimulatePaymentSucceededCommandHandler(unitOfWork, outbox, publisher);
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
        Assert.Equal(2, unitOfWork.SaveChangesCount);
        var outboxMessage = Assert.Single(outbox.OutboxMessages);
        Assert.Equal(ProcessStatus.Processed, outboxMessage.Status);
        Assert.NotNull(outboxMessage.ProcessedOnUtc);
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
    }

    private sealed class InMemoryPaymentUnitOfWork(IPaymentRepository paymentRepository) : IUnitOfWork
    {
        public int SaveChangesCount { get; private set; }

        public IPaymentRepository Payments { get; } = paymentRepository;

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class InMemoryOutbox : IOutbox
    {
        private readonly List<OutboxMessage> _outboxMessages = new();

        public IReadOnlyCollection<OutboxMessage> OutboxMessages => _outboxMessages.AsReadOnly();

        public Task AddOutboxMessageAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
        {
            _outboxMessages.Add(outboxMessage);
            return Task.CompletedTask;
        }

        public Task UpdateOutboxMessageAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class RecordingPaymentEventPublisher : IPaymentEventPublisher
    {
        private readonly List<string> _publishedProviderEventIds = new();

        public IReadOnlyCollection<string> PublishedProviderEventIds => _publishedProviderEventIds.AsReadOnly();

        public Task<bool> PublishPaymentConfirmedAsync(
            PaymentEntity payment,
            string providerEventId,
            DateTimeOffset paidAt,
            CancellationToken cancellationToken)
        {
            _publishedProviderEventIds.Add(providerEventId);
            return Task.FromResult(true);
        }
    }
}
