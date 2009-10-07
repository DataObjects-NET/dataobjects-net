// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.07

using System;
using System.Diagnostics;

namespace Xtensive.Storage.Tests.ObjectModel.GenericModel
{
  [HierarchyRoot]
  public class A<T> : Entity 
    where T : A<T>
  {
    [Field]
    public T Generic { get; private set; }
  }

  public class B : A<B>
  {
    
  }

  public class C<T> : A<T>
    where T : A<T>
  {
    
  }
}