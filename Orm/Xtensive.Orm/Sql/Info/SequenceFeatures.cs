// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Defines a list of sequence related features.
  /// </summary>
  [Flags]
  public enum SequenceFeatures
  {
    /// <summary>
    /// Indicates that RDBMS server does not support anything special
    /// regarding sequences.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Indicates that RDBMS supports caching of sequence values
    /// (i.e. it would be possible to take several sequential values at a time
    /// and then use those values without access to sequence object).
    /// </summary>
    Cache = 0x1,

    /// <summary>
    /// Indicates that RDBMS allows to specify for a sequence an 
    /// order of generated values (ascending or descending).
    /// </summary>
    Order = 0x2,
  }
}
