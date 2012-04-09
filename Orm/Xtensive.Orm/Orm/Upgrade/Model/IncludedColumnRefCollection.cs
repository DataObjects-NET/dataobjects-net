// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;

using Xtensive.Modelling;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// A collection of <see cref="IncludedColumnRef"/> instances.
  /// </summary>
  [Serializable]
  public sealed class IncludedColumnRefCollection : NodeCollectionBase<IncludedColumnRef, SecondaryIndexInfo>
  {
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="parent">The parent index.</param>
    public IncludedColumnRefCollection(SecondaryIndexInfo parent)
      : base(parent, "ValueColumns")
    {
    }
  }
}