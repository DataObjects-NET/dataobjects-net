// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.24

using System;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Providers
{
  [Serializable]
  public abstract class IncrementalGeneratorHandler : HandlerBase
  {
    /// <summary>
    /// Fills the specified tuple with unique values.
    /// </summary>
    /// <param name="tuple">The tuple.</param>
    public abstract void Fill(Tuple tuple);
  }
}