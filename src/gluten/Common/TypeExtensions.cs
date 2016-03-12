using System;
using System.Linq;

namespace Gluten.Common
{
    public static class TypeExtensions
    {
        public static bool Implements<TInterface>(this Type @this)
            where TInterface : class
        {
            return @this.GetInterfaces().Contains(typeof(TInterface));
        }
    }
}
