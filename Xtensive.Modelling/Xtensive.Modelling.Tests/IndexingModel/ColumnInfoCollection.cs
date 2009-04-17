// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling;

namespace Xtensive.Modelling.Tests.IndexingModel
{
  /// <summary>
  /// Column collection.
  /// </summary>
  [Serializable]
  public class ColumnInfoCollection : NodeCollectionBase<ColumnInfo, TableInfo>, 
    IUnorderedNodeCollection
  {
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent.</param>
    public ColumnInfoCollection(Node parent)
      : base(parent, "Columns")
    {
    }
  }
}