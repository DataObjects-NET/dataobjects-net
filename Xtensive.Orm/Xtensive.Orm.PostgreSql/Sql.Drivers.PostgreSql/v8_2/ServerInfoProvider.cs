// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_2
{
  internal class ServerInfoProvider : v8_1.ServerInfoProvider
  {
    protected override IndexFeatures GetIndexFeatures()
    {
      return base.GetIndexFeatures() | IndexFeatures.FillFactor;
    }
    
    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}