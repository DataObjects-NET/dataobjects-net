// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.01

using System;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public struct ColumnGroup
  {
    public readonly ReadOnlyList<int> Keys;
    public readonly ReadOnlyList<int> Columns;


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ColumnGroup(IEnumerable<int> keys, IEnumerable<int> columns)
      : this(new List<int>(keys), new List<int>(columns))
    {}

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ColumnGroup(IList<int> keys, IList<int> columns)
    {
      Keys = new ReadOnlyList<int>(keys);
      Columns = new ReadOnlyList<int>(columns);
    }
  }
}