// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.26

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Serializes and deserializes an object, or an entire graph of connected objects.
  /// </summary>
  public abstract class Formatter :
    ConfigurableBase<FormatterConfiguration>,
    IFormatter
  {
    private static readonly ObjectSerializerProvider serializerProvider =
      ObjectSerializer<RecordDescriptor>.Default.Provider as ObjectSerializerProvider;
    private readonly StreamingContext streamingContext = new StreamingContext(StreamingContextStates.Other);
    private object rootObject;
    private bool isNotRoot;

    #region Properties: Binder, AssemblyStyle, TypeFormat

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown on attempt to set this property.</exception>
    public SerializationBinder Binder {
      get { return Configuration.Binder; }
      set { throw new NotSupportedException(); }
    }

    /// <summary>
    /// Gets the behavior of the formatter describing how to find and load assemblies.
    /// </summary>
    public FormatterAssemblyStyle AssemblyStyle {
      get { return Configuration.AssemblyStyle; }
    }

    /// <summary>
    /// Gets the format in which type descriptions are stored in the serialized stream.
    /// </summary>
    public FormatterTypeStyle TypeFormat {
      get { return Configuration.TypeFormat; }
    }

    #endregion

    #region Explicit IFormatter properties: SurrogateSelector, Context (unused here)

    /// <summary>
    /// Always returns <see langword="null" />.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown on attempt to set this property.</exception>
    ISurrogateSelector IFormatter.SurrogateSelector {
      get { return null; }
      set { throw new NotSupportedException(); }
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown on attempt to set this property.</exception>
    StreamingContext IFormatter.Context {
      get { return streamingContext; }
      set { throw new NotSupportedException(); }
    }

    #endregion

    /// <summary>
    /// Creates new <see cref="SerializationContext"/> for the current process.
    /// </summary>
    /// <param name="formatterProcessType"><see cref="FormatterProcessType"/> of the current process.</param>
    /// <returns>New <see cref="SerializationContext"/>.</returns>
    protected abstract SerializationContext GetContext(FormatterProcessType formatterProcessType);

    /// <summary>
    /// Registers the type descriptor to use it during the serialization or deserialization.
    /// </summary>
    /// <param name="descriptor">The descriptor to register.</param>
    public abstract void RegisterDescriptor(RecordDescriptor descriptor);

    #region Deserialization

    private static void ExecuteFixups() 
    {
      SerializationContext context = SerializationContext.Current;
      foreach (var action in context.FixupQueue)
        action.Execute();
    }

    /// <summary>
    /// Creates <see cref="RecordReader"/> to read serialized data from <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">Stream to read from.</param>
    /// <returns>Reader from the stream.</returns>
    protected abstract RecordReader CreateReader(Stream stream);

    private Type ResolveDescriptor(RecordDescriptor descriptor) 
    {
      return Binder.BindToType(descriptor.AssemblyName, descriptor.TypeName);
    }

    /// <summary>
    /// Deserializes the object from the <paramref name="record"/> and 
    /// return <see cref="IReference"/> to it.
    /// </summary>
    /// <param name="record"><see cref="Record"/> to deserialize from.</param>
    /// <returns><see cref="IReference"/> to deserialized object.</returns>
    public IReference Deserialize(Record record) 
    {
      if (record.Descriptor == null)
        throw new FormatException(Strings.ExInvalidRecordType);

      Type type = ResolveDescriptor(record.Descriptor);
      IObjectSerializer serializer = serializerProvider.GetSerializer(type);
      SerializationContext context = SerializationContext.Current;
      IReference reference = null;
      var isRoot = !isNotRoot;
      isNotRoot = true;
      if (serializer.IsReferable) {
        reference = record.Reference;
        context.Path.Push(reference);
      }
      var data = new SerializationData(record);
      object obj = serializer.CreateObject();
      obj = serializer.SetObjectData(obj, data);
      if (obj != null) {
        if (serializer.IsReferable)
          context.ReferenceManager.Define(reference, obj);
        if (isRoot)
          rootObject = obj;
      }
      else if (isRoot)
        isNotRoot = false;
      if (serializer.IsReferable)
        context.Path.Pop();
      else
        reference = obj is IReference ? (IReference) obj : new Reference(obj);
      return reference;
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Ensures all the descriptors were successfully serialized to the <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer"><see cref="RecordWriter"/> to which <see cref="RecordDescriptor"/>s should be serialized.</param>
    protected abstract void EnsureDescriptorAreWritten(RecordWriter writer);

    /// <summary>
    /// Create <see cref="RecordWriter"/> to write serialized data to <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">Stream to write to.</param>
    /// <returns>Writer to the stream.</returns>
    protected abstract RecordWriter CreateWriter(Stream stream);

    /// <summary>
    /// Create new <see cref="RecordDescriptor"/> inheritor.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to create <see cref="RecordDescriptor"/> for.</param>
    /// <returns><see cref="RecordDescriptor"/> for the <paramref name="type"/>.</returns>
    protected internal virtual RecordDescriptor CreateDescriptor(Type type) 
    {
      return new RecordDescriptor(type);
    }

    /// <summary>
    /// Serialize <paramref name="graph"/> to the new <see cref="Record"/>.
    /// </summary>
    /// <param name="graph">Object to serialize.</param>
    /// <param name="recordCreator">Delegate for creating new empty <see cref="Record"/>.</param>
    public Record Serialize(object graph, Func<string, IReference, Record> recordCreator) 
    {
      ArgumentValidator.EnsureArgumentNotNull(recordCreator, "recordCreator");
      ArgumentValidator.EnsureArgumentNotNull(graph, "graph");

      var serializer = serializerProvider.GetSerializerByInstance(graph);
      var context = SerializationContext.Current;
      var type = graph.GetType();

      IReference graphRef = null;
      if (serializer.IsReferable)
        graphRef = context.ReferenceManager.GetReference(graph);
      if (graphRef != null && graphRef.IsQueueable)
        context.Path.Push(graphRef);

      Record newRecord = recordCreator(context.GetTypeName(type), graphRef);
      var data = new SerializationData(newRecord);
      serializer.GetObjectData(graph, data);

      if (graphRef != null)
        context.Path.Pop();
      return newRecord;
    }

    #endregion

    #region IFormatter Members

    /// <inheritdoc/>
    public void Serialize(Stream stream, object graph) 
    {
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");
      ArgumentValidator.EnsureArgumentNotNull(graph, "graph");

      var context = GetContext(FormatterProcessType.Serialization);
      using (context.Activate())
      using (var writer = CreateWriter(stream)) {
        Func<string, IReference, Record> createRecord = writer.CreateRecord;
        writer.Append(Serialize(graph, createRecord));
        while (context.SerializationQueue.Count > 0)
          writer.Append(Serialize(context.SerializationQueue.Dequeue(), createRecord));
        EnsureDescriptorAreWritten(writer);
      }
    }

    /// <inheritdoc/>
    public object Deserialize(Stream stream) 
    {
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");

      SerializationContext context = GetContext(FormatterProcessType.Deserialization);
      using (context.Activate()) {
        isNotRoot = false;
        rootObject = null;
        var reader = CreateReader(stream);
        foreach (var record in reader)
          Deserialize(record);
        while (context.DeserializationQueue.Count > 0)
          Deserialize(context.DeserializationQueue.Dequeue().Record);

        ExecuteFixups();
        return rootObject;
      }
    }

    #endregion

    /// <inheritdoc/>
    protected override void OnConfigured() 
    {
      base.OnConfigured();
      foreach (Pair<string, string> location in Configuration.SerializerLocations) {
        var an = new AssemblyName(location.First);
        serializerProvider.AddHighPriorityLocation(Assembly.Load(an), location.Second);
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    protected Formatter(FormatterConfiguration configuration)
      : base(configuration) {}
  }
}