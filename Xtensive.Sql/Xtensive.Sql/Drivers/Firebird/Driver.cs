// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.08

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Firebird
{
  internal abstract class Driver : SqlDriver
  {
    protected override SqlConnection CreateConnection(string connectionString)
    {
      return new Connection(this, connectionString);
    }


    // Constructors

    protected Driver(CoreServerInfo coreServerInfo)
      : base(coreServerInfo)
    {
    }
  }
}