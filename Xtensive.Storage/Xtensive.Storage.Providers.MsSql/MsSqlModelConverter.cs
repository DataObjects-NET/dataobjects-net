// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.26

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Providers.Sql;

namespace Xtensive.Storage.Providers.MsSql
{
  internal sealed class MsSqlModelConverter : SqlModelConverter
  {
    protected override TypeInfo ExtractType(TableColumn column)
    {
      if (column.Domain!=null && column.Domain.Name==SqlWellknown.TimeSpanDomainName)
        return new TypeInfo(
          column.IsNullable ? typeof (TimeSpan?) : typeof (TimeSpan), column.IsNullable);

      return base.ExtractType(column);
    }


    // Constructor

    /// <inheritdoc/>
    public MsSqlModelConverter(Schema storageSchema, Func<ISqlCompileUnit, object> commandExecutor, 
      Func<SqlValueType, TypeInfo> valueTypeConverter)
      : base(storageSchema, commandExecutor, valueTypeConverter)
    {}
  }
}