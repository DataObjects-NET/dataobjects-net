// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.12

using System.Collections.Generic;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// A registry of temporary tables.
  /// </summary>
  public class TemporaryTableStateRegistry
  {
    /// <summary>
    /// Gets or sets the states of temporary tables.
    /// </summary>
    public Dictionary<string, bool> States { get; private set; }

    public TemporaryTableStateRegistry()
    {
      States = new Dictionary<string, bool>();
    }
  }
}