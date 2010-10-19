// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.03.18

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Helpers
{
  /// <summary>
  /// A binary serializable wrapper using .NET <see cref="BinaryFormatter"/>
  /// to serialize and deserialize the <see cref="Value"/>.
  /// </summary>
  /// <typeparam name="T">The type of the <see cref="Value"/>.</typeparam>
  [Serializable]
  public class BinarySerializable<T>
  {
    [NonSerialized]
    private T value;
    private byte[] data;

    /// <summary>
    /// Gets or sets the value to wrap (serialize).
    /// </summary>
    public T Value {
      get { return value; }
      set { this.value = value; }
    }

    [OnSerializing]
    private void OnSerializing(StreamingContext context)
    {
      using (var stream = new MemoryStream()) {
        new BinaryFormatter().Serialize(stream, value);
        data = stream.ToArray();
      }
    }

    [OnSerialized]
    private void OnSerialized(StreamingContext context)
    {
      data = null;
    }

    [OnDeserialized]
    private void OnDeserializing(StreamingContext context)
    {
      using (var stream = new MemoryStream(data))
        value = (T) new BinaryFormatter().Deserialize(stream);
      data = null;
    }
    

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public BinarySerializable()
    {
      value = default(T);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    public BinarySerializable(T value)
    {
      this.value = value;
    }
  }
}