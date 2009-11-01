// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.28

namespace Xtensive.Core.Serialization
{
  public abstract class RecordReader
  {
    ///<summary>
    /// Advances the <see cref="RecordReader"/> to the next record.
    ///</summary>
    ///<returns><see langword="true"/> if there are more records; otherwise <see langword="false"/>.</returns>
    public abstract bool Read();

    /// <summary>
    /// Gets the current record.
    /// </summary>
    /// <value>The current record.</value>
    public abstract Record GetRecord();
  }
}