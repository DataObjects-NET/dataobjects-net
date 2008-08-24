// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.26

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// The context of the serialization / deserialization process.
  /// </summary>
  public abstract class SerializationContext : Context<SerializationScope>
  {
    private readonly Func<Type, string> typeToStringConverter;
    private readonly Dictionary<Type, bool> isObjectOrValueType = new Dictionary<Type, bool>();

    /// <summary>
    /// Gets the current <see cref="SerializationContext"/>.
    /// </summary>        
    public static SerializationContext Current {
      [DebuggerStepThrough]
      get { return Scope<SerializationContext>.CurrentContext; }
    }

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    /// <value>The configuration.</value>
    public FormatterConfiguration Configuration { get; internal set; }

    /// <summary>
    /// Gets the current <see cref="Formatter"/> process type.
    /// </summary>
    public FormatterProcessType ProcessType { get; private set; }

    /// <summary>
    /// Gets <see cref="Dictionary{TKey,TItem}"/> for differentiation
    /// types with <see cref="IObjectSerializer{T}"/> and types with <see cref="IValueSerializer{T}"/>
    /// </summary>
    public Dictionary<Type, bool> IsObjectOrValueType {
      get { return isObjectOrValueType; }
    }

    /// <summary>
    /// Gets a <see cref="Serialization.FixupQueue"/> of field recovery
    /// actions for objects which fields haven't already deserialized.
    /// </summary>
    public FixupQueue FixupQueue { get; private set; }

    /// <summary>
    /// Gets current <see cref="Formatter"/>.
    /// </summary>
    public Formatter Formatter { get; private set; }

    /// <summary>
    /// Gets the stack of <see cref="IReference"/> objects that are currently 
    /// serialized or deserialized.
    /// </summary>
    public Stack<IReference> Path { get; private set; }

    /// <summary>
    /// Gets the <see cref="Queue{T}"/> of <see cref="SerializationData"/> objects 
    /// to be deserialized further.
    /// </summary>
    public Queue<SerializationData> DeserializationQueue { get; private set; }

    /// <summary>
    /// Gets the <see cref="Queue"/> of objects to be serialized further.
    /// </summary>
    public Queue<object> SerializationQueue { get; private set; }

    /// <summary>
    /// Gets the <see cref="ReferenceManager"/> managing <see cref="IReference"/>s 
    /// in this context.
    /// </summary>
    public ReferenceManager ReferenceManager { get; private set; }

    /// <summary>
    /// Gets short id for the type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>Short id of the type.</returns>
    public string GetTypeName(Type type) 
    {
      EnsureFormatterProcessTypeIs(FormatterProcessType.Serialization);
      return typeToStringConverter.Invoke(type);
    }

    /// <summary>
    /// Add types resolved by default to <see cref="ReferenceManager"/>.
    /// </summary>
    /// <param name="types"></param>
    protected void AddResolvedType(params Type[] types) 
    {
      foreach (var type in types)
        ReferenceManager.resolvedTypes.Add(type);
    }

    /// <exception cref="InvalidOperationException">Current formatter process type 
    /// differs from the <paramref name="expectedProcessType"/>.</exception>
    public void EnsureFormatterProcessTypeIs(FormatterProcessType expectedProcessType) 
    {
      if (ProcessType != expectedProcessType)
        throw new InvalidOperationException(string.Format(
          Strings.ExInvalidFormatterProcessType,
          ProcessType));
    }


    // Constructors

    /// <summary>
    /// Protected constructor for overriding any of private fields.
    /// </summary>
    /// <remarks>
    /// Only <param name="formatter"/> and <param name="processType"/> should be defined.
    /// Any of the other parameters may be <see langword="null"/>.
    /// In this case default values are assigned to them.
    /// </remarks>
    protected SerializationContext(Formatter formatter, 
      FormatterProcessType processType,
      FixupQueue fixupQueue, Stack<IReference> traversalPath,
      Queue<SerializationData> deserializationQueue, 
      Queue<object> serializationQueue,
      Func<Type, string> typeToStringConverter, 
      Func<object, IReference> referenceFactory) 
    {
      ArgumentValidator.EnsureArgumentNotNull(formatter, "formatter");
      Formatter = formatter;
      ProcessType = processType;
      FixupQueue = fixupQueue ?? new FixupQueue();
      Path = traversalPath ?? new Stack<IReference>(8);
      if (processType==FormatterProcessType.Deserialization)
        DeserializationQueue = deserializationQueue ?? new Queue<SerializationData>(32);
      else
        SerializationQueue = serializationQueue ?? new Queue<object>();
      this.typeToStringConverter = typeToStringConverter ?? (type => type.FullName);
      ReferenceManager = new ReferenceManager(this);
      AddResolvedType(typeof (RecordDescriptor), typeof (IReference));
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="formatter">The formatter.</param>
    /// <param name="formatterProcessType">Process type of current (de)serialization.</param>
    protected SerializationContext(Formatter formatter, FormatterProcessType formatterProcessType)
      : this(formatter, formatterProcessType, null, null, null, null, null, null)
    {
    }
  }
}