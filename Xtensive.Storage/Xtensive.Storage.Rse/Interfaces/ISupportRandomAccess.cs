// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

namespace Xtensive.Storage.Rse
{
  public interface ISupportRandomAccess<T>
  {
    T this[int index] { get; }
  }
}