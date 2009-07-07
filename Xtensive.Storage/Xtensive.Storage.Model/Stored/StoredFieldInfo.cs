// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.22

using System.ComponentModel;
using System.Xml.Serialization;

namespace Xtensive.Storage.Model.Stored
{
  /// <summary>
  /// A xml serializable representation of <see cref="FieldInfo"/>.
  /// </summary>
  public sealed class StoredFieldInfo : StoredNode
  {
    /// <summary>
    /// <see cref="FieldInfo.DeclaringType"/>
    /// </summary>
    [XmlIgnore]
    public StoredTypeInfo DeclaringType;

    /// <summary>
    /// <see cref="FieldInfo.UnderlyingProperty"/>.
    /// </summary>
    public string PropertyName;

    /// <summary>
    /// <see cref="FieldInfo.OriginalName"/>.
    /// </summary>
    public string OriginalName;

    /// <summary>
    /// <see cref="FieldInfo.ValueType"/>.
    /// </summary>
    public string ValueType;

    /// <summary>
    /// <see cref="FieldInfo.Parent"/>.
    /// </summary>
    [XmlIgnore]
    public StoredFieldInfo Parent;

    /// <summary>
    /// <see cref="FieldInfo.Fields"/>.
    /// </summary>
    [XmlArray("Fields"), XmlArrayItem("Field")]
    public StoredFieldInfo[] Fields;

    /// <summary>
    /// <see cref="FieldInfo.Length"/>.
    /// </summary>
    [DefaultValue(0)]
    public int Length;

    /// <summary>
    /// <see cref="FieldInfo.ItemType"/>.
    /// </summary>
    public string ItemType;

    #region IsXxx fields

    /// <summary>
    /// <see cref="FieldInfo.IsSystem"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool IsSystem;

    /// <summary>
    /// <see cref="FieldInfo.IsTypeId"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool IsTypeId;

    /// <summary>
    /// <see cref="FieldInfo.IsEnum"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool IsEnum;

    /// <summary>
    /// <see cref="FieldInfo.IsPrimaryKey"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool IsPrimaryKey;

    /// <summary>
    /// <see cref="FieldInfo.IsNested"/>.
    /// </summary>
    [XmlIgnore]
    public bool IsNested { get { return Parent!=null; } }

    /// <summary>
    /// <see cref="FieldInfo.IsExplicit"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool IsExplicit;

    /// <summary>
    /// <see cref="FieldInfo.IsInterfaceImplementation"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool IsInterfaceImplementation;

    /// <summary>
    /// <see cref="FieldInfo.IsPrimitive"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool IsPrimitive;

    /// <summary>
    /// <see cref="FieldInfo.IsEntity"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool IsEntity;

    /// <summary>
    /// <see cref="FieldInfo.IsStructure"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool IsStructure;

    /// <summary>
    /// <see cref="FieldInfo.IsEntitySet"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool IsEntitySet;

    /// <summary>
    /// <see cref="FieldInfo.IsNullable"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool IsNullable;

    /// <summary>
    /// <see cref="FieldInfo.IsLazyLoad"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool IsLazyLoad;

    #endregion

    public override string ToString()
    {
      return DeclaringType.Name + "." + Name;
    }
  }
}