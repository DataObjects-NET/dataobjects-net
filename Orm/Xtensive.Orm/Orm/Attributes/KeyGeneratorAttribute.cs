// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.26

using System;
using Xtensive.Core;


namespace Xtensive.Orm
{
  /// <summary>
  /// Configures <see cref="KeyGenerator"/> for the hierarchy.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  public sealed class KeyGeneratorAttribute : StorageAttribute
  {
    /// <summary>
    /// Gets the key generator type.
    /// </summary>
    public Type Type { get; private set; }

    /// <summary>
    /// Gets the kind of key generator.
    /// </summary>
    public KeyGeneratorKind Kind {
      get {
        var type = Type;
        if (type==null && Name.IsNullOrEmpty()) 
          return KeyGeneratorKind.None;
        if (type==typeof(KeyGeneratorKind) && Name.IsNullOrEmpty())
          return KeyGeneratorKind.Default;
        return KeyGeneratorKind.Custom;
      }
    }

    /// <summary>
    /// Gets or sets the name of the key generator.
    /// </summary>
    public string Name { get; set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public KeyGeneratorAttribute()
      : this(typeof(KeyGenerator))
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">The generator type. Must be inherited from the <see cref="KeyGenerator"/> type</param>
    /// <remarks><paramref name="type"/> can be null.</remarks>
    public KeyGeneratorAttribute(Type type)
    {
      Type = type;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="keyGeneratorKind">Kind of the key generator.</param>
    /// <exception cref="ArgumentOutOfRangeException"><c>keyGeneratorKind</c> cannot be 
    /// <see cref="KeyGeneratorKind.Custom"/> here.</exception>
    public KeyGeneratorAttribute(KeyGeneratorKind keyGeneratorKind)
      : this(keyGeneratorKind==KeyGeneratorKind.None ? null : typeof(KeyGenerator))
    {
      if (keyGeneratorKind==KeyGeneratorKind.Custom)
        throw new ArgumentOutOfRangeException("keyGeneratorKind");
    }
  }
}