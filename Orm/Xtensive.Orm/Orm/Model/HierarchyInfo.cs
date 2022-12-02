// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;
using System.Collections.Generic;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Describes a single persistent type hierarchy.
  /// </summary>
  [Serializable]
  public sealed class HierarchyInfo : Node
  {
    /// <summary>
    /// Gets the root of the hierarchy.
    /// </summary>
    public TypeInfo Root { get; private set; }

    /// <summary>
    /// Gets the <see cref="Model.InheritanceSchema"/> for this hierarchy.
    /// </summary>
    public InheritanceSchema InheritanceSchema { get; private set; }

    /// <summary>
    /// Gets the types of the current <see cref="HierarchyInfo"/>.
    /// </summary>
    public IReadOnlyList<TypeInfo> Types { get; private set; }

    /// <summary>
    /// Gets the <see cref="Key"/> for this instance.
    /// </summary>
    public KeyInfo Key { get; private set; }

    /// <summary>
    /// Gets the type discriminator.
    /// </summary>
    public TypeDiscriminatorMap TypeDiscriminatorMap { get; private set; }

    /// <inheritdoc/>
    public override void UpdateState()
    {
      base.UpdateState();
      Key.UpdateState();
      var list = new List<TypeInfo> {Root};
      list.AddRange(Root.RecursiveDescendants);
      Types = list.AsReadOnly();
      if (Types.Count == 1)
        InheritanceSchema = InheritanceSchema.ConcreteTable;
      if (TypeDiscriminatorMap != null)
        TypeDiscriminatorMap.UpdateState();
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      Key.Lock(recursive);
      if (TypeDiscriminatorMap != null)
        TypeDiscriminatorMap.Lock();
    }


    // Constructors

    /// <summary>
    /// 	Initializes a new instance of this class.
    /// </summary>
    /// <param name="root">The hierarchy root.</param>
    /// <param name="key">The key info.</param>
    /// <param name="inheritanceSchema">The inheritance schema.</param>
    /// <param name="typeDiscriminatorMap">The type discriminator map.</param>
    public HierarchyInfo(
      TypeInfo root, 
      KeyInfo key, 
      InheritanceSchema inheritanceSchema, 
      TypeDiscriminatorMap typeDiscriminatorMap)
    {
      Root = root;
      Key = key;
      InheritanceSchema = inheritanceSchema;
      TypeDiscriminatorMap = typeDiscriminatorMap;
    }
  }
}