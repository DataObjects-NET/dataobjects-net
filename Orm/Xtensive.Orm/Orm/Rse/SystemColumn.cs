// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.05

using System;


namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// System column of the <see cref="RecordSetHeader"/>.
  /// </summary>
  [Serializable]
  public class SystemColumn : Column
  {
    
    public override Column Clone(int newIndex)
    {
      return new SystemColumn(this, newIndex);
    }

    
    public override Column Clone(string newName)
    {
      return new SystemColumn(this, newName);
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="name"><see cref="Column.Name"/> property value.</param>
    /// <param name="index"><see cref="Column.Index"/> property value.</param>
    /// <param name="type"><see cref="Column.Type"/> property value.</param>
    public SystemColumn(string name, int index, Type type)
      : base(name, index, type)
    {
    }

    #region Clone constructors

    private SystemColumn(SystemColumn column, int newIndex)
      : base(column.Name, newIndex, column.Type)
    {
    }

    private SystemColumn(SystemColumn column, string newName)
      : base(newName, column.Index, column.Type)
    {
    }

    #endregion
  }
}