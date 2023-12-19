// Copyright (C) 2020-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Orm.Linq;

namespace Xtensive.Reflection
{
  internal static class WellKnownTypes
  {
    public static readonly Type Object = typeof(object);
    public static readonly Type Array = typeof(Array);
    public static readonly Type Enum = typeof(Enum);
    public static readonly Type Void = typeof(void);
    public static readonly Type Delegate = typeof(Delegate);

    public static readonly Type Bool = typeof(bool);
    public static readonly Type NullableBool = typeof(bool?);

    public static readonly Type Byte = typeof(byte);
    public static readonly Type NullableByte = typeof(byte?);
    public static readonly Type SByte = typeof(sbyte);
    public static readonly Type NullableSByte = typeof(sbyte?);
    public static readonly Type Int16 = typeof(short);
    public static readonly Type NullableInt16 = typeof(short?);
    public static readonly Type UInt16 = typeof(ushort);
    public static readonly Type NullableUInt16 = typeof(ushort?);
    public static readonly Type Int32 = typeof(int);
    public static readonly Type NullableInt32 = typeof(int?);
    public static readonly Type UInt32 = typeof(uint);
    public static readonly Type NullableUInt32 = typeof(uint?);
    public static readonly Type Int64 = typeof(long);
    public static readonly Type NullableInt64 = typeof(long?);
    public static readonly Type UInt64 = typeof(ulong);
    public static readonly Type NullableUInt64 = typeof(ulong?);

    public static readonly Type Char = typeof(char);
    public static readonly Type NullableChar = typeof(char?);

    public static readonly Type Decimal = typeof(decimal);
    public static readonly Type NullableDecimal = typeof(decimal?);
    public static readonly Type Single = typeof(float);
    public static readonly Type NullableSingle = typeof(float?);
    public static readonly Type Double = typeof(double);
    public static readonly Type NullableDouble = typeof(double?);

    public static readonly Type String = typeof(string);

    public static readonly Type TimeSpan = typeof(TimeSpan);
    public static readonly Type NullableTimeSpan = typeof(TimeSpan?);
    public static readonly Type DateTime = typeof(DateTime);
    public static readonly Type NullableDateTime = typeof(DateTime?);
    public static readonly Type DateTimeOffset = typeof(DateTimeOffset);
    public static readonly Type NullableDateTimeOffset = typeof(DateTimeOffset?);
    public static readonly Type DateOnly = typeof(DateOnly);
    public static readonly Type TimeOnly = typeof(TimeOnly);
    public static readonly Type NullableDateOnly = typeof(DateOnly?);
    public static readonly Type NullableTimeOnly = typeof(TimeOnly?);

    public static readonly Type Guid = typeof(Guid);
    public static readonly Type NullableGuid = typeof(Guid?);

    public static readonly Type NullableOfT = typeof(Nullable<>);

    public static readonly Type Queryable = typeof(Queryable);
    public static readonly Type QueryableOfT = typeof(Queryable<>);

    public static readonly Type Enumerable = typeof(Enumerable);

    public static readonly Type Expression = typeof(Expression);
    public static readonly Type ExpressionOfT = typeof(Expression<>);

    public static readonly Type FuncOfTArgTResultType = typeof(Func<,>);

    public static readonly Type ByteArray = typeof(byte[]);
    public static readonly Type ObjectArray = typeof(object[]);

    public static readonly Type DefaultMemberAttribute = typeof(DefaultMemberAttribute);
  }
}