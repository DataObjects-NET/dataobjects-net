// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.01

using System;
using Xtensive.Core;

namespace Xtensive.Sql.VistaDb
{
  /// <summary>
  /// A <see cref="SqlDriver"/> factory for VistaDB.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    /// <inheritdoc/>
    public override SqlDriver CreateDriver(UrlInfo sqlConnectionUrl)
    {
      return new v3.Driver();
    }
  }
}