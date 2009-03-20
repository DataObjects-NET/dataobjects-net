// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Modelling;
using System.Diagnostics;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// A collection of storage primary indexes.
  /// </summary>
  [Serializable]
  public class PrimaryIndexInfoCollection : NodeCollectionBase<PrimaryIndexInfo, StorageInfo>,
    IUnorederedNodeCollection
  {

    //Constructors

    public PrimaryIndexInfoCollection(Node parent)
      : base(parent, "PrimaryIndexes")
    {
    }
  }
}