// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.11

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Descriptor of the calculated column.
  /// </summary>
  public class AggregateColumnDescriptor
  {
    /// <summary>
    /// Gets the column index.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the column index.
    /// </summary>
    public int SourceIndex { get; private set; }

    /// <summary>
    /// Gets the column type.
    /// </summary>
    public AggregateType AggregateType { get; private set; }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name"><see cref="Name"/> property value.</param>
    /// <param name="index"><see cref="SourceIndex"/> property value.</param>
    /// <param name="aggregateType">The <see cref="AggregateType"/> property value.</param>
    public AggregateColumnDescriptor(string name, int index, AggregateType aggregateType)
    {
      Name = name;
      SourceIndex = index;
      AggregateType = aggregateType;
    }
    
  }
}