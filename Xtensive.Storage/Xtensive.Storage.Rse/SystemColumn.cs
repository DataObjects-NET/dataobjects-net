// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.05

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Row number column of the <see cref="RecordSetHeader"/>.
  /// </summary>
  [Serializable]
  public class SystemColumn : Column
  {
    /// <inheritdoc/>
    public override Column Clone(int newIndex)
    {
      return new SystemColumn(this, newIndex);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name"><see cref="Column.Name"/> property value.</param>
    /// <param name="index"><see cref="Column.Index"/> property value.</param>
    /// <param name="type"><see cref="Column.Type"/> property value.</param>
    public SystemColumn(string name, int index, Type type)
      : base(name, index, type)
    {
    }

    private SystemColumn(Column column, int newIndex)
      : base(column.Name, newIndex, column.Type)
    {
    }
  }
}