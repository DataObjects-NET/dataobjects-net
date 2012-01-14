// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved_
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.09.24

using System;
using System.Diagnostics;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tests.ObjectModel.Interfaces.Alphabet
{
  [Index("Name")]
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

  [Index("First")]
  public interface IComposite : IEntity
  {
    [Field]
    string First { get; set; }

    [Field]
    string Second { get; set; }
  }

  [Serializable]
  [HierarchyRoot(InheritanceSchema.ClassTable)]
  public class A : Entity, INamed
  {
    [Field, Key]
    public long Id { get; private set; }
    public string Name { get; set; }
  }

  [Serializable]
  public class B : A, INamed, ITagged
  {
    string INamed.Name { get; set; }
    public string Tag { get; set; }
  }

  [Serializable]
  public class C : A, ITagged
  {
    string ITagged.Tag { get; set; }
  }

  [Serializable]
  public class D : B, INamed, IComposite
  {
    string INamed.Name { get; set; }
    public string First { get; set; }
    public string Second { get; set;}
  }

  [Serializable]
  public class E : D, IComposite
  {
    string IComposite.First { get; set; }
  }

  [Serializable]
  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public class F : Entity, INamed
  {
    [Field, Key]
    public long Id { get; private set; }
    public string Name { get; set; }
  }

  [Serializable]
  public class  G : F, INamed, ITagged
  {
    string INamed.Name { get; set; }
    public string Tag { get; set; }
  }

  [Serializable]
  public class H : F, ITagged
  {
    string ITagged.Tag { get; set; }
  }

  [Serializable]
  public class I : G, INamed, IComposite
  {
    string INamed.Name { get; set; }
    public string First { get; set; }
    public string Second { get; set; }
  }

  [Serializable]
  public class J : I, IComposite
  {
    string IComposite.First { get; set; }
  }

  [Serializable]
  [HierarchyRoot(InheritanceSchema.SingleTable)]
  public class K : Entity, INamed
  {
    [Field, Key]
    public long Id { get; private set; }
    public string Name { get; set; }
  }

  [Serializable]
  public class L : K, INamed, ITagged
  {
    string INamed.Name { get; set; }
    public string Tag { get; set; }
  }

  [Serializable]
  public class M : K, ITagged
  {
    string ITagged.Tag { get; set; }
  }

  [Serializable]
  public class N : L, INamed, IComposite
  {
    string INamed.Name { get; set; }
    public string First { get; set; }
    public string Second { get; set; }
  }

  [Serializable]
  public class O : N, IComposite
  {
    string IComposite.First { get; set; }
  }

  [Serializable]
  [HierarchyRoot(InheritanceSchema.ClassTable)]
  [TypeDiscriminatorValue(0, Default = true)]
  public class P : Entity, INamed
  {
    [Field, Key]
    public long Id { get; private set; }
    
    [Field, TypeDiscriminator]
    public int ElementType { get; private set; }

    public string Name { get; set; }
  }

  [Serializable]
  [TypeDiscriminatorValue(1)]
  public class Q : P, INamed, ITagged
  {
    string INamed.Name { get; set; }
    public string Tag { get; set; }
  }

  [Serializable]
  [TypeDiscriminatorValue(2)]
  public class R : P, ITagged
  {
    string ITagged.Tag { get; set; }
  }

  [Serializable]
  [TypeDiscriminatorValue(3)]
  public class S : Q, INamed, IComposite
  {
    string INamed.Name { get; set; }
    public string First { get; set; }
    public string Second { get; set; }
  }

  [Serializable]
  [TypeDiscriminatorValue(4)]
  public class T : S, IComposite
  {
    string IComposite.First { get; set; }
  }

  [Serializable]
  [HierarchyRoot(InheritanceSchema.SingleTable)]
  [TypeDiscriminatorValue(0, Default = true)]
  public class U : Entity, INamed
  {
    [Field, Key]
    public long Id { get; private set; }
    
    [Field, TypeDiscriminator]
    public int ElementType { get; private set; }

    public string Name { get; set; }
  }

  [Serializable]
  [TypeDiscriminatorValue(1)]
  public class V : U, INamed, ITagged
  {
    string INamed.Name { get; set; }
    public string Tag { get; set; }
  }

  [Serializable]
  [TypeDiscriminatorValue(2)]
  public class W : U, ITagged
  {
    string ITagged.Tag { get; set; }
  }

  [Serializable]
  [TypeDiscriminatorValue(3)]
  public class X : V, INamed, IComposite
  {
    string INamed.Name { get; set; }
    public string First { get; set; }
    public string Second { get; set; }
  }

  [Serializable]
  [TypeDiscriminatorValue(4)]
  public class Y : X, IComposite
  {
    string IComposite.First { get; set; }
  }
}