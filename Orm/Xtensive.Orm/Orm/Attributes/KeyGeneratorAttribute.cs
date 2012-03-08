// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.26

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Reflection;

namespace Xtensive.Orm
{
  /// <summary>
  /// Configures key generator for the hierarchy.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  public sealed class KeyGeneratorAttribute : StorageAttribute
  {
    /// <summary>
    /// Gets the kind of key generator.
    /// </summary>
    public KeyGeneratorKind Kind { get; private set; }

    /// <summary>
    /// Gets or sets the name of the key generator.
    /// </summary>
    public string Name { get; set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public KeyGeneratorAttribute()
      : this(KeyGeneratorKind.Default)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The generator type. It should be type implmenting <see cref="IKeyGenerator"/>.</param>
    /// <remarks><paramref name="type"/> can be null.</remarks>
    public KeyGeneratorAttribute(Type type)
    {
      if (type==null)
        Kind = KeyGeneratorKind.None;
      else {
        Kind = KeyGeneratorKind.Custom;
        Name = type.GetShortName();
      }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keyGeneratorKind">Kind of the key generator.</param>
    /// <exception cref="ArgumentOutOfRangeException"><c>keyGeneratorKind</c> cannot be 
    /// <see cref="KeyGeneratorKind.Custom"/> here.</exception>
    public KeyGeneratorAttribute(KeyGeneratorKind keyGeneratorKind)
    {
      Kind = keyGeneratorKind;
    }
  }
}