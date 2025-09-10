// Copyright (C) 2013-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alena Mikshina
// Created:    2013.12.30

using System;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.MySql.v5_6
{
  internal class ServerInfoProvider : v5_5.ServerInfoProvider
  {
    /// <inheritdoc/>
    public override DataTypeCollection GetDataTypesInfo()
    {
      var types = base.GetDataTypesInfo();

      var common = DataTypeFeatures.Default | DataTypeFeatures.Nullable | DataTypeFeatures.NonKeyIndexing |
        DataTypeFeatures.Grouping | DataTypeFeatures.Ordering | DataTypeFeatures.Multiple;

      var index = DataTypeFeatures.Indexing | DataTypeFeatures.KeyConstraint;

      types.DateTime = DataTypeInfo.Range(SqlType.DateTime, common | index,
         new ValueRange<DateTime>(new DateTime(1000, 1, 1), new DateTime(9999, 12, 31)),
         "datetime(6)", "datetime", "time(6)", "time");
      return types;
    }

    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}