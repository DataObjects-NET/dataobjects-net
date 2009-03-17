// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes index change operation.
  /// </summary>
  [Serializable]
  public class IndexChangeInfo
  {
    /// <summary>
    /// Gets the name of the source index.
    /// </summary>
    public string SourceName { get; private set; }

    /// <summary>
    /// Gets the target index model.
    /// </summary>
    public IndexInfo Target { get; private set; }

    /// <summary>
    /// Gets the column mapping.
    /// </summary>
    public ReadOnlyDictionary<string, string> ColumnMapping { get; private set; }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="sourceName">The name of the source index.</param>
    /// <param name="target">The target index model.</param>
    /// <param name="columnMapping">The column mapping.</param>
    public IndexChangeInfo(string sourceName, IndexInfo target, IDictionary<string, string> columnMapping)
    {
      SourceName = sourceName;
      Target = target;
      ColumnMapping = new ReadOnlyDictionary<string, string>(columnMapping, true);
    }
  }
}