// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.26

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// Optional attribute that is responsible for key generator creation and configuration.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  public class KeyGeneratorAttribute : StorageAttribute
  {
    internal int? cacheSize;

    /// <summary>
    /// Gets or sets the key generator type.
    /// </summary>
    /// <exception cref="InvalidOperationException">Value is not inherited from <see cref="KeyGenerator"/> type.</exception>
    public Type Type { get; private set; }

    /// <summary>
    /// Gets the kind of key generator.
    /// </summary>
    public KeyGeneratorKind Kind {
      get {
        var type = Type;
        if (type==null) 
          return KeyGeneratorKind.None;
        if (type==typeof(KeyGeneratorKind))
          return KeyGeneratorKind.Default;
        return KeyGeneratorKind.Custom;
      }
    }

    /// <summary>
    /// Gets or sets the size of the key generator cache.
    /// </summary>
    public int CacheSize
    {
      get { return cacheSize.HasValue ? cacheSize.Value : 0; }
      set { cacheSize = value; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public KeyGeneratorAttribute()
      : this(typeof(KeyGenerator))
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The generator type. Must be inherited from the <see cref="KeyGenerator"/> type</param>
    /// <remarks><paramref name="type"/> can be null.</remarks>
    public KeyGeneratorAttribute(Type type)
    {
      Type = type;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
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