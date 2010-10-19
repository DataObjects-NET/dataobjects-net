// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.09.15

using System;
using Xtensive;

namespace Xtensive.Sql.All
{
  /// <summary>
  /// Does nothing, but references types from all SQL DOM assemblies.
  /// </summary>
  public sealed class Referencer
  {
    private Type[] types = new [] {
      typeof (Pair<>),
      typeof (SqlType),
      typeof (SqlServer.DriverFactory),
      typeof (PostgreSql.DriverFactory),
      typeof (Oracle.DriverFactory),
      typeof (SqlServerCe.DriverFactory),
    };


    // Constructors

    // This is the only one. So you can't instantiate this type.
    private Referencer()
    {
    }
  }
}