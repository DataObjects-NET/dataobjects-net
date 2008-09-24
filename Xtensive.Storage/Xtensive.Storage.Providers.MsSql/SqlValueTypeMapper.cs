// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.23

using System;
using Xtensive.Sql.Common;

namespace Xtensive.Storage.Providers.MsSql
{
  public sealed class SqlValueTypeMapper : Sql.SqlValueTypeMapper
  {
    protected override void BuildTypeSubstituteMappings()
    {
      base.BuildTypeSubstituteMappings();

      SubstituteTimeSpan();
    }

    private void SubstituteTimeSpan()
    {
      var substitute = DomainHandler.Driver.ServerInfo.DataTypes.Int64;
      RangeDataTypeInfo<TimeSpan> target = new RangeDataTypeInfo<TimeSpan>(SqlDataType.Int64, null);
      target.Value = new ValueRange<TimeSpan>(TimeSpan.FromTicks(substitute.Value.MinValue), TimeSpan.FromTicks(substitute.Value.MaxValue));
      Register(target);
    }
  }
}