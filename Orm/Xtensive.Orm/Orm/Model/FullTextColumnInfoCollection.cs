// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.18

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// A collection of <see cref="FullTextColumnInfo"/> objects.
  /// </summary>
  [Serializable]
  public sealed class FullTextColumnInfoCollection : NodeCollection<FullTextColumnInfo>
  {
    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"></exception>
    public override void Insert(int index, FullTextColumnInfo value)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"></exception>
    public override void RemoveAt(int index)
    {
      throw new NotSupportedException();
    }


    // Constructors

    /// <inheritdoc/>
    public FullTextColumnInfoCollection(Node owner, string name)
      : base(owner, name)
    {
    }
  }
}