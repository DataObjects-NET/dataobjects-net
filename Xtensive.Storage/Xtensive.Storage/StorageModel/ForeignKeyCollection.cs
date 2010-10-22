// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using Xtensive.Internals.DocTemplates;
using Xtensive.Modelling;
using Xtensive.Storage.StorageModel;

namespace Xtensive.Storage.StorageModel
{
  /// <summary>
  /// Foreign key collection.
  /// </summary>
  [Serializable]
  public sealed class ForeignKeyCollection : NodeCollectionBase<ForeignKeyInfo, TableInfo>, 
    IUnorderedNodeCollection
  {
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="table">The table.</param>
    public ForeignKeyCollection(TableInfo table)
      : base(table, "ForeignKeys")
    {
    }
  }
}