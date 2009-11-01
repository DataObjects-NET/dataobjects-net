// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.08

using System;

namespace Xtensive.Core.Serialization
{
  public struct FixupAction
  {
    private readonly IReference reference;
    private readonly Action<IReference> action;

    public IReference Reference
    {
      get { return reference; }
    }

    public Action<IReference> Action
    {
      get { return action; }
    }


    // Constructors

    public FixupAction(IReference reference, Action<IReference> action)
    {
      if (reference.IsEmpty)
        throw new ArgumentException("reference is empty.");
      ArgumentValidator.EnsureArgumentNotNull(action, "action");
      this.reference = reference;
      this.action = action;
    }
  }
}