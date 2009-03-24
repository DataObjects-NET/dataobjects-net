// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Modelling;
using System.Diagnostics;
using Xtensive.Core;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// An abstract base class for all columns refs.
  /// </summary>
  /// <typeparam name="TParent">The type of the parent node.</typeparam>
  [Serializable]
  public abstract class ColumnInfoRef<TParent>: Ref<ColumnInfo, TParent>
    where TParent : Node
  {


    //Constructors

    protected ColumnInfoRef(TParent parent, int index)
      : base(parent, index)
    {
    }

    protected ColumnInfoRef(TParent parent, ColumnInfo column, int index)
      : base(parent, index)
    {
      Value = column;
    }
  }
}