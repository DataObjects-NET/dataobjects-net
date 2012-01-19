// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.26

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Disposing;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;
using Xtensive.Resources;
using Xtensive.Serialization.Implementation;

namespace Xtensive.Serialization
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
    public WorkingSerializerBase Serializer { get; protected set; }

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
    /// Gets the <see cref="TokenManager"/> managing <see cref="Token"/>s 
    /// in this context.
    /// </summary>
    public TokenManager TokenManager { get; protected set; }

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

    #region Shortcut-like properties

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    /// <value>The configuration.</value>
    public SerializerConfiguration Configuration { get; protected set; }

    /// <summary>
    /// Gets or sets the object serializer provider.
    /// </summary>
    public IObjectSerializerProvider ObjectSerializerProvider { get; protected set; }

    /// <summary>
    /// Gets or sets the value serializer provider.
    /// </summary>
    public IValueSerializerProvider ValueSerializerProvider { get; protected set; }

    #endregion

    #region Cached value serializers

    /// <summary>
    /// Gets the <see cref="bool"/> value serializer.
    /// </summary>
    public ValueSerializer<bool> BooleanSerializer { get; protected set; }

    /// <summary>
    /// Gets the <see cref="byte"/> value serializer.
    /// </summary>
    public ValueSerializer<byte> ByteSerializer { get; protected set; }

    /// <summary>
    /// Gets the <see cref="short"/> value serializer.
    /// </summary>
    public ValueSerializer<short> Int16Serializer { get; protected set; }

    /// <summary>
    /// Gets the <see cref="int"/> value serializer.
    /// </summary>
    public ValueSerializer<int> Int32Serializer { get; protected set; }

    /// <summary>
    /// Gets the <see cref="long"/> value serializer.
    /// </summary>
    public ValueSerializer<long> Int64Serializer { get; protected set; }

    /// <summary>
    /// Gets the value serializer for <see cref="string"/> type.
    /// </summary>
    public ValueSerializer<string> StringSerializer { get; protected set; }

    /// <summary>
    /// Gets the value serializer for <see cref="Type"/> type.
    /// </summary>
    public ValueSerializer<Type> TypeSerializer { get; protected set; }

    /// <summary>
    /// Gets the value serializer for <see cref="Token{T}"/> of <see cref="string"/> type.
    /// </summary>
    public ValueSerializer<Token<string>> StringTokenSerializer { get; protected set; }

    /// <summary>
    /// Gets the value serializer for <see cref="Token{T}"/> of <see cref="Type"/> type.
    /// </summary>
    public ValueSerializer<Token<Type>> TypeTokenSerializer { get; protected set; }

    #endregion

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

    #region EnsureXxx methods

    /// <exception cref="InvalidOperationException">Current formatter process type 
    /// differs from the <paramref name="expectedProcessType"/>.</exception>
    public void EnsureProcessTypeIs(SerializerProcessType expectedProcessType) 
    {
      if (ProcessType!=expectedProcessType)
        throw new InvalidOperationException(string.Format(
          Strings.ExInvalidFormatterProcessType,
          ProcessType));
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
      TokenManager = TokenManager ?? new TokenManager();
      ReferenceManager = ReferenceManager ?? new ReferenceManager();
      switch (ProcessType) {
      case SerializerProcessType.Serialization:
        SerializationQueue = SerializationQueue ?? 
          new TopDeque<object, Pair<IReference, object>>(ReferenceEqualityComparer<object>.Instance);
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
    public SerializationContext(WorkingSerializerBase serializer, Stream stream, SerializerProcessType processType)
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
      Stream = stream;
      ProcessType = processType;

      // Shortcut properties
      Configuration = serializer.Configuration;
      ObjectSerializerProvider = Serializer.ObjectSerializerProvider;
      ValueSerializerProvider = Serializer.ValueSerializerProvider;
      var vsp = ValueSerializerProvider;
      BooleanSerializer = vsp.GetSerializer<bool>();
      ByteSerializer = vsp.GetSerializer<byte>();
      Int16Serializer = vsp.GetSerializer<short>();
      Int32Serializer = vsp.GetSerializer<int>();
      Int64Serializer = vsp.GetSerializer<long>();
      StringSerializer = vsp.GetSerializer<string>();
      TypeSerializer = vsp.GetSerializer<Type>();
      StringTokenSerializer = vsp.GetSerializer<Token<string>>();
      TypeTokenSerializer = vsp.GetSerializer<Token<Type>>();

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