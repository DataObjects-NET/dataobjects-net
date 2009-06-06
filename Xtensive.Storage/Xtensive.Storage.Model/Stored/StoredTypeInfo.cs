// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.22

using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Xtensive.Storage.Model.Stored
{
  /// <summary>
  /// A xml serializable representation of <see cref="TypeInfo"/>.
  /// </summary>
  public sealed class StoredTypeInfo : StoredNode
  {
    /// <summary>
    /// <see cref="TypeInfo.UnderlyingType"/>.
    /// </summary>
    public string UnderlyingType;

    /// <summary>
    /// <see cref="TypeInfo.TypeId"/>.
    /// </summary>
    [DefaultValue(TypeInfo.NoTypeId)]
    public int TypeId;

    /// <summary>
    /// If is not <see langword="null"/> declares this instance as hierarchy root
    /// and value of this property speicifes <see cref="InheritanceSchema"/>.
    /// </summary>
    public string HierarchyRoot;

    /// <summary>
    /// <see cref="TypeInfo.Hierarchy"/>.
    /// </summary>
    [XmlIgnore]
    public StoredHierarchyInfo Hierarchy;

    /// <summary>
    /// <see cref="TypeInfo.Fields"/>.
    /// </summary>
    [XmlArray("Fields"), XmlArrayItem("Field")]
    public StoredFieldInfo[] Fields;

    /// <summary>
    /// Contains all fields inherited by this <see cref="StoredTypeInfo"/>
    /// plus all fields in declared in this <see cref="StoredTypeInfo"/>.
    /// </summary>
    [XmlIgnore]
    public StoredFieldInfo[] AllFields;

    /// <summary>
    /// Associations outgoing from this <see cref="StoredTypeInfo"/>.
    /// </summary>
    [XmlArray("Associations"), XmlArrayItem("Association")]
    public StoredAssociationInfo[] Associations;

    /// <summary>
    /// Gets the ancestors of this <see cref="StoredTypeInfo"/>.
    /// </summary>
    [XmlIgnore]
    public StoredTypeInfo Ancestor;

    /// <summary>
    /// Name of <see cref="Ancestor"/>.
    /// </summary>
    [XmlElement("Ancestor")]
    public string AncestorName;

    /// <summary>
    /// Gets both direct and indirect ancestors of this <see cref="StoredTypeInfo"/>.
    /// </summary>
    [XmlIgnore]
    public StoredTypeInfo[] AllAncestors;

    /// <summary>
    /// Gets direct descendants of this <see cref="StoredTypeInfo"/>.
    /// </summary>
    [XmlIgnore]
    public StoredTypeInfo[] Descendants;

    /// <summary>
    /// Gets both direct and indirect descendants of this <see cref="StoredTypeInfo"/>.
    /// </summary>
    [XmlIgnore]
    public StoredTypeInfo[] AllDescendants;

    #region IsXxx fields

    /// <summary>
    /// <see cref="TypeInfo.IsEntity"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool IsEntity;

    /// <summary>
    /// <see cref="TypeInfo.IsAbstract"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool IsAbstract;

    /// <summary>
    /// <see cref="TypeInfo.IsInterface"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool IsInterface;

    /// <summary>
    /// <see cref="TypeInfo.IsStructure"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool IsStructure;

    /// <summary>
    /// <see cref="TypeInfo.IsSystem"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool IsSystem;

    /// <summary>
    /// Gets a value indicating whether this instance is hierarchy root.
    /// </summary>
    [XmlIgnore]
    public bool IsHierarchyRoot { get { return !string.IsNullOrEmpty(HierarchyRoot); } }

    #endregion

    public override string ToString()
    {
      return Name;
    }
  }
}