// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.14

using System;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;
using Xtensive.Modelling;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// A collection of <see cref="FullTextColumnRef"/> instances.
  /// </summary>
  [Serializable]
  public sealed class FullTextColumnRefCollection : NodeCollectionBase<FullTextColumnRef, FullTextIndexInfo>
  {

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public FullTextColumnRefCollection(FullTextIndexInfo parent)
      : base(parent, "Columns")
    {
    }
  }
}