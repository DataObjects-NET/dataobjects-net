// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.09

using System;
using System.Diagnostics;
using Xtensive.Core;


namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Provides information about sequence associated with <see cref="KeyInfo"/>.
  /// </summary>
  public sealed class SequenceInfo : SchemaMappedNode
  {
    private long seed = 1;
    private long increment = 1;

    /// <summary>
    /// Gets or sets the seed value.
    /// </summary>
    public long Seed {
      [DebuggerStepThrough]
      get { return seed; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        seed = value;
      }
    }

    /// <summary>
    /// Gets or sets the increment value.
    /// </summary>
    public long Increment {
      [DebuggerStepThrough]
      get { return increment; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        increment = value;
      }
    }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="name">The sequence name.</param>
    public SequenceInfo(string name)
      : base(name)
    {
    }
  }
}