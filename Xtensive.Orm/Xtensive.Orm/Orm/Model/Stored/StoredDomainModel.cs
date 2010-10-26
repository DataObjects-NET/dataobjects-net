// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.22

using System.Xml.Serialization;

namespace Xtensive.Orm.Model.Stored
{
  /// <summary>
  /// An xml serializable representation of <see cref="DomainModel"/>.
  /// </summary>
  [XmlRoot("DomainModel", Namespace = "")]
  public sealed class StoredDomainModel
  {
    /// <summary>
    /// <see cref="DomainModel.Types"/>.
    /// </summary>
    [XmlArray("Types"), XmlArrayItem("Type")]
    public StoredTypeInfo[] Types;

    /// <summary>
    /// <see cref="DomainModel.Associations"/>
    /// </summary>
    [XmlIgnore]
    public StoredAssociationInfo[] Associations;

    /// <summary>
    /// <see cref="DomainModel.Hierarchies"/>
    /// </summary>
    [XmlIgnore]
    public StoredHierarchyInfo[] Hierarchies;

    /// <summary>
    /// Updates all references within this model.
    /// </summary>
    public void UpdateReferences()
    {
      new ReferenceUpdater().UpdateReferences(this);
    }
  }
}