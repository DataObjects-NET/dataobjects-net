// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.22

using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Xtensive.Core;

namespace Xtensive.Orm.Model.Stored
{
  /// <summary>
  /// An xml serializable representation of <see cref="AssociationInfo"/>.
  /// </summary>
  public sealed class StoredAssociationInfo : StoredNode
  {
    private const string FormatString = "{0}.{1}";

    /// <summary>
    /// <see cref="AssociationInfo.OwnerField"/>.
    /// </summary>
    [XmlIgnore]
    public StoredFieldInfo ReferencingField;

    /// <summary>
    /// Name of <see cref="ReferencingField"/>.
    /// </summary>
    [XmlElement("ReferencingField")]
    public string ReferencingFieldName;

    /// <summary>
    /// <see cref="AssociationInfo.TargetType"/>.
    /// </summary>
    [XmlIgnore]
    public StoredTypeInfo ReferencedType;

    /// <summary>
    /// Name of <see cref="ReferencedType"/>
    /// </summary>
    [XmlElement("ReferencedType")]
    public string ReferencedTypeName;

    /// <summary>
    /// <see cref="AssociationInfo.Multiplicity"/>.
    /// </summary>
    [XmlIgnore]
    public Multiplicity Multiplicity;

    /// <summary>
    /// Name of <see cref="Multiplicity"/>.
    /// </summary>
    [XmlElement("Multiplicity")]
    public string MultiplicityName;

    /// <summary>
    /// <see cref="AssociationInfo.Reversed"/>.
    /// </summary>
    [XmlIgnore]
    public StoredAssociationInfo Reversed;

    /// <summary>
    /// Name of <see cref="Reversed"/>
    /// </summary>
    [XmlElement("Reversed")]
    public string ReversedName;

    /// <summary>
    /// <see cref="AssociationInfo.IsMaster"/>
    /// </summary>
    [DefaultValue(false)]
    public bool IsMaster;

    /// <summary>
    /// <see cref="AssociationInfo.AuxiliaryType"/>
    /// </summary>
    [XmlIgnore]
    public StoredTypeInfo ConnectorType;

    /// <summary>
    /// Name of <see cref="ConnectorType"/>
    /// </summary>
    [XmlElement("ConnectorType")]
    public string ConnectorTypeName;

    /// <inheritdoc/>
    public override string ToString()
    {
      return Name ?? string.Format(FormatString, ReferencingField.DeclaringType.Name, ReferencingField.Name);
    }
  }
}