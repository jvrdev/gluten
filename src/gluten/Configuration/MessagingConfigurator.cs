
namespace Gluten.Configuration
{
    public class MessagingConfigurator
    {
        #region Singleton
        public static MessagingConfigurator Instance { get; private set; }

        static MessagingConfigurator()
        {
            Instance = new MessagingConfigurator();
        }
        #endregion

        public IEndpointConfigurationExpression ConfigureEndpoint()
        {
            var expression = new EndpointConfigurationExpression();

            return expression;
        }
    }
}
