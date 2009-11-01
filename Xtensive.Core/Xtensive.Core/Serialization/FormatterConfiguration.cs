// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.26

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using Xtensive.Core.Helpers;

namespace Xtensive.Core.Serialization
{
  [Serializable]
  public abstract class FormatterConfiguration : ConfigurationBase
  {
    private SerializationBinder binder;
    private FormatterAssemblyStyle assemblyStyle;
    private FormatterTypeStyle typeFormat;
    private List<Pair<string, string>> serializerLocations = new List<Pair<string, string>>(2);

    /// <summary>
    /// Gets or sets the <see cref="SerializationBinder"/> that performs type lookups during deserialization.
    /// </summary>
    /// <value>The binder.</value>
    public SerializationBinder Binder
    {
      get { return binder; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        binder = value;
      }
    }

    /// <summary>
    /// Gets or sets the behavior of the deserializer with regards to finding and loading assemblies.
    /// </summary>
    /// <value>One of the <see cref="FormatterAssemblyStyle"/> values that specifies the deserializer behavior.</value>
    public FormatterAssemblyStyle AssemblyStyle
    {
      get { return assemblyStyle; }
      set { assemblyStyle = value; }
    }

    /// <summary>
    /// Gets or sets the format in which type descriptions are laid out in the serialized stream.
    /// </summary>
    /// <value>The format in which type descriptions are laid out in the serialized stream.</value>
    public FormatterTypeStyle TypeFormat
    {
      get { return typeFormat; }
      set { typeFormat = value; }
    }

    /// <summary>
    /// Gets the serializer locations.
    /// </summary>
    /// <value>The serializer locations.</value>
    public List<Pair<string, string>> SerializerLocations
    {
      get { return serializerLocations; }
    }

    /// <inheritdoc/>
    public override void Validate()
    {
      if (binder == null)
        binder = new DefaultSerializationBinder();
    }
  }
}