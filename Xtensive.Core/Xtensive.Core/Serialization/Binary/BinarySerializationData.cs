// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.26

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;
using Xtensive.Core.Serialization.Implementation;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// A <see cref="SerializationData"/> specific to binary serialization.
  /// </summary>
  [Serializable]
  public class BinarySerializationData : SerializationData
  {
    private const string ErasedSlotName = "~~Erased~~";
    private List<Triplet<string, long, bool>> slotList = new List<Triplet<string, long, bool>>();
    private Dictionary<string, int> slotMap  = new Dictionary<string, int>();
    private int readCount = 0;
    private bool isCompletelyRead;

    #region Properties: Context, Stream, Count, ReadCount

    /// <summary>
    /// Gets the <see cref="SerializationContext"/> this instance belongs to.
    /// </summary>
    public BinarySerializationContext Context { get; private set; }

    /// <summary>
    /// Gets the <see cref="ValueSerializerProvider"/> used by this instance.
    /// </summary>
    public ValueSerializerProvider ValueSerializerProvider { get; private set; }

    /// <summary>
    /// Gets the underlying stream for this instance.
    /// </summary>
    public Stream Stream { get; private set; }

    /// <inheritdoc/>
    public override int Count {
      get {
        EnsureIsCompletelyRead();
        return slotList.Count;
      }
    }

    /// <inheritdoc/>
    public override int ReadCount {
      get {
        return readCount;
      }
    }

    #endregion

    /// <inheritdoc/>
    /// <exception cref="SerializationException">Value with specified <paramref name="name"/> already exists.</exception>
    public override void AddValue<T>(string name, T value)
    {
      if (string.IsNullOrEmpty(name))
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      if (slotMap.ContainsKey("name"))
        throw new SerializationException(string.Format(
          Strings.ExValueWithNameXAlreadyExists, 
          name));
      var context = Context;
      context.EnsureProcessTypeIs(SerializerProcessType.Serialization);
      var valueSerializer = ValueSerializerProvider.FastGetSerializer<T>();
      if (valueSerializer==null)
        Serializer.EnsureValueSerializerIsFound<T>(valueSerializer);

      long originalPosition = Stream.Position;
      long originalLength = Stream.Length;
      try {
        // Writing name
        context.TokenStringSerializer.Serialize(Stream, Token.GetOrCreate(name));
        long dataLengthOffset = Stream.Position;
        // Skipping the space for the data length
        context.LongSerializer.Serialize(Stream, 0);
        long dataOffset = Stream.Position;
        // Writting data
        valueSerializer.Serialize(Stream, value);
        var dataEnd = Stream.Position;
        long dataLength = dataEnd - dataOffset;
        // Writting data length
        Stream.Position = dataLengthOffset;
        context.LongSerializer.Serialize(Stream, dataLength);
        Stream.Position = dataEnd;

        // Updating everything
        slotMap.Add(name, slotList.Count);
        slotList.Add(new Triplet<string, long, bool>(name, dataOffset, true));
        readCount++;
      }
      catch {
        Stream.Position = originalPosition;
        Stream.SetLength(originalLength);
        throw;
      }
    }

    /// <inheritdoc/>
    /// <exception cref="SerializationException">Value with specified <paramref name="name"/> is not found.</exception>
    public override T GetValue<T>(string name)
    {
      var valueSerializer = ValueSerializerProvider.FastGetSerializer<T>();
      Serializer.EnsureValueSerializerIsFound<T>(valueSerializer);

      int slotIndex;
      if (!slotMap.TryGetValue(name, out slotIndex)) {
        slotIndex = -1;
        while (!isCompletelyRead) {
          string currentName = ReadNextSlot();
          if (name==currentName) {
            slotIndex = slotList.Count - 1;
            break;
          }
        }
        if (slotIndex<0)
          throw new SerializationException(string.Format(
            Strings.ExValueWithNameXIsNotFound, 
            name));
      }

      var slotInfo = slotList[slotIndex];
      long originalPosition = Stream.Position;
      try {
        // Reading value
        Stream.Position = slotInfo.Second;
        var value = valueSerializer.Deserialize(Stream);

        // Updating ReadCount, if necessary
        if (!slotInfo.Third) {
          slotList[slotIndex] = new Triplet<string, long, bool>(slotInfo.First, slotInfo.Second, true);
          readCount++;
        }

        return value;
      }
      finally {
        Stream.Position = originalPosition;
      }
    }

    /// <inheritdoc/>
    /// <exception cref="SerializationException">Value with specified <paramref name="name"/> is not found.</exception>
    public override void SkipValue(string name)
    {
      int slotIndex;
      if (!slotMap.TryGetValue(name, out slotIndex)) {
        slotIndex = -1;
        while (!isCompletelyRead) {
          string currentName = ReadNextSlot();
          if (name==currentName) {
            slotIndex = slotList.Count - 1;
            break;
          }
        }
        if (slotIndex<0)
          throw new SerializationException(string.Format(
            Strings.ExValueWithNameXIsNotFound, 
            name));
      }

      var slotInfo = slotList[slotIndex];
      if (!slotInfo.Third) {
        slotList[slotIndex] = new Triplet<string, long, bool>(slotInfo.First, slotInfo.Second, true);
        readCount++;
      }
    }

    /// <inheritdoc/>
    public override bool HasValue(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      EnsureIsCompletelyRead();
      return slotMap.ContainsKey(name);
    }

    /// <inheritdoc/>
    /// <exception cref="SerializationException">Value with specified <paramref name="name"/> is not found.</exception>
    public override void RemoveValue(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      int slotIndex;
      if (!slotMap.TryGetValue(name, out slotIndex))
        throw new SerializationException(string.Format(
          Strings.ExValueWithNameXIsNotFound,
          name));
      var slotInfo = slotList[slotIndex];
      var context = Context;
      context.EnsureProcessTypeIs(SerializerProcessType.Serialization);

      long originalPosition = Stream.Position;
      try {
        long dataOffset = slotInfo.Second;
        long lengthLength = (context.LongSerializer.Implementation as ValueSerializerBase<long>).OutputLength;
        Stream.Position = dataOffset - lengthLength;
        // Reading old data length
        long dataLength = context.LongSerializer.Deserialize(Stream);
        // Writing back bitwise compliment of the data length
        context.LongSerializer.Serialize(Stream, ~dataLength);

        // Updating everything
        slotMap.Remove(name);
        slotList[slotIndex] = new Triplet<string, long, bool>(ErasedSlotName, dataOffset, true);
      }
      finally {
        Stream.Position = originalPosition;
      }
    }

    /// <inheritdoc/>
    public override IEnumerator<string> GetEnumerator()
    {
      int slotIndex = 0;
      while (true) {
        while (slotIndex>=slotList.Count) {
          if (ReadNextSlot()==null)
            break;
        }
        if (slotIndex>=slotList.Count)
          break;
        yield return slotList[slotIndex++].First;
      }
    }

    #region Protected methods

    /// <summary>
    /// Ensures the data is fully read (recalculates <see cref="Count"/>).
    /// </summary>
    protected void EnsureIsCompletelyRead()
    {
      while (!isCompletelyRead)
        ReadNextSlot();
    }

    /// <summary>
    /// Reads the next serialized slot.
    /// </summary>
    /// <returns>The name of the serialized value.
    /// <see langword="null" />, if there are no more slots.</returns>
    protected string ReadNextSlot()
    {
      if (isCompletelyRead)
        return null;
      Stream stream = Stream;
      if (stream.Position==stream.Length) {
        isCompletelyRead = true;
        return null;
      }
      var context = Context;
      long finalPosition = stream.Position;
      try {
        // Reading name
        string name = context.TokenStringSerializer.Deserialize(stream).Value;
        // Reading data length
        long dataLength = context.LongSerializer.Deserialize(stream);
        bool isErased = false;
        if (dataLength<0) {
          // Slot is erased
          isErased = true;
          name = ErasedSlotName;
          dataLength = ~dataLength;
        }
        long dataOffset = stream.Position;
        // Ensures finally we'll move to the beginning of the next slot
        finalPosition = dataOffset + dataLength; 

        // Updating everything
        if (!isErased) {
          slotMap.Add(name, slotList.Count);
          slotList.Add(new Triplet<string, long, bool>(name, dataOffset, false));
        }
        return name;
      }
      finally {
        stream.Position = finalPosition;
        if (finalPosition==stream.Length)
          isCompletelyRead = true;
      }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> property value.</param>
    public BinarySerializationData(Stream stream)
    {
      Context = (BinarySerializationContext) SerializationContext.Current;
      ValueSerializerProvider = (ValueSerializerProvider) Context.ValueSerializerProvider;
      Stream = stream;
    }

    /// <inheritdoc/>
    public BinarySerializationData(IReference reference, object source, object origin)
      : base(reference, source, origin)
    {
      Context = (BinarySerializationContext) SerializationContext.Current;
      ValueSerializerProvider = (ValueSerializerProvider) Context.ValueSerializerProvider;
      Stream = new MemoryStream();
      isCompletelyRead = true;
    }
  }
}