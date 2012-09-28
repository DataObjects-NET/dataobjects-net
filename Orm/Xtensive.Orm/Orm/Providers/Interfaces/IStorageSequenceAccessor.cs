// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.09

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Accessor for native storage sequences.
  /// </summary>
  public interface IStorageSequenceAccessor
  {
    /// <summary>
    /// Gets next range of sequental values.
    /// </summary>
    /// <param name="sequenceInfo">Sequence that should be used.</param>
    /// <param name="session">Current session.</param>
    /// <returns>Next range of sequental value.</returns>
    Segment<long> NextBulk(SequenceInfo sequenceInfo, Session session);

    /// <summary>
    /// Performs clean up on the specified <paramref name="sequences"/>.
    /// This method does nothing is storage does not use key generator tables.
    /// </summary>
    /// <param name="sequences">Sequences to clean up.</param>
    /// <param name="session">Session to use.</param>
    void CleanUp(IEnumerable<SequenceInfo> sequences, Session session);
  }
}