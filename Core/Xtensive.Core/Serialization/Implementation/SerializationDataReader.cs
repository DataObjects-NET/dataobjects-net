// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.28

using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Serialization.Implementation
{
  /// <summary>
  /// Abstract base class any <see cref="SerializationData"/> reader.
  /// </summary>
  public abstract class SerializationDataReader : IEnumerable<SerializationData>
  {
    /// <inheritdoc/>
    public abstract IEnumerator<SerializationData> GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() 
    {
      return GetEnumerator();
    }
  }
}