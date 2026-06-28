using Eventbox.EventBus.Core.Abstractions;

namespace EventBus.Aws
{
    public class SnsMapping
    {
        public Type EventType { get; set; }
        public string TopicArn { get; set; }
    }

    public class SqsMapping
    {
        public Type EventType { get; set; }
        public string QueueUrl { get; set; }
        public string DlqUrl { get; set; }
    }

    public class AwsOptions
    {
        public string Region { get; set; }
        public List<SnsMapping> PublishMappings { get; } = new();
        public List<SqsMapping> SubscribeMappings { get; } = new();

        public AwsOptions Publish<TEvent>(string topicArn) where TEvent : IIntegrationEvent
        {
            PublishMappings.Add(new SnsMapping
            {
                EventType = typeof(TEvent),
                TopicArn = topicArn
            });
            return this;
        }

        public AwsOptions Subscribe<TEvent>(string queueUrl, string dlqUrl) where TEvent : IIntegrationEvent
        {
            SubscribeMappings.Add(new SqsMapping
            {
                EventType = typeof(TEvent),
                QueueUrl = queueUrl,
                DlqUrl = dlqUrl
            });
            return this;
        }
    }
}
