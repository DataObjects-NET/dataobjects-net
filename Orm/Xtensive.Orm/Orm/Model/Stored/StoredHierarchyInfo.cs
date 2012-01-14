// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.22

using System.Xml.Serialization;

namespace Xtensive.Orm.Model.Stored
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
    /// <see cref="HierarchyInfo.InheritanceSchema"/>
    /// </summary>
    [XmlElement("Schema")]
    public InheritanceSchema InheritanceSchema;

    /// <summary>
    /// <see cref="HierarchyInfo.Types"/>.
    /// </summary>
    public StoredTypeInfo[] Types;
  }
}