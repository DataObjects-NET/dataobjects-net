// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.22

using System.Xml.Serialization;

namespace Xtensive.Storage.Model.Stored
{
  /// <summary>
  /// A xml serializable representation of <see cref="TypeInfo"/>.
  /// </summary>
  public sealed class StoredTypeInfo : StoredMappingNode
  {
    /// <summary>
    /// <see cref="TypeInfo.TypeId"/>.
    /// </summary>
    public int TypeId;

    /// <summary>
    /// <see cref="TypeInfo.Hierarchy"/>.
    /// </summary>
    [XmlIgnore]
    public StoredHierarchyInfo Hierarchy;

    /// <summary>
    /// Name of <see cref="Hierarchy"/>.
    /// </summary>
    [XmlElement(ElementName = "Hierarchy")]
    public string HierarchyName;

    /// <summary>
    /// <see cref="TypeInfo.Fields"/>.
    /// </summary>
    [XmlIgnore]
    public StoredFieldInfo[] Fields;

    /// <summary>
    /// Names of <see cref="Fields"/>.
    /// </summary>
    [XmlArray(ElementName = "Fields")]
    public string[] FieldNames;

    /// <summary>
    /// <see cref="TypeInfo.GetAncestor"/>.
    /// </summary>
    [XmlIgnore]
    public StoredTypeInfo Ancestor;

    /// <summary>
    /// Name of <see cref="Ancestor"/>.
    /// </summary>
    [XmlElement(ElementName = "Ancestor")]
    public string AncestorName;

    /// <summary>
    /// <see cref="TypeInfo.IsEntity"/>.
    /// </summary>
    public bool IsEntity;

    /// <summary>
    /// <see cref="TypeInfo.IsAbstract"/>.
    /// </summary>
    public bool IsAbstract;

    /// <summary>
    /// <see cref="TypeInfo.IsInterface"/>.
    /// </summary>
    public bool IsInterface;

    /// <summary>
    /// <see cref="TypeInfo.IsStructure"/>.
    /// </summary>
    public bool IsStructure;

    /// <summary>
    /// <see cref="TypeInfo.IsSystem"/>.
    /// </summary>
    public bool IsSystem;
  }
}