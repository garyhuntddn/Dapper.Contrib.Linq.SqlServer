using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace DDN.SharedCode
{
  public static class ICustomAttributeProviderExtensions
  {
    public static IEnumerable<T> GetCustomAttributes<T>( this ICustomAttributeProvider mi, bool inherit ) where T : Attribute
    {
      return mi.GetCustomAttributes( typeof( T ), inherit ).OfType<T>();
    }

    public static T GetCustomAttribute<T>( this ICustomAttributeProvider mi, bool inherit ) where T : Attribute
    {
      return mi.GetCustomAttributes<T>( inherit ).FirstOrDefault();
    }

    public static bool HasCustomAttribute<T>( this ICustomAttributeProvider mi, bool inherit ) where T : Attribute
    {
      return mi.GetCustomAttributes( typeof(T), inherit ).Any();
    }
  }
}
