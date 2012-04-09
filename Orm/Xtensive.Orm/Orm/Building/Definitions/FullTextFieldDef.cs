// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.18

using System;
using System.Diagnostics;

using Xtensive.Orm.Model;

namespace Xtensive.Orm.Building.Definitions
{
  /// <summary>
  /// Defines a single field inside full-text index.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{Name}")]
  public class FullTextFieldDef : Node
  {
    /// <summary>
    /// Gets or sets the configuration for word-breaker and stemmer.
    /// </summary>
    /// <value>The configuration name.</value>
    public string Configuration { get; set; }

    /// <summary>
    /// Gets or sets the name of the type field.
    /// </summary>
    public string TypeFieldName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this field is analyzed by stemmer.
    /// </summary>
    public bool IsAnalyzed { get; set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public FullTextFieldDef(string fieldName, bool isAnalyzed)
      : base(fieldName)
    {
      IsAnalyzed = isAnalyzed;
    }
  }
}