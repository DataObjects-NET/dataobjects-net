// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.30

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// An attribute describing the recycled type or property.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property, 
    AllowMultiple = false, Inherited = false)]
  public class RecycledAttribute : Attribute
  {
    /// <summary>
    /// Gets or sets the original name of the type or property.
    /// </summary>
    public string OriginalName { get; set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public RecycledAttribute()
    {
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="originalName">The original name of the type or property.</param>
    public RecycledAttribute(string originalName)
    {
      OriginalName = originalName;
    }
  }
}