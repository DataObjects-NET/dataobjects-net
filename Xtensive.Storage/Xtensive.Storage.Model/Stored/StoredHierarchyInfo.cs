// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.22

namespace Xtensive.Storage.Model.Stored
{
  /// <summary>
  /// A xml serializable representation of <see cref="HierarchyInfo"/>.
  /// </summary>
  public sealed class StoredHierarchyInfo
  {
    /// <summary>
    /// <see cref="HierarchyInfo.Root"/>.
    /// </summary>
    public StoredTypeInfo Root;

    /// <summary>
    /// <see cref="HierarchyInfo.Schema"/>
    /// </summary>
    public InheritanceSchema Schema;

    /// <summary>
    /// <see cref="HierarchyInfo.Types"/>.
    /// </summary>
    public StoredTypeInfo[] Types;
  }
}