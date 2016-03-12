using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Configuration
{
    public class PrefixIndex
    {
        public static string GetResourcePrefix(string environment)
        {
            CheckEnvironment(environment);

            if (environment == Environments.ENVIRONMENT_STAGING)
            {
                return "stagin1_";
            }

            return environment + '_';
        }

        public static string GetDynamoDBTablePrefix(string environment)
        {
            CheckEnvironment(environment);

            if (environment == Environments.ENVIRONMENT_PRODUCTION)
            {
                return string.Empty;
            }
            // Test environment shares DB resources with Staging
            else if (environment == Environments.ENVIRONMENT_TEST)
            {
                return GetDynamoDBTablePrefix(Environments.ENVIRONMENT_STAGING);
            }

            return GetResourcePrefix(environment);
        }

        private static void CheckEnvironment(string environment)
        {
            if (!Environments.Exists(environment))
            {
                throw new ArgumentException("Environment does not exist.",
                    "environment");
            }
        }
    }
}
