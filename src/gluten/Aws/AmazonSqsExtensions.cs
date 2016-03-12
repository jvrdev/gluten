using System;
using System.Globalization;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Collections.Generic;
using System.Linq;
using Amazon.SQS.Util;

namespace Gluten.Aws
{
    public static class AmazonSqsExtensions
    {
        public const int MAX_MESSAGE_COUNT = 10;
        private const int _DEFAULT_VISIBILITY_TIMEOUT = 300;
        private const int _RECEIVE_MESSAGE_WAIT_TIME_SECONDS = 20;

        public static string GetQueueUrl(this IAmazonSQS @this, string queueName)
        {
            var request = new GetQueueUrlRequest { QueueName = queueName};
            try
            {
                var response = @this.GetQueueUrl(request);
                var queueArn = response.QueueUrl;

                return queueArn;
            }
            catch (AmazonSQSException e)
            {
                if (e.ErrorCode == "AWS.SimpleQueueService.NonExistentQueue")
                {
                    var message = string.Format("Queue with name {0} was not found.", queueName);
                    throw new KeyNotFoundException(message);
                }
                else
                {
                    throw;
                }
            }
        }

        public static string GetQueueArn(this IAmazonSQS @this, string queueUrl)
        {
            var request = new GetQueueAttributesRequest()
            {
                QueueUrl = queueUrl,
                AttributeNames = { SQSConstants.ATTRIBUTE_QUEUE_ARN },
            };

            var response = @this.GetQueueAttributes(request);

            var queueArn = response.QueueARN;

            return queueArn;
        }

        public static string CreateQueue(this IAmazonSQS @this, string queueName)
        {
            var attribute = new Dictionary<string, string>
            {
                {
                    "ReceiveMessageWaitTimeSeconds",
                    _RECEIVE_MESSAGE_WAIT_TIME_SECONDS.ToString(CultureInfo.InvariantCulture)
                },
                {
                    "VisibilityTimeout ",
                    _DEFAULT_VISIBILITY_TIMEOUT.ToString(CultureInfo.InvariantCulture)
                }
            };

            var request = new CreateQueueRequest
            {
                QueueName = queueName,
                Attributes = attribute,
            };
           
            var response = @this.CreateQueue(request);
            var queueUrl = response.QueueUrl;

            return queueUrl;
        }

        public static void SendMessage(this IAmazonSQS @this, string message, string queueUrl)
        {
            var request = new SendMessageRequest
            {
                MessageBody = message,
                QueueUrl = queueUrl,
            };
            var response = @this.SendMessage(request);
        }

        public static void SendDelayedMessage(this IAmazonSQS @this, string message, string queueUrl, int delaySeconds)
        {
            if (delaySeconds < 0 || delaySeconds > AwsConstants.MAX_MESSAGE_DELAY_SECONDS)
            {
                var exceptionMessage = string.Format("Delay must be among 0 and {0} seconds.", delaySeconds);
                throw new ArgumentOutOfRangeException("delaySeconds", exceptionMessage);
            }

            var request = new SendMessageRequest
            {
                MessageBody = message,
                QueueUrl = queueUrl,
                DelaySeconds = delaySeconds,
            };
            var response = @this.SendMessage(request);
        }

        /// <summary>
        /// Receives the message contents directly, trying only once
        /// and throwing timeout after a single attempt (20 seconds with current settings).
        /// This makes impossible deleteing the message from the queue unless
        /// the queue is deleted.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="queueUrl"></param>
        /// <returns></returns>
        public static string ReceiveMessageUnsafe(this IAmazonSQS @this, string queueUrl)
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl
            };
            var response = @this.ReceiveMessage(request);

            if (response.Messages.Count == 0)
            {
                throw new TimeoutException("No messages were received after a single long-poll operation.");
            }
            var messageBody = response.Messages[0].Body;

            return messageBody;
        }

        private static Message ReceiveMessagePolling(this IAmazonSQS @this, string queueUrl)
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
            };
            ReceiveMessageResponse response;
            do
            {
                response = @this.ReceiveMessage(request);
            }
            while (response.Messages.Count > 0);

            var message = response.Messages[0];

            return message;
        }

        public static void DeleteMessages(this IAmazonSQS @this, IEnumerable<Message> messages, string queueUrl)
        {
            var entries = messages
                .Select((m, i) =>
                    new DeleteMessageBatchRequestEntry
                    {
                        Id = i.ToString(CultureInfo.InvariantCulture),
                        ReceiptHandle = m.ReceiptHandle,
                    })
                .ToList();

            var request = new DeleteMessageBatchRequest
            {
                QueueUrl = queueUrl,
                Entries = entries,
            };
            var response = @this.DeleteMessageBatch(request);
        }

        public static void DeleteMessage(this IAmazonSQS @this, Message message, string queueUrl)
        {
            @this.DeleteMessage(message.ReceiptHandle, queueUrl);
        }

        public static void DeleteMessage(this IAmazonSQS @this, string receiptHandle, string queueUrl)
        {
            var request = new DeleteMessageRequest
            {
                ReceiptHandle = receiptHandle,
                QueueUrl = queueUrl,
            };
            var response = @this.DeleteMessage(request);
        }

        public static Message[] ReceiveMessages(this IAmazonSQS @this, string queueUrl)
        {
            var request = new ReceiveMessageRequest
            {
                MaxNumberOfMessages = MAX_MESSAGE_COUNT,
                QueueUrl = queueUrl,
            };
            var response = @this.ReceiveMessage(request);

            return response.Messages.ToArray();
        }

        public static void SetQueueAttributesRequest(this IAmazonSQS @this, string queueUrl, string policy)
        {
            var attribute = new Dictionary<string, string>
            {
                {
                    SQSConstants.ATTRIBUTE_POLICY,
                    policy
                }
            };

            var request = new SetQueueAttributesRequest
            {
                QueueUrl = queueUrl,
                Attributes = attribute,
            };
            var response = @this.SetQueueAttributes(request);
        }

        public static string[] ListQueues(this IAmazonSQS @this, string prefix)
        {
            var request = new ListQueuesRequest { QueueNamePrefix = prefix };
            var response = @this.ListQueues(request);
            var queueUrls = response.QueueUrls.ToArray();

            return queueUrls;
        }

        public static void DeleteQueue(this IAmazonSQS @this, string queueUrl)
        {
            var request = new DeleteQueueRequest
            {
                QueueUrl = queueUrl,
            };
            var response = @this.DeleteQueue(request);
        }

        /*public static void ChangeMessageVisibility(this AmazonSQS @this, string[] receiptHandles, string queueUrl)
        {
            var entries = receiptHandles
                .Select(r => new ChangeMessageVisibilityBatchRequestEntry()
                    .WithId(Guid.NewGuid().ToString())
                    .WithReceiptHandle(r)
                    .WithVisibilityTimeout(0))
                .ToArray();
            var request = new ChangeMessageVisibilityBatchRequest()
                .WithEntries(entries)
                .WithQueueUrl(queueUrl);
            var response = @this.ChangeMessageVisibilityBatch(request);
        }*/
    }
}
