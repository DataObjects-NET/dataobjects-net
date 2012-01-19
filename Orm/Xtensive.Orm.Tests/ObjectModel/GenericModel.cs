// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.07

using System;

namespace Xtensive.Orm.Tests.ObjectModel.GenericModel
{
  [Serializable]
  [HierarchyRoot]
  public class A<T> : Entity 
    where T : A<T>
  {
    [Field]
    public T Generic { get; private set; }
  }

  [Serializable]
  public class B : A<B>
  {
    
  }

  [Serializable]
  public class C<T> : A<T>
    where T : A<T>
  {
    
  }
}