// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.08

namespace Xtensive.Storage.PairIntegrity
{
  internal class PairContext : Core.Context<PairScope>
  {
    /// <inheritdoc/>
    protected override PairScope CreateActiveScope()
    {
      return new PairScope(this);
    }

    /// <inheritdoc/>
    public override bool IsActive
    {
      get { return PairScope.Context==this; }
    }
  }
}