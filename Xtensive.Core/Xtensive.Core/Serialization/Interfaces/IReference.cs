// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.27

namespace Xtensive.Core.Serialization
{
  public interface IReference
  {
    bool IsEmpty { get; }

    long Value { get; }

    bool IsResolved { get; }

    object Resolve();

    T Resolve<T>();
  }
}