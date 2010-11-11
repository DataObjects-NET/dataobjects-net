// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Sql.Info;

namespace Xtensive.Sql.PostgreSql.v8_3
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
    

    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}