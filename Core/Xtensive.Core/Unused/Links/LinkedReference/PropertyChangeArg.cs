// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created:    2007.07.02

namespace Xtensive.Core.Links.LinkedReference
{
  public struct PropertyChangeArg<TOwner, TProperty>
  {
    public TProperty NewValue;
    public TProperty OldValue;
    public TOwner Owner;

    public PropertyChangeArg(TOwner owner, TProperty oldValue, TProperty newValue)
    {
      Owner = owner;
      OldValue = oldValue;
      NewValue = newValue;
    }
  }
}