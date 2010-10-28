// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.22

using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Xtensive.Orm.Model.Stored
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
    /// and value of this property specifies <see cref="InheritanceSchema"/>.
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

    /// <summary>
    /// Gets the name of the generic definition type.
    /// </summary>
    [XmlIgnore]
    public string GenericTypeDefinition {
      get {
        if (UnderlyingType == null)
          return null;
        var indexOfGenericSection = UnderlyingType.LastIndexOf("<");
        if (indexOfGenericSection < 0)
          return UnderlyingType;
        var name = UnderlyingType.Substring(0, indexOfGenericSection);
        return name + "<>";
      }
    }

    /// <summary>
    /// Gets the name of the generic argument type.
    /// </summary>
    [XmlIgnore]
    public string[] GenericArguments {
      get {
        if (UnderlyingType == null)
          return null;
        var indexOfGenericSection = UnderlyingType.LastIndexOf("<");
        var arguments = UnderlyingType.Substring(indexOfGenericSection);
        arguments = arguments.Substring(1, arguments.Length - 2);
        return arguments.Split(',');
      }
    }

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

    /// <summary>
    /// Gets a value indicating whether underlying type is generic.
    /// </summary>
    [XmlIgnore]
    public bool IsGeneric { get { return UnderlyingType.IndexOf("<") > 0; } }

    
    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return Name;
    }
  }
}