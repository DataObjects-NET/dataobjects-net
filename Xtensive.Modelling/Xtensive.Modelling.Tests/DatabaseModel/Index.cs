// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Modelling.Tests.DatabaseModel
{
  [Serializable]
  public abstract class Index : NodeBase<Table>
  {
    private bool isPrimary;
    private bool isUnique;

    public bool IsPrimary {
      get { return isPrimary; }
    }

    [Property]
    public bool IsUnique {
      get { return isUnique; }
      set {
        EnsureIsEditable();
        if (IsPrimary && !value)
          throw Exceptions.AlreadyInitialized("IsUnique");
        using (var scope = LogChange("IsUnique", value)) {
          isUnique = value;
          scope.Commit();
        }
      }
    }

    protected override void Initialize()
    {
      isPrimary = this is PrimaryIndex;
      base.Initialize();
    }

    public Index(Table parent, string name)
      : base(parent, name)
    {
    }

    public Index(Table parent, string name, int index)
      : base(parent, name, index)
    {
    }
  }
}