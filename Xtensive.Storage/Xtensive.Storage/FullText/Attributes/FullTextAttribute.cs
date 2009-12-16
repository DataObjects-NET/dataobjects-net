// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.14

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Fulltext.Attributes
{
  /// <summary>
  /// Marks persistent property as fulltext indexed.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public class FullTextAttribute : StorageAttribute
  {
    /// <summary>
    /// Gets or sets a value indicating whether the field marked by <see cref="FullTextAttribute"/> is analyzed.
    /// </summary>
    /// <remarks>Has <see langword="true" /> value by default.</remarks>
    public bool Analyzed { get; set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public FullTextAttribute()
    {
      Analyzed = true;
    }
  }
}