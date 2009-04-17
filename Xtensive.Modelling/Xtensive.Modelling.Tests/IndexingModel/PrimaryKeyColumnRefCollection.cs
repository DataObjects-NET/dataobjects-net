// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Tests.IndexingModel
{
  /// <summary>
  /// A collection of <see cref="PrimaryKeyColumnRef"/> instances.
  /// </summary>
  [Serializable]
  public sealed class PrimaryKeyColumnRefCollection : NodeCollectionBase<PrimaryKeyColumnRef, SecondaryIndexInfo>
  {
    // Constructors
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent.</param>
    public PrimaryKeyColumnRefCollection(SecondaryIndexInfo parent)
      : base(parent, "PrimaryKeyColumns")
    {
    }
  }
}