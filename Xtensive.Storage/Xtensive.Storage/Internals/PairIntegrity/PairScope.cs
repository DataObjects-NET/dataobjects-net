// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.08

using Xtensive.Core;

namespace Xtensive.Storage.PairIntegrity
{
  internal class PairScope : Scope<PairContext>
  {
    /// <summary>
    /// Gets the context.
    /// </summary>
    public new static PairContext Context
    {
      get { return CurrentContext; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PairScope"/> class.
    /// </summary>
    /// <param name="pairContext">The context.</param>
    public PairScope(PairContext pairContext)
      : base(pairContext)
    {
    }
  }
}