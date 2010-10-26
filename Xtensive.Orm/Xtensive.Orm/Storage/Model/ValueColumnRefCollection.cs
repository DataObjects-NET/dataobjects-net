// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using Xtensive.Internals.DocTemplates;
using Xtensive.Modelling;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// A collection of <see cref="ValueColumnRef"/> instances.
  /// </summary>
  [Serializable]
  public sealed class ValueColumnRefCollection : NodeCollectionBase<ValueColumnRef, PrimaryIndexInfo>,
    IUnorderedNodeCollection
  {
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent index.</param>
    public ValueColumnRefCollection(PrimaryIndexInfo parent)
      : base(parent, "ValueColumns")
    {
    }
  }
}