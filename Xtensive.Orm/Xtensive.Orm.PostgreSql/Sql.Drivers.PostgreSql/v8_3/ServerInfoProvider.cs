// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_3
{
  internal class ServerInfoProvider : v8_2.ServerInfoProvider
  {
    protected override IndexFeatures GetIndexFeatures()
    {
      return base.GetIndexFeatures() | IndexFeatures.SortOrder | IndexFeatures.FullText;
    }

    public override FullTextSearchInfo GetFullTextInfo()
    {
      return new FullTextSearchInfo {Features = FullTextSearchFeatures.Full};
    }

    public override DataTypeCollection GetDataTypesInfo()
    {
      var commonFeatures =
      DataTypeFeatures.Clustering |
      DataTypeFeatures.Grouping |
      DataTypeFeatures.Indexing |
      DataTypeFeatures.KeyConstraint |
      DataTypeFeatures.Nullable |
      DataTypeFeatures.Ordering |
      DataTypeFeatures.Multiple |
      DataTypeFeatures.Default;


      DataTypeCollection dtc =  base.GetDataTypesInfo();
      dtc.Guid = DataTypeInfo.Regular(SqlType.Guid, commonFeatures, "uuid");

      return dtc;
    }

    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}