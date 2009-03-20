// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using System.Diagnostics;
using Xtensive.Modelling;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describe a collection of columns.
  /// </summary>
  [Serializable]
  public class ColumnInfoCollection : NodeCollectionBase<ColumnInfo, PrimaryIndexInfo>, 
    IUnorederedNodeCollection
  {


    //Constructors

    public ColumnInfoCollection(Node parent)
      : base(parent, "Columns")
    {
    }
  }
}