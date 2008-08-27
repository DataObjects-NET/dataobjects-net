// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.26

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposable;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;
using Xtensive.Core.Serialization.Implementation;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// The context of a single serialization / deserialization operation.
  /// </summary>
  public abstract class SerializationContext : Context<SerializationScope>, IDisposable
  {
    /// <summary>
    /// Gets the current <see cref="SerializationContext"/>.
    /// </summary>        
    public static SerializationContext Current {
      [DebuggerStepThrough]
      get { return Scope<SerializationContext>.CurrentContext; }
    }

    /// <summary>
    /// Gets current <see cref="Serializer"/>.
    /// </summary>
    public SerializerBase Serializer { get; protected set; }

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    /// <value>The configuration.</value>
    public SerializerConfiguration Configuration { get; protected set; }

    /// <summary>
    /// Gets the current <see cref="Serializer"/> process type.
    /// </summary>
    public SerializerProcessType ProcessType { get; protected set; }

    /// <summary>
    /// Gets the stream used in current process.
    /// </summary>
    public Stream Stream { get; protected set; }

    /// <summary>
    /// Gets the current <see cref="SerializerConfiguration.PreferNesting"/> value.
    /// </summary>
    public bool PreferNesting { get; set; }

    /// <summary>
    /// Gets the current <see cref="SerializerConfiguration.PreferAttributes"/> value.
    /// </summary>
    public bool PreferAttributes { get; set; }

    /// <summary>
    /// Gets the current <see cref="SerializationData"/> reader.
    /// </summary>
    public SerializationDataReader Reader { get; protected set; }

    /// <summary>
    /// Gets the current <see cref="SerializationData"/> writer.
    /// </summary>
    public SerializationDataWriter Writer { get; protected set; }

    /// <summary>
    /// Gets the stack of <see cref="SerializationData"/> objects 
    /// that are currently serialized or deserialized.
    /// </summary>
    public Stack<SerializationData> Path { get; protected set; }

    /// <summary>
    /// Gets the <see cref="ReferenceManager"/> managing <see cref="IReference"/>s 
    /// in this context.
    /// </summary>
    public ReferenceManager ReferenceManager { get; protected set; }

    /// <summary>
    /// Gets the deserialization map.
    /// </summary>
    public IDictionary<IReference, object> DeserializationMap { get; protected set; }

    /// <summary>
    /// Gets the serialization queue.
    /// </summary>
    public ITopDeque<object, Pair<IReference, object>> SerializationQueue { get; protected set; }

    /// <summary>
    /// Gets a <see cref="FixupManager"/> of field recovery
    /// actions for objects which fields haven't already deserialized.
    /// </summary>
    public FixupManager FixupManager { get; protected set; }

    /// <exception cref="InvalidOperationException">Current formatter process type 
    /// differs from the <paramref name="expectedProcessType"/>.</exception>
    public void EnsureFormatterProcessTypeIs(SerializerProcessType expectedProcessType) 
    {
      if (ProcessType!=expectedProcessType)
        throw new InvalidOperationException(string.Format(
          Strings.ExInvalidFormatterProcessType,
          ProcessType));
    }

    #region IContext<...> methods

    /// <inheritdoc/>
    public override bool IsActive {
      get { return SerializationScope.CurrentContext==this; }
    }

    /// <inheritdoc/>
    protected override SerializationScope CreateActiveScope() 
    {
      return new SerializationScope(this);
    }

    #endregion

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    protected virtual void Initialize()
    {
      PreferNesting = Configuration.PreferNesting;
      PreferAttributes = Configuration.PreferAttributes;
      Path = Path ?? new Stack<SerializationData>();
      ReferenceManager = ReferenceManager ?? new ReferenceManager();
      switch (ProcessType) {
      case SerializerProcessType.Serialization:
        SerializationQueue = SerializationQueue ?? new TopDeque<object, Pair<IReference, object>>();
        break;
      case SerializerProcessType.Deserialization:
        FixupManager = FixupManager ?? new FixupManager();
        DeserializationMap = DeserializationMap ?? new Dictionary<IReference, object>();
        break;
      }
    }


    // Constructors

    /// <summary>
    /// Initializes the context for specified process type.
    /// </summary>
    /// <param name="serializer">The <see cref="Serializer"/> property value.</param>
    /// <param name="processType">Type of the process to prepare for.</param>
    /// <param name="stream">The stream to use.</param>
    /// <exception cref="NotSupportedException">Context is already initialized.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="processType"/> is
    /// <see cref="SerializerProcessType.None"/>.</exception>
    public SerializationContext(SerializerBase serializer, Stream stream, SerializerProcessType processType)
    {
      ArgumentValidator.EnsureArgumentNotNull(serializer, "serializer");
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");
      switch (processType) {
      case SerializerProcessType.Serialization:
        break;
      case SerializerProcessType.Deserialization:
        break;
      default:
        throw new ArgumentOutOfRangeException("processType");
      }
      Serializer = serializer;
      Configuration = serializer.Configuration;
      Stream = stream;
      ProcessType = processType;
      Initialize();
    }

    /// <see cref="ClassDocTemplate.Dispose" copy="true" />
    public virtual void Dispose()
    {
      try {
        Writer.DisposeSafely();
      }
      finally {
        Writer = null;
      }
    }
  }
}