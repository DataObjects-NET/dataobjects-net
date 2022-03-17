// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.09.25

using System;
using System.Data.Common;
using System.Security;
using Npgsql;
using NpgsqlTypes;

namespace Xtensive.Sql.Drivers.PostgreSql.v10_0
{
  internal class TypeMapper : v9_1.TypeMapper
  {
    // Constructors
    
    [SecuritySafeCritical]
    public override void BindDateTime(DbParameter parameter, object value)
    {
      var nativeParameter = (NpgsqlParameter) parameter;
      nativeParameter.NpgsqlDbType = NpgsqlDbType.Timestamp;
      
      if (value is DateTime {Kind: DateTimeKind.Utc} dateTime) {
        value = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
      }
      nativeParameter.NpgsqlValue = value ?? DBNull.Value;
    }

    public override SqlValueType MapDateTime(int? length, int? precision, int? scale) => new (SqlType.DateTime);

    public override object ReadDateTime(DbDataReader reader, int index) 
    {
      var nativeReader = (NpgsqlDataReader) reader;
      var value = nativeReader.GetFieldValue<DateTime>(index);
      return value;
    }

    public TypeMapper(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
