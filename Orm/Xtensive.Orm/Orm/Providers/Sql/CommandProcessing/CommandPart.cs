// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// A part of a command.
  /// </summary>
  public sealed class CommandPart
  {
    public string Statement { get; set; }

    public List<DbParameter> Parameters { get; private set; }

    public List<IDisposable> Resources { get; private set; }

    // Constructors

    internal CommandPart()
    {
      Parameters = new List<DbParameter>();
      Resources = new List<IDisposable>();
    }
  }
}