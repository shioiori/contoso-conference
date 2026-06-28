using Amazon.SimpleNotificationService;
using Amazon.SQS;

namespace EventBus.Aws
{
    public class AwsClient
    {
        private readonly IAmazonSQS _sqsClient;
        public IAmazonSQS SqsClient => _sqsClient;
        
        private readonly IAmazonSimpleNotificationService _snsClient;
        public IAmazonSimpleNotificationService SnsClient => _snsClient;

        private readonly AwsOptions _awsOptions;

        public AwsClient(AwsOptions awsOptions)
        {
            _awsOptions = awsOptions;
            _snsClient = new AmazonSimpleNotificationServiceClient(Amazon.RegionEndpoint.GetBySystemName(_awsOptions.Region));
            _sqsClient = new AmazonSQSClient(Amazon.RegionEndpoint.GetBySystemName(_awsOptions.Region));
        }
    }
}
