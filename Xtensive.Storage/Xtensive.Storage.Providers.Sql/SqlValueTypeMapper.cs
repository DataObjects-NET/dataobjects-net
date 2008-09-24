// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.23

using System;
using Xtensive.Core.Reflection;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Providers.Sql.Mappings;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlValueTypeMapper : InitializableHandlerBase
  {
    protected DomainHandler DomainHandler { get; private set; }

    public ValueTypeMappingSchema MappingSchema { get; private set; }

    /// <exception cref="InvalidOperationException">Type is not supported.</exception>
    public SqlValueType BuildSqlValueType(Model.ColumnInfo column)
    {
      int length = column.Length.HasValue ? column.Length.Value : 0;

      {
        DataTypeInfo dti = MappingSchema.GetExactMapping(column.ValueType);
        if (dti != null)
          return new SqlValueType(dti.SqlType, length);
      }

      DataTypeInfo[] ambigiousMappings = MappingSchema.GetAmbigiousMappings(column.ValueType);
      if (ambigiousMappings!=null) {
        foreach (DataTypeInfo dti in ambigiousMappings) {
          StreamDataTypeInfo sdti = dti as StreamDataTypeInfo;
          if (sdti == null)
            return new SqlValueType(dti.SqlType);

          if (length == 0)
            return new SqlValueType(sdti.SqlType, sdti.Length.MaxValue);

          if (sdti.Length.MaxValue < length)
            continue;

          return new SqlValueType(sdti.SqlType, length);
        }
      }
      throw new InvalidOperationException(string.Format("Type '{0}' is not supported.", column.ValueType.GetShortName()));
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      DomainHandler = Handlers.DomainHandler as DomainHandler;
      MappingSchema = new ValueTypeMappingSchema();
      BuildNativeTypeMappings();
      BuildTypeSubstituteMappings();
    }

    protected virtual void BuildTypeSubstituteMappings()
    {
    }

    protected virtual void BuildNativeTypeMappings()
    {
      DataTypeCollection types = DomainHandler.Driver.ServerInfo.DataTypes;
      Register(types.Boolean);
      Register(types.Byte);
      Register(types.DateTime);
      Register(types.Decimal);
      Register(types.Double);
      Register(types.Float);
      Register(types.Guid);
      Register(types.Int16);
      Register(types.Int32);
      Register(types.Int64);
      Register(types.Interval);
      Register(types.SByte);
      Register(types.UInt16);
      Register(types.UInt32);
      Register(types.UInt64);
      Register(types.VarBinary);
      Register(types.VarBinaryMax);
      Register(types.VarChar);
      Register(types.VarCharMax);
    }

    protected void Register(DataTypeInfo dataTypeInfo)
    {
      if (dataTypeInfo == null)
        return;

      MappingSchema.RegisterMapping(dataTypeInfo.Type, dataTypeInfo);
    }
  }
}