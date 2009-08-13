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

    protected override SqlConnectionHandler CreateConnectionHandler()
    {
      return new ConnectionHandler(this);
    }

    protected override ValueTypeMapping.TypeMappingHandler CreateTypeMappingHandler()
    {
      return new TypeMappingHandler(this);
    }

    protected override string GetDefaultSchemaName(UrlInfo url)
    {
      string result = base.GetDefaultSchemaName(url);
      if (!string.IsNullOrEmpty(result))
        return result;
      return DefaultSchemaName;
    }

    // Constructors

    protected Driver(ServerInfoProvider serverInfoProvider)
      : base(serverInfoProvider)
    {
    }
  }
}