// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// A part of a command.
  /// </summary>
  public sealed class CommandPart
  {
    /// <summary>
    /// Query text.
    /// </summary>
    public string Query;

    /// <summary>
    /// Parameters bound to this command part.
    /// </summary>
    public readonly List<DbParameter> Parameters = new List<DbParameter>();

    /// <summary>
    /// Objects that should be disposed uppon this command part completion.
    /// </summary>
    public readonly List<IDisposable> Disposables = new List<IDisposable>();
  }
}