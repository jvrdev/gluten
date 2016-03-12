using Gluten.Common;
using System.Linq;
using System.Text;

namespace Gluten.Messages
{
    public static class IMessageExtensions
    {
        public static string ToPretty(this IMessage message)
        {
            var values = message.GetPublicPropertiesAsMetadata();
            var preface = string.Format("message of type {0} with values: ",
                message.GetType());
            var pretty = values.Aggregate(new StringBuilder(preface),
                (s, p) => s.AppendFormat("{0}={1},", p.Key, p.Value),
                s => s.ToString().Substring(0, s.Length - 1));

            return pretty;
        }
    }
}
