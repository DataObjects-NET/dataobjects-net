// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Diagnostics;
using Xtensive.Modelling;

namespace Xtensive.Indexing.Storage.Model
{
  [Serializable]
  public class ForeignKeyCollection : NodeCollectionBase<ForeignKeyInfo, PrimaryIndexInfo>, 
    IUnorderedNodeCollection
  {


    //Constructors

    public ForeignKeyCollection(Node parent, string name)
      : base(parent, name)
    {
    }
  }
}