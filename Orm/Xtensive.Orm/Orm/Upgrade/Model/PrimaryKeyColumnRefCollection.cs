// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;


namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// A collection of <see cref="PrimaryKeyColumnRef"/> instances.
  /// </summary>
  [Serializable]
  public sealed class PrimaryKeyColumnRefCollection : NodeCollectionBase<PrimaryKeyColumnRef, SecondaryIndexInfo>
  {
    // Constructors
    
    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="parent">The parent.</param>
    public PrimaryKeyColumnRefCollection(SecondaryIndexInfo parent)
      : base(parent, "PrimaryKeyColumns")
    {
    }
  }
}