// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.18

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public class FullTextColumnInfo : Node
  {
    public ColumnInfo Column { get; private set; }
    public string Configuration { get; set; }
    public ColumnInfo TypeColumn { get; set; }
    public bool IsAnalyzed { get; set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public FullTextColumnInfo(ColumnInfo column)
      : base(column.Name)
    {
      Column = column;
    }
  }
}