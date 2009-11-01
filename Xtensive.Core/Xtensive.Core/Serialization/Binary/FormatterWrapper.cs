// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

using System;
using System.IO;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Base class for serializers implemented by
  /// wrapping the .Net <see cref="IFormatter"/>.
  /// </summary>
  public class FormatterWrapper: IValueSerializer
  {
    private readonly IFormatter formatter;

    /// <summary>
    /// Gets the wrapped formatter.
    /// </summary>
    protected IFormatter Formatter
    {
      get { return formatter; }
    }

    /// <inheritdoc/>
    public object Deserialize(Stream stream)
    {
      return formatter.Deserialize(stream);
    }

    /// <inheritdoc/>
    public void Serialize(Stream stream, object graph)
    {
      formatter.Serialize(stream, graph);
    }

    #region IContext<SerializationScope> Members

    /// <inheritdoc/>
    public bool IsActive
    {
      get { return ValueSerializationScope.CurrentSerializer==this; }
    }

    /// <inheritdoc/>
    IDisposable IContext.Activate()
    {
      return Activate();
    }

    /// <inheritdoc/>
    public ValueSerializationScope Activate()
    {
      return IsActive ? null : new ValueSerializationScope(this);
    }

    #endregion

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="formatter">Initial <see cref="Formatter"/> property value.</param>
    public FormatterWrapper(IFormatter formatter)
    {
      this.formatter = formatter;
    }
  }
}