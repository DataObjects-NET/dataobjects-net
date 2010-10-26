// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.04.12

using System;
using System.Collections.Generic;

namespace Xtensive.Orm.Tests.Issues.Issue0631_DisconnectedStateBugs
{
  public abstract class EntityBase : Entity
  {
    private Dictionary<string, object> originalFields;

    [Field, Key]
    public Guid Id { get; private set; }

    public Dictionary<string, object> OriginalFields {
      get { return originalFields ?? (originalFields = new Dictionary<string, object>()); }
    }


    // Constructors

    protected EntityBase(Guid id) : base(id)
    {
    }
  }
}