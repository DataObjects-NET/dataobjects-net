// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kryuchkov
// Created:    2009.05.22

namespace Xtensive.Storage.Model.Stored
{
  /// <summary>
  /// A xml serializable representation of <see cref="MappingName"/>.
  /// </summary>
  public abstract class StoredMappingNode : StoredNode
  {
    /// <summary>
    /// <see cref="MappingNode.MappingName"/>.
    /// </summary>
    public string MappingName;
  }
}