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
  [Index("Name", Unique = true)]
  public interface INamed : IEntity
  {
    [Field]
    string Name { get; set; }
  }

  [Index("Tag")]
  public interface ITagged : IEntity
  {
    [Field]
    string Tag { get; set; }
  }

  [Index("First", "Second")]
  public interface IComposite : IEntity
  {
    [Field]
    string First { get; set; }

    [Field]
    string Second { get; set; }
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
    string ITagged.Tag { get; set; }
  }

  public class D : B, INamed, IComposite
  {
    string INamed.Name { get; set; }
    public string First { get; set; }
    public string Second { get; set;}
  }

  public class E : D, IComposite
  {
    string IComposite.First { get; set; }
  }

  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public class F : Entity, INamed
  {
    [Field, Key]
    public long Id { get; private set; }
    public string Name { get; set; }
  }

  public class G : F, INamed, ITagged
  {
    string INamed.Name { get; set; }
    public string Tag { get; set; }
  }

  public class H : F, ITagged
  {
    string ITagged.Tag { get; set; }
  }

  public class I : G, INamed, IComposite
  {
    string INamed.Name { get; set; }
    public string First { get; set; }
    public string Second { get; set; }
  }

  public class J : I, IComposite
  {
    string IComposite.First { get; set; }
  }

  [HierarchyRoot(InheritanceSchema.SingleTable)]
  public class K : Entity, INamed
  {
    [Field, Key]
    public long Id { get; private set; }
    public string Name { get; set; }
  }

  public class L : K, INamed, ITagged
  {
    string INamed.Name { get; set; }
    public string Tag { get; set; }
  }

  public class M : K, ITagged
  {
    string ITagged.Tag { get; set; }
  }

  public class N : L, INamed, IComposite
  {
    string INamed.Name { get; set; }
    public string First { get; set; }
    public string Second { get; set; }
  }

  public class O : N, IComposite
  {
    string IComposite.First { get; set; }
  }
}