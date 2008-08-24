// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Kononchuk
// Created:    2008.03.28

using System;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Base class for writers of serialized data.
  /// </summary>
  public abstract class RecordWriter : IDisposable
  {
    /// <summary>
    /// Writes <see cref="Record"/> to the end of the stream.
    /// </summary>
    /// <param name="record">Record for recording.</param>
    public abstract void Append(Record record);

    /// <summary>
    /// Creates a new instance of <see cref="Record"/>.
    /// </summary>
    /// <param name="reference"><see cref="Record.Reference"/> of new <see cref="Record"/>.</param>
    /// <param name="name">Name of the new <see cref="Record"/>.</param>
    /// <returns>New instance of <see cref="Record"/>.</returns>
    public abstract Record CreateRecord(string name, IReference reference);

    /// <inheritdoc/>
    public abstract void Dispose();
  }
}