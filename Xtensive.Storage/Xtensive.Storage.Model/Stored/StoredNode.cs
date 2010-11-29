// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.22

using System.Diagnostics;

namespace Xtensive.Storage.Model.Stored
{
  /// <summary>
  /// A xml serializable representation of <see cref="MappingName"/>.
  /// </summary>
  [DebuggerDisplay("{MappingName??Name}")]
  public abstract class StoredNode
  {
    /// <summary>
    /// <see cref="Node.Name"/>
    /// </summary>
    public string Name;

    /// <summary>
    /// <see cref="MappingNode.MappingName"/>.
    /// </summary>
    public string MappingName;
  }
}