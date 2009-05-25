// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.22

using System.Xml.Serialization;

namespace Xtensive.Storage.Model.Stored
{
  /// <summary>
  /// A xml serializable representation of <see cref="FieldInfo"/>.
  /// </summary>
  public sealed class StoredFieldInfo : StoredMappingNode
  {
    /// <summary>
    /// <see cref="FieldInfo.UnderlyingProperty"/>.
    /// </summary>
    [XmlElement(ElementName = "UnderlyingProperty")]
    public string UnderlyingPropertyName;

    /// <summary>
    /// <see cref="FieldInfo.Length"/>.
    /// </summary>
    public int? Length;

    /// <summary>
    /// <see cref="FieldInfo.ItemType"/>.
    /// </summary>
    public string ItemTypeName;

    /// <summary>
    /// <see cref="FieldInfo.IsSystem"/>.
    /// </summary>
    public bool IsSystem;

    /// <summary>
    /// <see cref="FieldInfo.IsTypeId"/>.
    /// </summary>
    public bool IsTypeId;

    /// <summary>
    /// <see cref="FieldInfo.IsEnum"/>.
    /// </summary>
    public bool IsEnum;

    /// <summary>
    /// <see cref="FieldInfo.IsPrimaryKey"/>.
    /// </summary>
    public bool IsPrimaryKey;

    /// <summary>
    /// <see cref="FieldInfo.IsNested"/>.
    /// </summary>
    public bool IsNested;

    /// <summary>
    /// <see cref="FieldInfo.IsExplicit"/>.
    /// </summary>
    public bool IsExplicit;

    /// <summary>
    /// <see cref="FieldInfo.IsInterfaceImplementation"/>.
    /// </summary>
    public bool IsInterfaceImplementation;

    /// <summary>
    /// <see cref="FieldInfo.IsPrimitive"/>.
    /// </summary>
    public bool IsPrimitive;

    /// <summary>
    /// <see cref="FieldInfo.IsEntity"/>.
    /// </summary>
    public bool IsEntity;

    /// <summary>
    /// <see cref="FieldInfo.IsStructure"/>.
    /// </summary>
    public bool IsStructure;

    /// <summary>
    /// <see cref="FieldInfo.IsEntitySet"/>.
    /// </summary>
    public bool IsEntitySet;

    /// <summary>
    /// <see cref="FieldInfo.IsNullable"/>.
    /// </summary>
    public bool IsNullable;

    /// <summary>
    /// <see cref="FieldInfo.IsLazyLoad"/>.
    /// </summary>
    public bool IsLazyLoad;

    /// <summary>
    /// <see cref="FieldInfo.IsTranslatable"/>.
    /// </summary>
    public bool IsTranslatable;

    /// <summary>
    /// <see cref="FieldInfo.IsCollatable"/>.
    /// </summary>
    public bool IsCollatable;

    /// <summary>
    /// <see cref="FieldInfo.ValueType"/>.
    /// </summary>
    [XmlElement(ElementName = "ValueType")]
    public string ValueTypeName;

    /// <summary>
    /// <see cref="FieldInfo.Fields"/>.
    /// </summary>
    [XmlIgnore]
    public StoredFieldInfo[] Fields;

    /// <summary>
    /// Names of <see cref="Fields"/>.
    /// </summary>
    [XmlArray(ElementName = "Fields")]
    public string[] FieldNames;

    /// <summary>
    /// <see cref="FieldInfo.Parent"/>.
    /// </summary>
    [XmlIgnore]
    public StoredFieldInfo Parent;
  }
}