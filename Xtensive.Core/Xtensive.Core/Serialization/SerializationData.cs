// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.19

using System;
using System.Collections.Generic;
using Xtensive.Core.Serialization;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Stores all the data needed to serialize or deserialize an object.
  /// </summary>
  public class SerializationData
  {
    private Record record;

    internal Record Record
    {
      get { return record; }
    }

    /// <summary>
    /// Determines whether the <see cref="SerializationData"/> store contains an element associated with 
    /// the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="SerializationData"/> store contains 
    /// an element associated with the specified <paramref name="name"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      return Record.Contains(name);
    }

    /// <summary>
    /// Adds the value.
    /// </summary>
    /// <typeparam name="T">The type of the value to add.</typeparam>
    /// <param name="name">The name to associate with the value, so it can be deserialized later.</param>
    /// <param name="value">The value to serialize.</param>
    public void AddValue<T>(string name, T value)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      ValidateFormatterState(FormatterProcessType.Serialization);

      Record.AddValue(name, value);
    }

    /// <summary>
    /// Retrieves a value from the <see cref="SerializationData"/> store.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="name">The name associated with the value to retrieve.</param>
    /// <returns>The value associated with the <paramref name="name"/>.</returns>
    public T GetValue<T>(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      ValidateFormatterState(FormatterProcessType.Deserialization);

      return Record.GetValue<T>(name);
    }

    /// <summary>
    /// Retrieves a referenced object from the <see cref="SerializationData"/> store.
    /// </summary>
    /// <param name="name">The name associated with the object to retrieve.</param>
    /// <remarks>Referenced object may be not deserialized yet.</remarks>
    public IReference GetObject(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      ValidateFormatterState(FormatterProcessType.Deserialization);
      
      Record innerRecord = Record.GetRecord(name);
      if (innerRecord == null)
        return Reference.Empty;

      return SerializationScope.CurrentContext.Formatter.Deserialize(innerRecord);
    }

    /// <summary>
    /// Sets a fixup action to an object associated with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name associated with the object to retrieve.</param>
    /// <param name="action">The action to be executed when the referenced object will be deserialized.</param>
    public void SetFixupAction(string name, Action<IReference> action)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      ValidateFormatterState(FormatterProcessType.Deserialization);

      if (action == null)
        return;

      IReference objRef = GetObject(name);
      if (objRef.IsEmpty)
        return;
      object obj = SerializationScope.CurrentContext.ReferenceManager.Resolve(objRef);
      if (obj != null)
        action(objRef);
      else 
        SerializationScope.CurrentContext.FixupQueue.Enqueue(objRef, action);
    }

    /// <summary>
    /// Adds the set of items.
    /// </summary>
    /// <typeparam name="T">The type of the item to add.</typeparam>
    /// <param name="name">The name associated with the items to store.</param>
    /// <param name="items">The items to serialize.</param>
    public void AddValues<T>(string name, IEnumerable<T> items)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      ValidateFormatterState(FormatterProcessType.Serialization);

      throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves an set of values from the <see cref="SerializationData"/> store.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve.</typeparam>
    /// <param name="name">The name associated with the set of values to retrieve.</param>
    public IEnumerable<T> GetValues<T>(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      ValidateFormatterState(FormatterProcessType.Serialization);

      throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves an set of <see cref="IReference"/> instances from the <see cref="SerializationData"/> store.
    /// </summary>
    /// <param name="name">The name associated with the set of object to retrieve.</param>
    public IEnumerable<IReference> GetObjects(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      ValidateFormatterState(FormatterProcessType.Deserialization);

      SerializationContext context = SerializationScope.CurrentContext;
      foreach (Record innerRecord in Record.GetRecords(name))
        yield return context.Formatter.Deserialize(innerRecord);
    }

    /// <summary>
    /// Sets a fixup action to a set of object associated with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name associated with the set of object to retrieve.</param>
    /// <param name="action">The action to be executed with each element when the set of objects will be deserialized.</param>
    public void SetFixupActions(string name, Action<IReference> action)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      ValidateFormatterState(FormatterProcessType.Deserialization);

      if (action == null)
        return;

      SerializationContext context = SerializationScope.CurrentContext;
      foreach (IReference objRef in GetObjects(name))
        if (!objRef.IsEmpty)
          context.FixupQueue.Enqueue(objRef, action);
    }

    private static void ValidateFormatterState(FormatterProcessType processType)
    {
      if (SerializationScope.CurrentContext.ProcessType != processType)
        throw new InvalidOperationException("Operation is invalid within current formatter process.");
    }


    // Constructors

    internal SerializationData(Record record)
    {
      this.record = record;
    }
  }
}