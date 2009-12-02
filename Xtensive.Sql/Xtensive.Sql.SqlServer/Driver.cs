// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using Xtensive.Core;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.SqlServer
{
  internal abstract class Driver : SqlDriver
  {
    private const string DefaultSchemaName = "dbo";

    public override SqlConnection CreateConnection(UrlInfo url)
    {
      return new Connection(this, url);
    }

    protected override string GetDefaultSchemaName(UrlInfo url)
    {
      return url.GetSchema(DefaultSchemaName);
    }

    // Constructors

    protected Driver(ServerInfoProvider serverInfoProvider)
      : base(serverInfoProvider)
    {
    }
  }
}