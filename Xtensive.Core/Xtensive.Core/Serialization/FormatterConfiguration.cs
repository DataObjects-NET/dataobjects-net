// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.26

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Configuration for <see cref="Formatter"/>.
  /// </summary>
  [Serializable]
  public abstract class FormatterConfiguration : ConfigurationBase
  {
    private SerializationBinder binder;
    private FormatterAssemblyStyle assemblyStyle;
    private FormatterTypeStyle typeFormat;
    private IList<Pair<string, string>> serializerLocations = new List<Pair<string, string>>(2);
    private bool preferNesting;

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
    /// Gets the list of serializer locations.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown on attempt to set this property.</exception>
    public IList<Pair<string, string>> SerializerLocations {
      get { return serializerLocations; }
      set {
        throw Exceptions.AlreadyInitialized("SerializerLocations");
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

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      serializerLocations = new ReadOnlyList<Pair<string, string>>(serializerLocations);
    }

    /// <inheritdoc/>
    public override void Validate() 
    {
      if (binder == null)
        binder = new DefaultSerializationBinder();
    }

    /// <inheritdoc/>
    protected override void Clone(ConfigurationBase source)
    {
      base.Clone(source);
      var other = (FormatterConfiguration) source;
      binder = other.binder;
      assemblyStyle = other.assemblyStyle;
      typeFormat = other.typeFormat;
      serializerLocations = new List<Pair<string, string>>(other.serializerLocations);
      preferNesting = other.preferNesting;
    }
  }
}