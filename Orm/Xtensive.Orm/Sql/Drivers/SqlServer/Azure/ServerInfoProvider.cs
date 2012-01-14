// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.09

using Xtensive.Sql.Info;
using SqlServerConnection = System.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.Drivers.SqlServer.Azure
{
  internal class ServerInfoProvider : v10.ServerInfoProvider
  {
    public override FullTextSearchInfo GetFullTextInfo()
    {
      return null;
    }

    public override IndexInfo GetIndexInfo()
    {
      var result = base.GetIndexInfo();
      result.Features = result.Features & ~IndexFeatures.Clustered;
      return result;
    }

    public override PrimaryKeyConstraintInfo GetPrimaryKeyInfo()
    {
      var result = base.GetPrimaryKeyInfo();
      result.Features = result.Features & ~PrimaryKeyConstraintFeatures.Clustered;
      return result;
    }

    public override UniqueConstraintInfo GetUniqueConstraintInfo()
    {
      var result = base.GetUniqueConstraintInfo();
      result.Features = result.Features & ~UniqueConstraintFeatures.Clustered;
      return result;
    }


    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}