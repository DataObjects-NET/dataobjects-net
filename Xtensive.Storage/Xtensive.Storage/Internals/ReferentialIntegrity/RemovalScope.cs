// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.02

using Xtensive.Core;

namespace Xtensive.Storage.ReferentialIntegrity
{
  internal class RemovalScope : Scope<RemovalContext>
  {
    /// <summary>
    /// Gets the context.
    /// </summary>
    public new static RemovalContext Context
    {
      get { return CurrentContext; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RemovalScope"/> class.
    /// </summary>
    /// <param name="removalContext">The context.</param>
    public RemovalScope(RemovalContext removalContext)
      : base(removalContext)
    {
    }
  }
}