// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.14

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// Indicates that persistent property must be included into full-text index.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public sealed class FullTextAttribute : StorageAttribute
  {
    /// <summary>
    /// Gets or sets a value indicating whether content of the field marked by this
    /// attribute must be analyzed or not. Analyzed implies it will be splat to a
    /// sequence of words; otherwise it will be represented as a single word
    /// in index.
    /// </summary>
    public bool Analyze { get; set; }

    /// <summary>
    /// Gets the configuration name for word-breaker and stemmer. 
    /// </summary>
    public string Configuration { get; private set; }

  
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public FullTextAttribute(string configuration)
    {
      Configuration = configuration;
      Analyze = true;
    }
  }
}