using System.Linq;

namespace Gluten.Configuration
{
    public class Environments
    {
        public const string ENVIRONMENT_TEST = "test";
        public const string ENVIRONMENT_STAGING = "staging";
        public const string ENVIRONMENT_PRODUCTION = "production";

        public static readonly string[] _ENVIRONMENTS = new[]
        {
            ENVIRONMENT_TEST,
            ENVIRONMENT_STAGING,
            ENVIRONMENT_PRODUCTION,
        };

        public static bool Exists(string environment)
        {
            return _ENVIRONMENTS.Contains(environment);
        }
    }
}
