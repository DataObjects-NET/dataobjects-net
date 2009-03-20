// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using System.Diagnostics;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Modelling.Tests.DatabaseModel
{
  [Serializable]
  public class User : NodeBase<Security, Server>
  {
    [Property]
    public string Password { get; set; }

    [Property(IgnoreInComparison = true)]
    public int UsageCount { get; set; }

    /// <exception cref="InvalidOperationException">UsageCount &gt; 0</exception>
    protected override void ValidateRemove()
    {
      base.ValidateRemove();
      if (UsageCount>0)
        throw new InvalidOperationException("UsageCount > 0");
    }

    /// <exception cref="InvalidOperationException">UsageCount &lt; 0</exception>
    protected override void ValidateState()
    {
      base.ValidateState();
      if (UsageCount<0)
        throw new InvalidOperationException("UsageCount < 0");
    }

    protected override Nesting CreateNesting()
    {
      return new Nesting<User, Security, UserCollection>(this, "Users");
    }


    public User(Security parent, string name)
      : base(parent, name)
    {
    }

    public User(Security parent, string name, int index)
      : base(parent, name, index)
    {
    }
  }
}