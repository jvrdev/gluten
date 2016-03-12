using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Gluten.Common
{
    public static class ObjectExtensions
    {
        public static bool IsEnumerable(this object @this)
        {
            return @this.GetType().Implements<IEnumerable>();
        }

        public static Dictionary<string, string> GetPublicPropertiesAsMetadata(this object @this)
        {
            var metadata = new Dictionary<string, string>();
            var type = @this.GetType();
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                var name = property.Name;
                var value = property.GetValue(@this, null);
                var serializedValue = Serialize(value);
                metadata[name] = serializedValue;
            }

            return metadata;
        }

        private static string Serialize(object value)
        {
            if (value == null) return null;
            if (value is string) return (string)value;
            if (value.IsEnumerable())
            {
                var builder = new StringBuilder();
                var list = (IEnumerable)value;
                foreach (var element in list)
                {
                    builder.Append(element + ", ");
                }
                if (builder.Length >= 2) builder.Remove(builder.Length - 2, 2);

                return builder.ToString();
            }

            return value.ToString();
        }
    }
}
