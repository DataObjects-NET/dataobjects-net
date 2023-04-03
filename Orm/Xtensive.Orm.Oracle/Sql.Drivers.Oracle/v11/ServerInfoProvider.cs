// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.Oracle.v11
{
  internal class ServerInfoProvider : v10.ServerInfoProvider
  {
    // Constructors

    public override DataTypeCollection GetDataTypesInfo()
    {
      const DataTypeFeatures common = DataTypeFeatures.Default | DataTypeFeatures.Nullable |
        DataTypeFeatures.NonKeyIndexing | DataTypeFeatures.Grouping | DataTypeFeatures.Ordering |
        DataTypeFeatures.Multiple;
      const DataTypeFeatures index = DataTypeFeatures.Indexing | DataTypeFeatures.Clustering |
        DataTypeFeatures.FillFactor | DataTypeFeatures.KeyConstraint;

      var baseCollection = base.GetDataTypesInfo();

      baseCollection.Interval = DataTypeInfo.Range(SqlType.Interval, common | index,
        ValueRange.TimeSpan, "INTERVAL DAY(6) TO SECONDS(7)");

      return baseCollection;
    }


    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}