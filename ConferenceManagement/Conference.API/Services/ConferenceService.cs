using Conference.API.IntegrationEvents;
using Conference.API.Repositories.Abstractions;
using Conference.API.Services.Abstractions;
using Contoso.ServiceBus.Abstractions;
using System.Security.Cryptography;

namespace Conference.API.Services
{
    public class ConferenceService : IConferenceService
    {
        private readonly IConferenceRepository _repository;
        private readonly IEventBus _eventBus;

        public ConferenceService(IConferenceRepository repository, IEventBus eventBus)
        {
            _repository = repository;
            _eventBus = eventBus;
        }

        public Task<Domains.Conference?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _repository.GetByIdAsync(id, cancellationToken);

        public Task<Domains.Conference?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
            => _repository.GetBySlugAsync(slug, cancellationToken);

        public Task<IEnumerable<Domains.Conference>> GetAllAsync(CancellationToken cancellationToken = default)
            => _repository.GetAllAsync(cancellationToken);

        public Task<IEnumerable<Domains.Conference>> GetPublishedAsync(CancellationToken cancellationToken = default)
            => _repository.GetPublishedAsync(cancellationToken);

        public async Task<Domains.Conference> CreateAsync(string name, string slug, DateTime startDate, DateTime endDate, string? description = null, CancellationToken cancellationToken = default)
        {
            var existing = await _repository.GetBySlugAsync(slug, cancellationToken);
            if (existing is not null)
                throw new InvalidOperationException($"A conference with slug '{slug}' already exists.");

            var accessCode = GenerateAccessCode();
            var conference = new Domains.Conference(Guid.NewGuid(), name, slug, startDate, endDate, description, accessCode);
            await _repository.AddAsync(conference, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            await _eventBus.PublishAsync(new ConferenceCreatedEvent
            {
                ConferenceId = conference.Id,
                Name = conference.Name,
                Slug = conference.Slug,
                Description = conference.Description,
                StartDate = conference.StartDate,
                EndDate = conference.EndDate,
                AccessCode = conference.AccessCode!,
            }, cancellationToken);

            return conference;
        }

        public async Task<Domains.Conference> UpdateAsync(Guid id, string name, DateTime startDate, DateTime endDate, string? description = null, CancellationToken cancellationToken = default)
        {
            var conference = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Conference '{id}' not found.");

            conference.Update(name, startDate, endDate, description);
            await _repository.SaveChangesAsync(cancellationToken);

            await _eventBus.PublishAsync(new ConferenceUpdatedEvent
            {
                ConferenceId = conference.Id,
                Name = conference.Name,
                Description = conference.Description,
                StartDate = conference.StartDate,
                EndDate = conference.EndDate,
            }, cancellationToken);

            return conference;
        }

        public async Task SetVisibilityAsync(Guid id, bool isPublished, CancellationToken cancellationToken = default)
        {
            var conference = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Conference '{id}' not found.");

            if (isPublished)
            {
                conference.Publish();
                await _repository.SaveChangesAsync(cancellationToken);
                await _eventBus.PublishAsync(new ConferencePublishedEvent
                {
                    ConferenceId = conference.Id,
                    Name = conference.Name,
                    Slug = conference.Slug,
                }, cancellationToken);
            }
            else
            {
                conference.Unpublish();
                await _repository.SaveChangesAsync(cancellationToken);
                await _eventBus.PublishAsync(new ConferenceUnpublishedEvent
                {
                    ConferenceId = conference.Id,
                    Name = conference.Name,
                    Slug = conference.Slug,
                }, cancellationToken);
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var conference = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Conference '{id}' not found.");

            if (conference.IsPublished)
                throw new InvalidOperationException($"Cannot delete a published conference. Unpublish it first.");

            _repository.Delete(conference);
            await _repository.SaveChangesAsync(cancellationToken);
        }

        private static string GenerateAccessCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Range(0, 5)
                .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)])
                .ToArray());
        }
    }
}
