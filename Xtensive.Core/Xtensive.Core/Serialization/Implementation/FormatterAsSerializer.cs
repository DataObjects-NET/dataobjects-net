// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

using System;
using System.IO;
using System.Runtime.Serialization;
using Xtensive.Serialization.Implementation;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Serialization.Implementation
{
  /// <summary>
  /// Exposes .Net <see cref="IFormatter"/> as <see cref="ISerializer"/>.
  /// </summary>
  [Serializable]
  public class FormatterAsSerializer : SerializerBase
  {
    /// <summary>
    /// Gets the wrapped formatter.
    /// </summary>
    protected IFormatter Formatter { get; private set; }

    /// <inheritdoc/>
    public override void Serialize(Stream stream, object source, object origin)
    {
      Formatter.Serialize(stream, source);
    }

    /// <inheritdoc/>
    public override object Deserialize(Stream stream, object origin)
    {
      return Formatter.Deserialize(stream);
    }

    /// <inheritdoc/>
    protected override void OnConfigured()
    {
      base.OnConfigured();
      Formatter.Binder = Configuration.Binder;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="formatter">The <see cref="Formatter"/> property value.</param>
    public FormatterAsSerializer(IFormatter formatter) 
    {
      Formatter = formatter;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="formatter">The <see cref="Formatter"/> property value.</param>
    /// <param name="configuration">The configuration.</param>
    public FormatterAsSerializer(IFormatter formatter, SerializerConfiguration configuration)
      : this (formatter)
    {
      Configure(configuration);
    }
  }
}