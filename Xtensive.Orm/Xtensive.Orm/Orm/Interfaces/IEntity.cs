// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.25

using System.ComponentModel;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Operations;
using Xtensive.Storage.Rse;

namespace Xtensive.Orm
{
  /// <summary>
  /// Persistent entity contract.
  /// </summary>
  [SystemType]
  public interface IEntity: 
    ISessionBound,
    IIdentified<Key>, 
    IHasVersion<VersionInfo>,
    INotifyPropertyChanged
  {
    /// <summary>
    /// Gets the <see cref="Key"/> of the <see cref="Entity"/>.
    /// </summary>
    Key Key { get; }

    /// <summary>
    /// Gets the type id.
    /// </summary>
    [Field]
    int TypeId { get; }

    /// <summary>
    /// Gets <see cref="Xtensive.Orm.Model.TypeInfo"/> object describing <see cref="Entity"/> structure.
    /// </summary>
    TypeInfo TypeInfo { get; }

    /// <summary>
    /// Gets <see cref="VersionInfo"/> object describing 
    /// current version of the <see cref="Entity"/>.
    /// </summary>
    VersionInfo VersionInfo { get; }

    /// <summary>
    /// Gets persistence state of the entity.
    /// </summary>
    PersistenceState PersistenceState { get; }

    /// <summary>
    /// Gets or sets the value of the field with specified name.
    /// </summary>
    /// <value>Field value.</value>
    object this[string fieldName] { get; set; }

    /// <summary>
    /// Gets a value indicating whether this entity is removed.
    /// </summary>
    /// <seealso cref="Remove"/>
    bool IsRemoved { get; }

    /// <summary>
    /// Removes the instance.
    /// </summary>
    void Remove();

    /// <summary>
    /// Registers the instance in the removal queue.
    /// </summary>
    void RemoveLater();

    /// <summary>
    /// Locks this instance in the storage.
    /// </summary>
    /// <param name="lockMode">The lock mode.</param>
    /// <param name="lockBehavior">The lock behavior.</param>
    void Lock(LockMode lockMode, LockBehavior lockBehavior);

    /// <summary>
    /// Identifies the entity by identifier of specified type.
    /// This identifier is used by <see cref="Xtensive.Orm.Operations"/> framework
    /// to bind it with the identical entity while replaying the operation.
    /// </summary>
    /// <param name="identifierType">Type of the identifier.</param>
    void IdentifyAs(EntityIdentifierType identifierType);

    /// <summary>
    /// Identifies the entity by specified identifier.
    /// This identifier is used by <see cref="Xtensive.Orm.Operations"/> framework
    /// to bind it with the identical entity while replaying the operation.
    /// </summary>
    /// <param name="identifier">The entity identifier.
    /// <see langword="null" /> indicates no identifier must be associated with the entity.</param>
    void IdentifyAs(string identifier);
  }
}