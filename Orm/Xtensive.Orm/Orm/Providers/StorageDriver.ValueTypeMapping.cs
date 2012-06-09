// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.14

using System;
using Xtensive.Sql;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Providers
{
  partial class StorageDriver
  {
    public TypeMapping GetTypeMapping(ColumnInfo column)
    {
      return allMappings.GetMapping(column.ValueType);
    }

    public TypeMapping GetTypeMapping(Type type)
    {
      return allMappings.GetMapping(type);
    }

    public SqlValueType MapValueType(ColumnInfo columnInfo)
    {
      return allMappings.GetMapping(columnInfo.ValueType).MapType(columnInfo.Length, null, null);
    }

    public SqlValueType MapValueType(Type type)
    {
      return allMappings.GetMapping(type).MapType();
    }

    public SqlValueType MapValueType(Type type, int? length, int? precision, int? scale)
    {
      return allMappings.GetMapping(type).MapType(length, precision, scale);
    }
  }
}