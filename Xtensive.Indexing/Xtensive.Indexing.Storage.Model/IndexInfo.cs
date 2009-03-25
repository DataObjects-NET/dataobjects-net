// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Indexing.Storage.Model
{
  [Serializable]
  public abstract class IndexInfo : NodeBase<TableInfo>
  {
    private bool isUnique;

    [Property]
    public bool IsPrimary { get; private set; }

    [Property]
    public bool IsUnique
    {
      get { return isUnique; }
      set
      {
        EnsureIsEditable();
        if (IsPrimary && !value)
          throw Exceptions.AlreadyInitialized("IsUnique");
        using (var scope = LogPropertyChange("IsUnique", value))
        {
          isUnique = value;
          scope.Commit();
        }
      }
    }
    
    [Property]
    public KeyColumnRefCollection KeyColumns { get; private set; }

    [Property]
    public ValueColumnRefCollection ValueColumns { get; private set; }

    protected override void Initialize()
    {
      base.Initialize();
      IsPrimary = this is PrimaryIndexInfo;
      if (KeyColumns==null)
        KeyColumns = new KeyColumnRefCollection(this);
      if (ValueColumns==null)
        ValueColumns = new ValueColumnRefCollection(this);
    }

    protected IndexInfo(TableInfo parent, string name, int index)
      : base(parent, name, index)
    {
    }

    protected IndexInfo(TableInfo parent, string name)
      : base(parent, name)
    {
    }
  }
}