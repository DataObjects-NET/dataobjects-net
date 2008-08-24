// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.28

using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Base class for reading from serialized data.
  /// </summary>
  public abstract class RecordReader : IEnumerable<Record>
  {
    /// <inheritdoc/>
    public abstract IEnumerator<Record> GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
  }
}