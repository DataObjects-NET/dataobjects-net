// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24


namespace Xtensive.Orm.Tests.Core.Modelling.IndexingModel
{
  /// <summary>
  /// A collection of <see cref="IncludedColumnRef"/> instances.
  /// </summary>
  [Serializable]
  public sealed class IncludedColumnRefCollection: NodeCollectionBase<IncludedColumnRef, SecondaryIndexInfo>
  {
    // Constructors
    
    public IncludedColumnRefCollection(SecondaryIndexInfo parent)
      : base(parent, "ValueColumns")
    {
    }
  }
}