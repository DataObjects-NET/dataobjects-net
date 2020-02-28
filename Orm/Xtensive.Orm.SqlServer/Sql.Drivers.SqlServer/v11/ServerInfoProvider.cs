// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.02

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.SqlServer.v11
{
  internal class ServerInfoProvider : v10.ServerInfoProvider
  {
    public override SequenceInfo GetSequenceInfo()
    {
      var info = new SequenceInfo();
      info.AllowedDdlStatements = DdlStatements.All;
      info.Features = SequenceFeatures.Cache;
      info.MaxIdentifierLength = MaxIdentifierLength;
      return info;
    }

    public override QueryInfo GetQueryInfo()
    {
      var info = base.GetQueryInfo();
      info.Features |= QueryFeatures.Offset | QueryFeatures.ZeroLimitIsError;
      return info;
    }

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}