// Copyright (C) 2014-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alena Mikshina
// Created:    2014.04.09

using System;
using System.Data.Common;
using Npgsql;
using NpgsqlTypes;

namespace Xtensive.Sql.Drivers.PostgreSql
{
  internal abstract class PostgreSqlTypeMapper : CustomTypeMapper
  {
    private readonly Type type;
    private readonly NpgsqlDbType npgsqlDbType;
    private readonly SqlType sqlType;

    public override bool Enabled => type != null;

    public override Type Type => type;

    public override void BindValue(DbParameter parameter, object value)
    {
      if (value == null) {
        parameter.Value = DBNull.Value;
        return;
      }

      var npgsqlParameter = (NpgsqlParameter) parameter;
      npgsqlParameter.Value = value;
      npgsqlParameter.NpgsqlDbType = npgsqlDbType;
    }

    public override object ReadValue(DbDataReader reader, int index) => reader.GetValue(index);

    public override SqlValueType MapType(int? length, int? precision, int? scale) => new SqlValueType(sqlType);

    // Constructors

    protected PostgreSqlTypeMapper(string frameworkType, NpgsqlDbType npgsqlDbType, SqlType sqlType)
    {
      type = Type.GetType(frameworkType);

      this.npgsqlDbType = npgsqlDbType;
      this.sqlType = sqlType;
    }
  }
}
