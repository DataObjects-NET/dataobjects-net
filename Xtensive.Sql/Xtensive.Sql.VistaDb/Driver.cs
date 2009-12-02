// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.01

using System;
using Xtensive.Core;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.VistaDb
{
  internal abstract class Driver : SqlDriver
  {
    public override SqlConnection CreateConnection(UrlInfo url)
    {
      return new Connection(this, url);
    }

    protected override ValueTypeMapping.TypeMapper CreateTypeMapper()
    {
      return new TypeMapper(this);
    }

    // Constructors

    protected Driver(ServerInfoProvider serverInfoProvider)
      : base(serverInfoProvider)
    {
    }
  }
}