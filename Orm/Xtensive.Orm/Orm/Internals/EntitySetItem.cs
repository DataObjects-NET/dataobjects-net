// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.05

using System;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// Abstract base class for internally used <see cref="EntitySet{TItem}"/> items.
  /// Instances of runtime-generated descendants of this type are used
  /// to actually describe the relationship behind the <see cref="EntitySet{TItem}"/>,
  /// if it isn't paired, or is paired to another <see cref="EntitySet{TItem}"/>.
  /// </summary>
  /// <typeparam name="TMaster">The type of the master.</typeparam>
  /// <typeparam name="TSlave">The type of the slave.</typeparam>
  [Serializable]
  [KeyGenerator(KeyGeneratorKind.None)]
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ConcreteTable)]
  public abstract class EntitySetItem<TMaster, TSlave> : Entity
    where TMaster : IEntity
    where TSlave : IEntity
  {
    /// <summary>
    /// Gets the master entity reference.
    /// </summary>
    [Field, Key(0)]
    public TMaster Master { get; private set; }

    /// <summary>
    /// Gets the slave entity reference.
    /// </summary>
    [Field, Key(1)]
    public TSlave Slave { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="tuple">The tuple containing key value for this instance.</param>
    protected EntitySetItem(Session session, Tuple tuple)
      : base(session, tuple)
    {
    }
  }
}