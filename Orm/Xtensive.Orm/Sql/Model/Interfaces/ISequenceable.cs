// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Defines an object that could be the source of a sequence.
  /// </summary>
  public interface ISequenceable
  {
    /// <summary>
    /// Gets or sets the sequence descriptor.
    /// </summary>
    /// <value>The sequence descriptor.</value>
    SequenceDescriptor SequenceDescriptor { get; set; }
  }
}
