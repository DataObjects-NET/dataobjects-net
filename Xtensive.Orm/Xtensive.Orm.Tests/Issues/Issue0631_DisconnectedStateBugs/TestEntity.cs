// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.04.12

using System;

namespace Xtensive.Orm.Tests.Issues.Issue0631_DisconnectedStateBugs
{
  public class TestEntity : OwnerEntity
  {
    [Field]
    public Guid? NullableGuid { get; set; }

    [Field]
    public OwnerEntity Reference { get; set; }

    
    // Constructors
    
    public TestEntity(Guid id) : base(id)
    {
    }
  }
}