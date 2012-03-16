// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.26

using System;
using System.IO;
using System.Runtime.Serialization;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Serialization.Implementation
{
  /// <summary>
  /// Exposes <see cref="ISerializer"/> as .Net <see cref="IFormatter"/>.
  /// </summary>
  [Serializable]
  public class SerializerAsFormatter : IFormatter
  {
    private readonly StreamingContext streamingContext = new StreamingContext(StreamingContextStates.Other);

    /// <summary>
    /// Gets the wrapped serializer.
    /// </summary>
    protected ISerializer Serializer { get; private set; }

    #region Properties: Binder, Context, SurrogateSelector

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown on attempt to set this property.</exception>
    public SerializationBinder Binder
    {
      get {
        var sb = Serializer as SerializerBase;
        if (sb!=null && sb.Configuration!=null)
          return sb.Configuration.Binder;
        return null;
      }
      set { throw new NotSupportedException(); }
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown on attempt to set this property.</exception>
    public StreamingContext Context {
      get { return streamingContext; }
      set { throw new NotSupportedException(); }
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown on attempt to set this property.</exception>
    public ISurrogateSelector SurrogateSelector {
      get { return null; }
      set { throw new NotSupportedException(); }
    }

    #endregion

    /// <inheritdoc/>
    public object Deserialize(Stream stream)
    {
      return Serializer.Deserialize(stream);
    }

    /// <inheritdoc/>
    public void Serialize(Stream stream, object graph)
    {
      Serializer.Serialize(stream, graph);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="serializer">The <see cref="Serializer"/> property value.</param>
    public SerializerAsFormatter(ISerializer serializer)
    {
      Serializer = serializer;
    }
  }
}