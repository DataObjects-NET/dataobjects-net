// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2013.12.30

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.MySql.v5_6
{
  internal class ServerInfoProvider : v5_5.ServerInfoProvider
  {
#if NET6_0_OR_GREATER
    /// <inheritdoc/>
    public override DataTypeCollection GetDataTypesInfo()
    {
      var types = base.GetDataTypesInfo();

      var common = DataTypeFeatures.Default | DataTypeFeatures.Nullable | DataTypeFeatures.NonKeyIndexing |
        DataTypeFeatures.Grouping | DataTypeFeatures.Ordering | DataTypeFeatures.Multiple;

      var index = DataTypeFeatures.Indexing | DataTypeFeatures.KeyConstraint;

      types.TimeOnly = DataTypeInfo.Range(SqlType.Time, common | index, ValueRange.TimeOnly, "time(6)");

      return types;
    }
#endif

    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}