// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.08

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Firebird
{
  /// <summary>
  /// A <see cref="SqlDriver"/> factory for Firebird.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    public override SqlDriver CreateDriver(string connectionString)
    {
      throw new NotImplementedException();
    }

    public override string BuildConnectionString(UrlInfo connectionUrl)
    {
      throw new NotImplementedException();
    }
  }
}