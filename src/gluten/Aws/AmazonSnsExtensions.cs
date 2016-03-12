using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Collections.Generic;
using System.Linq;

namespace Gluten.Aws
{
    public static class AmazonSnsExtensions
    {
        public static string CreateTopic(this IAmazonSimpleNotificationService @this, string name)
        {
            var request = new CreateTopicRequest { Name = name };
            var response = @this.CreateTopic(request);
            var topicArn = response.TopicArn;

            return topicArn;
        }

        public static void Publish(this IAmazonSimpleNotificationService @this, string message, string topic)
        {
            var request = new PublishRequest
            {
                Message = message,
                TopicArn = topic
            };
            var response = @this.Publish(request);
        }

        public static string SubscribeQueueToTopic(
            this IAmazonSimpleNotificationService @this, string queueArn,
            string topic)
        {
            var request = new SubscribeRequest
            {
                Endpoint = queueArn,
                Protocol = "sqs",
                TopicArn = topic,
            };
            var response = @this.Subscribe(request);
            var subscriptionArn = response.SubscriptionArn;

            return subscriptionArn;
        }

        public static string[] ListTopics(this IAmazonSimpleNotificationService @this)
        {
            var arns = new List<string>();

            ListTopicsResponse response;
            for (var token = string.Empty; token != null;
                token = response.NextToken)
            {
                var request = CreateListTopicRequest(token);
                response = @this.ListTopics(request);
                arns.AddRange(response.Topics.Select(t => t.TopicArn));
            }

            return arns.ToArray();
        }

        public static void ActivateRawMessageDelivery(this IAmazonSimpleNotificationService @this,
            string subscriptionArn)
        {
            var request = new SetSubscriptionAttributesRequest
            {
                SubscriptionArn = subscriptionArn,
                AttributeName = "RawMessageDelivery",
                AttributeValue = "true",
            };
            var response = @this.SetSubscriptionAttributes(request);
        }

        private static ListTopicsRequest CreateListTopicRequest(string nextToken)
        {
            var request = new ListTopicsRequest();
            if (!string.IsNullOrEmpty(nextToken))
            {
                request.NextToken = nextToken;
            }

            return request;
        }
    }
}
