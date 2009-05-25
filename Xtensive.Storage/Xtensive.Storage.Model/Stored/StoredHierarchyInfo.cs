// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.22

using System.Xml.Serialization;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Model.Stored
{
  /// <summary>
  /// A xml serializable representation of <see cref="HierarchyInfo"/>.
  /// </summary>
  public sealed class StoredHierarchyInfo : StoredNode
  {
    /// <summary>
    /// <see cref="HierarchyInfo.Root"/>.
    /// </summary>
    [XmlIgnore]
    public StoredTypeInfo Root;

    /// <summary>
    /// Name of <see cref="Root"/>.
    /// </summary>
    [XmlElement(ElementName = "Root")]
    public string RootName;

    /// <summary>
    /// <see cref="HierarchyInfo.Schema"/>
    /// </summary>
    [XmlIgnore]
    public InheritanceSchema Schema;

    /// <summary>
    /// Name of <see cref="Schema"/>.
    /// </summary>
    [XmlElement(ElementName = "Schema")]
    public string SchemaName;

    /// <summary>
    /// <see cref="HierarchyInfo.Types"/>.
    /// </summary>
    [XmlIgnore]
    public StoredTypeInfo[] Types;
  }
}