// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.16

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Oracle
{
  /// <summary>
  /// A <see cref="SqlDriverFactory"/> for Oracle.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    public override SqlDriver CreateDriver(UrlInfo sqlConnectionUrl)
    {
      throw new NotImplementedException();
    }
  }
}