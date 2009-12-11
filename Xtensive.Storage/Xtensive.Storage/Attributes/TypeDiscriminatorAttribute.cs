// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.08.13

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// Marks persistent property as type discriminator.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public sealed class TypeDiscriminatorAttribute : StorageAttribute
  {
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public TypeDiscriminatorAttribute()
    {
    }
  }
}