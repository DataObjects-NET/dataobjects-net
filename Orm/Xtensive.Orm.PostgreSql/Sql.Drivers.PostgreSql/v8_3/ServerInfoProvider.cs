// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_3
{
  internal class ServerInfoProvider : v8_2.ServerInfoProvider
  {
    protected override IndexFeatures GetIndexFeatures() =>
      base.GetIndexFeatures() | IndexFeatures.SortOrder | IndexFeatures.FullText;

    public override FullTextSearchInfo GetFullTextInfo() =>
      new FullTextSearchInfo { Features = FullTextSearchFeatures.Full };

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

      var dtc =  base.GetDataTypesInfo();
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