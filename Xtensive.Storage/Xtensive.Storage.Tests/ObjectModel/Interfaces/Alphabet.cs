// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.09.24

using System;
using System.Diagnostics;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Tests.ObjectModel.Interfaces.Alphabet
{
  public interface INamed : IEntity
  {
    [Field]
    string Name { get; set; }
  }

  public interface ITagged : IEntity
  {
    [Field]
    string Tag { get; set; }
  }

  [HierarchyRoot(InheritanceSchema.ClassTable)]
  public class A : Entity, INamed
  {
    [Field, Key]
    public long Id { get; private set; }
    public string Name { get; set; }
  }

  public class B : A, INamed, ITagged
  {
    string INamed.Name { get; set; }
    public string Tag { get; set; }
  }

  public class C : A, ITagged
  {
    public string Tag { get; set; }
  }

  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public class D : Entity, INamed
  {
    [Field, Key]
    public long Id { get; private set; }
    public string Name { get; set; }
  }

  public class E : D, INamed, ITagged
  {
    string INamed.Name { get; set; }
    public string Tag { get; set; }
  }

  public class F : D, ITagged
  {
    public string Tag { get; set; }
  }

  [HierarchyRoot(InheritanceSchema.SingleTable)]
  public class G : Entity, INamed
  {
    [Field, Key]
    public long Id { get; private set; }
    public string Name { get; set; }
  }

  public class H : G, INamed, ITagged
  {
    string INamed.Name { get; set; }
    public string Tag { get; set; }
  }

  public class I : G, ITagged
  {
    public string Tag { get; set; }
  }
}