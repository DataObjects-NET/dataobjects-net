// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.01

namespace Xtensive.Core
{
  public interface ISubstitutable<T>
  {
    T Substitution { get; }
  }
}