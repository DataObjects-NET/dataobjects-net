// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// A collection of <see cref="KeyColumnRef"/> instances.
  /// </summary>
  [Serializable]
  public sealed class KeyColumnRefCollection : NodeCollectionBase<KeyColumnRef, IndexInfo>
  {
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent.</param>
    public KeyColumnRefCollection(IndexInfo parent)
      : base(parent, "KeyColumns")
    {
    }
  }
}