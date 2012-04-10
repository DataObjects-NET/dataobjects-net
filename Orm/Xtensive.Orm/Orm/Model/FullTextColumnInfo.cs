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
    /// <summary>
    /// Gets the column.
    /// </summary>
    public ColumnInfo Column { get; private set; }

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    /// <value>
    /// The configuration.
    /// </value>
    public string Configuration { get; set; }

    /// <summary>
    /// Gets or sets the type column.
    /// </summary>
    /// <value>
    /// The type column.
    /// </value>
    public ColumnInfo TypeColumn { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is analyzed.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is analyzed; otherwise, <c>false</c>.
    /// </value>
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