using System;

namespace Xtensive.Orm
{
  public static partial class WellKnown
  {
    public static class Types
    {
      public static readonly Type ByteType = typeof(byte);
      public static readonly Type NullableByteType = typeof(byte?);
      public static readonly Type SByteType = typeof(sbyte);
      public static readonly Type NullableSByteType = typeof(sbyte?);
      public static readonly Type Int16Type = typeof(short);
      public static readonly Type NullableInt16Type = typeof(short?);
      public static readonly Type UInt16Type = typeof(ushort);
      public static readonly Type NullableUInt16Type = typeof(ushort?);
      public static readonly Type Int32Type = typeof(int);
      public static readonly Type NullableInt32Type = typeof(int?);
      public static readonly Type UInt32Type = typeof(uint);
      public static readonly Type NullableUInt32Type = typeof(uint?);
      public static readonly Type Int64Type = typeof(long);
      public static readonly Type NullableInt64Type = typeof(long?);
      public static readonly Type UInt64Type = typeof(ulong);
      public static readonly Type NullableUInt64Type = typeof(ulong?);

      public static readonly Type CharType = typeof(char);
      public static readonly Type NullableCharType = typeof(char?);

      public static readonly Type DecimalType = typeof(decimal);
      public static readonly Type NullableDecimalType = typeof(decimal?);
      public static readonly Type FloatType = typeof(float);
      public static readonly Type NullableFloatType = typeof(float?);
      public static readonly Type DoubleType = typeof(double);
      public static readonly Type NullableDoubleType = typeof(double?);

      public static readonly Type StringType = typeof(string);

      public static readonly Type TimeSpanType = typeof(TimeSpan);
      public static readonly Type NullableTimeSpanType = typeof(TimeSpan?);
      public static readonly Type DateTimeType = typeof(DateTime);
      public static readonly Type NullableDateTimeType = typeof(DateTime?);
      public static readonly Type DateTimeOffsetType = typeof(DateTimeOffset);
      public static readonly Type NullableDateTimeOffsetType = typeof(DateTimeOffset?);
      public static readonly Type GuidType = typeof(Guid);
      public static readonly Type NullableGuidType = typeof(Guid?);

      public static readonly Type NullableType = typeof(Nullable<>);
    }
  }
}