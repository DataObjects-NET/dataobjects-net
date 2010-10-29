// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.26

using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using Xtensive.Configuration;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Serialization.Implementation;

namespace Xtensive.Serialization
{
  /// <summary>
  /// Configuration for <see cref="SerializerBase"/>.
  /// </summary>
  [Serializable]
  public class SerializerConfiguration : ConfigurationBase
  {
    private SerializationBinder binder = DefaultSerializationBinder.Instance;
    private FormatterAssemblyStyle assemblyStyle = FormatterAssemblyStyle.Simple;
    private FormatterTypeStyle typeFormat = FormatterTypeStyle.TypesWhenNeeded;
    private bool preferNesting;
    private bool preferAttributes;

    /// <summary>
    /// Gets or sets the <see cref="SerializationBinder"/> that performs type lookups during deserialization.
    /// </summary>
    /// <value>The binder.</value>
    public SerializationBinder Binder {
      get { return binder; }
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        this.EnsureNotLocked();
        binder = value;
      }
    }

    /// <summary>
    /// Gets the behavior of the formatter describing how to find and load assemblies.
    /// </summary>
    public FormatterAssemblyStyle AssemblyStyle
    {
      get { return assemblyStyle; }
      set {
        this.EnsureNotLocked();
        assemblyStyle = value;
      }
    }

    /// <summary>
    /// Gets the format in which type descriptions are stored in the serialized stream.
    /// </summary>
    public FormatterTypeStyle TypeFormat
    {
      get { return typeFormat; }
      set {
        this.EnsureNotLocked();
        typeFormat = value;
      }
    }

    /// <summary>
    /// Specifies when it's preferable to serialize the objects using 
    /// "depth traversal" rather then "wide traversal".
    /// </summary>
    public bool PreferNesting
    {
      get { return preferNesting; }
      set {
        this.EnsureNotLocked();
        preferNesting = value;
      }
    }

    /// <summary>
    /// Specifies when it's preferable to serialize the values into 
    /// the attributes rather then elements 
    /// (works only if supported by the underlying <see cref="ISerializer"/>).
    /// </summary>
    public bool PreferAttributes
    {
      get { return preferAttributes; }
      set {
        this.EnsureNotLocked();
        preferAttributes = value;
      }
    }

    /// <inheritdoc/>
    public override void Validate() 
    {
      if (binder == null)
        binder = DefaultSerializationBinder.Instance;
    }

    #region Cloning-related methods

    /// <inheritdoc/>
    protected override void CopyFrom(ConfigurationBase source)
    {
      base.CopyFrom(source);
      var other = (SerializerConfiguration) source;
      binder = other.binder;
      assemblyStyle = other.assemblyStyle;
      typeFormat = other.typeFormat;
      preferNesting = other.preferNesting;
      preferAttributes = other.preferAttributes;
    }

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      return new SerializerConfiguration();
    }

    #endregion
  }
}