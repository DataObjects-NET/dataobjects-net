// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using Xtensive.Core;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Core.Tests.Modelling.DatabaseModel
{
  [Serializable]
  public abstract class Index : NodeBase<Table>
  {
    private bool isPrimary;
    private bool isUnique;

    /// <summary>
    /// Gets a value indicating whether this is primary index.
    /// </summary>
    [Property (IgnoreInComparison = true)]
    public bool IsPrimary {
      get { return isPrimary; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this index is unique.
    /// </summary>
    /// <exception cref="NotSupportedException">Index is <see cref="PrimaryIndex"/>.</exception>
    [Property]
    public bool IsUnique {
      get { return isUnique; }
      set {
        if (IsPrimary && !value)
          throw Exceptions.AlreadyInitialized("IsUnique");
        ChangeProperty("IsUnique", value, (x,v) => ((Index)x).isUnique = v);
      }
    }

    protected override void Initialize()
    {
      isPrimary = this is PrimaryIndex;
      if (isPrimary)
        isUnique = true;
      base.Initialize();
    }

    public Index(Table parent, string name)
      : base(parent, name)
    {
    }
  }
}