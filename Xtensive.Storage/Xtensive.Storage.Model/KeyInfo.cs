// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.02.13

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public sealed class KeyInfo : Node
  {
    /// <summary>
    /// Gets the fields that are included in the key.
    /// </summary>
    public DirectionCollection<FieldInfo> Fields { get; private set; }

    /// <summary>
    /// Gets the columns that are included in the key.
    /// </summary>
    public ColumnInfoCollection Columns { get; private set; }


    public int Length
    {
      get { return Columns.Count; }
    }

    /// <summary>
    /// Gets the tuple descriptor of the key.
    /// </summary>
    /// <value></value>
    public TupleDescriptor TupleDescriptor { get; private set; }

    /// <inheritdoc/>
    public override void UpdateState(bool recursive)
    {
      base.UpdateState(recursive);
      TupleDescriptor = TupleDescriptor.Create(
        from c in Columns select c.ValueType);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public KeyInfo()
    {
      Columns = new ColumnInfoCollection();
      Fields = new DirectionCollection<FieldInfo>();
    }
  }
}