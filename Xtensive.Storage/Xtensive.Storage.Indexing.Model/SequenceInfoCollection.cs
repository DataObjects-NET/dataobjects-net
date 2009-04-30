// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.29

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling;

namespace Xtensive.Storage.Indexing.Model
{
  /// <summary>
  /// The collection of <see cref="SequenceInfo"/> instances.
  /// </summary>
  [Serializable]
  public sealed class SequenceInfoCollection : NodeCollectionBase<SequenceInfo, StorageInfo>,
    IUnorderedNodeCollection
  {
    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The storage.</param>
    public SequenceInfoCollection(StorageInfo parent)
      : base(parent, "Sequences")
    {
    }
  }
}