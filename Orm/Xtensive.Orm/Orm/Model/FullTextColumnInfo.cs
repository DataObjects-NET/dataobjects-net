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
  /// Describes single column in full-text index.
  /// </summary>
  [Serializable]
  public sealed class FullTextColumnInfo : Node
  {
    public ColumnInfo Column { get; private set; }
    public string Configuration { get; set; }
    public ColumnInfo TypeColumn { get; set; }
    public bool IsAnalyzed { get; set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public FullTextColumnInfo(ColumnInfo column)
      : base(column.Name)
    {
      Column = column;
    }
  }
}