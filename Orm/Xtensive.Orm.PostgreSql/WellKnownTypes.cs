// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using NpgsqlTypes;

namespace Xtensive.Reflection.PostgreSql
{
  internal static class WellKnownTypes
  {
    public static readonly Type DateTimeOffsetType = typeof(DateTimeOffset);
    public static readonly Type TimeSpanType = typeof(TimeSpan);
    public static readonly Type GuidType = typeof(Guid);
    public static readonly Type ByteArrayType = typeof(byte[]);
    public static readonly Type StringType = typeof(string);
    public static readonly Type DecimalType = typeof(decimal);

    public static readonly Type NpgsqlPointType = typeof(NpgsqlPoint);
    public static readonly Type NpgsqlLSegType = typeof(NpgsqlLSeg);
    public static readonly Type NpgsqlBoxType = typeof(NpgsqlBox);
    public static readonly Type NpgsqlPathType = typeof(NpgsqlPath);
    public static readonly Type NpgsqlPolygonType = typeof(NpgsqlPolygon);
    public static readonly Type NpgsqlCircleType = typeof(NpgsqlCircle);
  }
}
