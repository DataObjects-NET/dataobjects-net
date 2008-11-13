// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.07

using Xtensive.Storage.Attributes;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Tests.Storage.CustomEntitySetModel
{
  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Root : Entity
  {
    [Field]
    public int Id { get; private set; }
  }

  public class A : Root
  {
    [Field]
    public CustomEntitySet<B> ManyToZero { get; private set; }

    [Field]
    public CustomEntitySet<B> ManyToOne { get; private set; }

    [Field]
    public CustomEntitySet<B> ManyToMany { get; private set; }
  }

  public class B : Root
  {
    [Field(PairTo="ManyToOne")]
    public A OneToMany { get; set; }

    [Field(PairTo="ManyToMany")]
    public CustomEntitySet<A> ManyToMany { get; private set; }
  }

  public class CustomEntitySet<T> : EntitySet<T> where T : Entity
  {
    public CustomEntitySet(Persistent owner, FieldInfo field, bool notify)
      : base(owner, field, notify)
    {
    }
  }
}


namespace Xtensive.Storage.Tests.Storage
{
  public class CustomEntitySetTest : AutoBuildTest
  {
    
  }
}