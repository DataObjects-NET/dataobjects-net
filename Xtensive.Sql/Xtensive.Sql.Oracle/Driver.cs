// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using Xtensive.Sql.Info;
using Xtensive.Sql.Oracle.v09;

namespace Xtensive.Sql.Oracle
{
  internal abstract class Driver : SqlDriver
  {
    public override SqlConnection CreateConnection()
    {
      return new Connection(this);
    }

    protected override ValueTypeMapping.TypeMapper CreateTypeMapper()
    {
      return new TypeMapper(this);
    }

    // Constructors

    protected Driver(CoreServerInfo coreServerInfo)
      : base(coreServerInfo)
    {
    }
  }
}