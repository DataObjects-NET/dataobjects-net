// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes a sequence.
  /// </summary>
  public class SequenceInfo : EntityInfo
  {
    private SequenceFeatures features = SequenceFeatures.None;

    /// <summary>
    /// Gets or sets the features of this instance.
    /// </summary>
    /// <value>The features.</value>
    public SequenceFeatures Features
    {
      get { return features; }
      set
      {
        EnsureNotLocked();
        features = value;
      }
    }
  }
}
