// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.04.12

using System;

namespace Xtensive.Storage.Tests.Issues.Issue0631_DisconnectedStateBugs
{
  [HierarchyRoot]
  public class SampleEntity : EntityBase
  {
    public SampleEntity(Guid id) : base(id)
    {
    }

    [Field]
    public string Text { get; set; }

    [Field]
    public int Integer { get; set; }

    [Field]
    public DateTime Date { get; set; }

    [Field]
    public Guid Guid { get; set; }

    [Field]
    public AnotherEntity LinkedItem { get; set; }

    [Field]
    [Association(PairTo = "Owner")]
    public EntitySet<AnotherEntity> ItemGroup { get; private set; }
  }
}