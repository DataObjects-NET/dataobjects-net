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

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Serializes and deserializes an object, or an entire graph of connected objects.
  /// </summary>
  public abstract class Formatter: ConfigurableBase<FormatterConfiguration>,
    IFormatter
  {
    private StreamingContext streamingContext = new StreamingContext(StreamingContextStates.Other);
    private static SerializerProvider serializerProvider = Serializer<RecordDescriptor>.Default.Provider as SerializerProvider;
    private object topObject = null;

    /// <inheritdoc/>
    protected override void OnConfigured()
    {
      base.OnConfigured();
      foreach (Pair<string, string> location in Configuration.SerializerLocations) {
        AssemblyName an = new AssemblyName(location.First);
        serializerProvider.AddHighPriorityLocation(Assembly.Load(an), location.Second);
      }
    }

    /// <summary>
    /// Gets or sets the behavior of the deserializer with regards to finding and loading assemblies.
    /// </summary>
    /// <value>One of the <see cref="FormatterAssemblyStyle"/> values that specifies the deserializer behavior.</value>
    public FormatterAssemblyStyle AssemblyStyle
    {
      get { return Configuration.AssemblyStyle; }
    }

    /// <summary>
    /// Gets or sets the format in which type descriptions are laid out in the serialized stream.
    /// </summary>
    /// <value>The format in which type descriptions are laid out in the serialized stream.</value>
    public FormatterTypeStyle TypeFormat
    {
      get { return Configuration.TypeFormat; }
    }

    protected abstract RecordReader CreateReader(Stream stream);

    protected abstract SerializationContext CreateContext();

    protected abstract void RegisterDescriptor(RecordDescriptor descriptor);

    protected abstract ISerializer DescriptorSerializer { get; }

    private static void ExecuteFixUpActions()
    {
      SerializationContext context = SerializationScope.CurrentContext;
      if (context.FixupQueue.Count == 0)
        return;
      foreach (FixupAction action in context.FixupQueue)
        action.Action(action.Reference);
    }

    private void UpdateTopObject(object obj)
    {
      if (topObject == null)
        topObject = obj;
    }

    private Type ResolveDescriptor(RecordDescriptor descriptor)
    {
      return Binder.BindToType(descriptor.AssemblyName, descriptor.FullTypeName);
    }

    private static IReference ResolveRecordReference(RecordRef recordRef)
    {
      SerializationContext context = SerializationScope.CurrentContext;
      switch (recordRef.Type) {
        case RecordRefType.Extern:
          return context.ReferenceManager.CreateReference(recordRef.Id);
        case RecordRefType.Parent:
          return context.TraversalPath[SerializationScope.CurrentContext.TraversalPath.Count - 2];
        case RecordRefType.Self:
          return context.TraversalPath.Peek();
        default:
          return context.ReferenceManager.CreateSurrogate();
      }
    }

    internal IReference Deserialize(Record record)
    {
      switch (record.Type) {
        case RecordType.Unknown:
          // TODO: Log this.
          break;
        case RecordType.Descriptor:
          DeserializeDescriptor(record);
          break;
        case RecordType.Object:
          return DeserializeObject(record);
        case RecordType.Reference:
          return DeserializeReference(record);
      }
      return Reference.Empty;
    }

    private static IReference DeserializeReference(Record record)
    {
      return ResolveRecordReference(record.Reference);
    }

    private void DeserializeDescriptor(Record record)
    {
      SerializationContext context = SerializationScope.CurrentContext;
      SerializationData data = new SerializationData(record);
      RecordDescriptor descriptor = (RecordDescriptor)DescriptorSerializer.CreateObject(data, context);
      DescriptorSerializer.SetObjectData(descriptor, data, context);
      RegisterDescriptor(descriptor);
    }

    private IReference DeserializeObject(Record record)
    {
      Type type = ResolveDescriptor(record.Descriptor);
      // TODO: Check type is not null
      ISerializer serializer = serializerProvider.GetObjectSerializer(type);
      // TODO: Check serializer is not null 

      SerializationContext context = SerializationScope.CurrentContext;
      IReference reference = ResolveRecordReference(record.Reference);
      context.TraversalPath.Push(reference);
      SerializationData data = new SerializationData(record);
      object obj = serializer.CreateObject(data, context);
      if (obj != null) {
        context.ReferenceManager.Update(reference, obj);
        UpdateTopObject(obj);
        serializer.SetObjectData(obj, data, context);
      }
      context.TraversalPath.Pop();
      return reference;
    }

    #region IFormatter Members

    /// <inheritdoc/>
    public void Serialize(Stream stream, object graph)
    {
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");
      ArgumentValidator.EnsureArgumentNotNull(graph, "graph");
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public object Deserialize(Stream stream)
    {
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");

      SerializationContext context = CreateContext();
      context.ProcessType = FormatterProcessType.Deserialization;

      using(context.Activate()) {
        RecordReader reader = CreateReader(stream);
        while (reader.Read())
          Deserialize(reader.GetRecord());
        while (context.DeserializationQueue.Count > 0)
          Deserialize(context.DeserializationQueue.Dequeue().Record);

        ExecuteFixUpActions();
        return topObject;
      }
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">always on set.</exception>
    public ISurrogateSelector SurrogateSelector
    {
      get { return null; }
      set { throw new NotSupportedException(); }
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">always on set.</exception>
    public SerializationBinder Binder
    {
      get { return Configuration.Binder; }
      set { throw new NotSupportedException(); }
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">always on set.</exception>
    public StreamingContext Context
    {
      get { return streamingContext; }
      set { throw new NotSupportedException(); }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    protected Formatter(FormatterConfiguration configuration)
      : base(configuration)
    {
    }
  }
}