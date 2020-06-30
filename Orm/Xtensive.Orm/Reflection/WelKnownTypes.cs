using System;

namespace Xtensive.Reflection
{
  internal static class WellKnownTypes
  {
    public static readonly Type Object = typeof(object);
    public static readonly Type Array = typeof(Array);
    public static readonly Type Enum = typeof(Enum);

    public static readonly Type NullableOfT = typeof(Nullable<>);
  }
}