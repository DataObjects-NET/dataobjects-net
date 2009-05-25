// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.22

namespace Xtensive.Storage.Model.Stored
{
  /// <summary>
  /// An xml serializable representation of <see cref="DomainModel"/>.
  /// </summary>
  public sealed class StoredDomainModel
  {
    /// <summary>
    /// <see cref="DomainModel.Types"/>.
    /// </summary>
    public StoredTypeInfo[] Types;

    /// <summary>
    /// <see cref="DomainModel.Associations"/>
    /// </summary>
    public StoredAssociationInfo[] Associations;

    /// <summary>
    /// <see cref="DomainModel.Hierarchies"/>
    /// </summary>
    public StoredHierarchyInfo[] Hierarchies;

    /// <summary>
    /// All fields associated with this domain model.
    /// </summary>
    public StoredFieldInfo[] Fields;

    /// <summary>
    /// Updates all references within this model.
    /// </summary>
    public void UpdateReferences()
    {
      new ReferenceUpdater().UpdateReferences(this);
    }
  }
}