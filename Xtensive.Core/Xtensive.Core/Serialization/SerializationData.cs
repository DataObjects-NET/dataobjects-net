// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.19

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;
using Xtensive.Core.Serialization;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Stores all the data needed to serialize or deserialize an object.
  /// </summary>
  public sealed class SerializationData
  {
    private readonly Record record;

    internal Record Record {
      get { return record; }
    }

    #region AddValue(s) methods

    /// <summary>
    /// Adds the value to the <see cref="SerializationData"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to add.</typeparam>
    /// <param name="name">The name to associate with the value, so it can be deserialized later.</param>
    /// <param name="value">The value to serialize.</param>
    public void AddValue<T>(string name, T value) 
    {
      AddValue(name, value, null);
    }

    /// <summary>
    /// Adds the value to the <see cref="SerializationData"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to add.</typeparam>
    /// <param name="name">The name to associate with the value, so it can be deserialized later.</param>
    /// <param name="value">The value to serialize.</param>
    /// <param name="preferNesting"><see langword="true"/> if its necessary to prefer nesting records, otherwize <see langword="false"/>,
    ///  or <see langword="null"/> for using setting from <see cref="FormatterConfiguration.PreferNesting"/>.</param>
    public void AddValue<T>(string name, T value, bool? preferNesting) 
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      SerializationContext.Current.EnsureFormatterProcessTypeIs(FormatterProcessType.Serialization);

      if (IsReferenceType<T>()) {
        var context = SerializationContext.Current;
        var pn = preferNesting.HasValue ? preferNesting.Value : context.Formatter.Configuration.PreferNesting;
        bool isNew;
        var referece = context.ReferenceManager.GetReference(value, out isNew);
        if ((!referece.IsQueueable || pn) && isNew)
          Record.AddObject(name, value);
        else {
          if (isNew)
            context.SerializationQueue.Enqueue(value);
          Record.AddObject(name, referece);
        }
      }
      else
        Record.AddValue(name, value);
    }

    /// <summary>
    /// Adds the set of items.
    /// </summary>
    /// <typeparam name="T">The type of the item to add.</typeparam>
    /// <param name="name">The name associated with the items to store.</param>
    /// <param name="items">The items to serialize.</param>
    public void AddValues<T>(string name, IEnumerable<T> items) {
      AddValues(name, items, null);
    }

    /// <summary>
    /// Adds the set of items.
    /// </summary>
    /// <typeparam name="T">The type of the item to add.</typeparam>
    /// <param name="name">The name associated with the items to store.</param>
    /// <param name="items">The items to serialize.</param>
    /// <param name="preferNesting"><see langword="true"/> if its necessary to prefer nesting records, otherwize <see langword="false"/>,
    ///  or <see langword="null"/> for using setting from <see cref="FormatterConfiguration.PreferNesting"/>.</param>
    public void AddValues<T>(string name, IEnumerable<T> items, bool? preferNesting) 
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      SerializationContext.Current.EnsureFormatterProcessTypeIs(FormatterProcessType.Serialization);

      if (IsReferenceType<T>()) {
        var context = SerializationContext.Current;
        var pn = preferNesting.HasValue ? preferNesting.Value : context.Formatter.Configuration.PreferNesting;
        foreach (var value in items) {
          bool isNew;
          var reference = context.ReferenceManager.GetReference(value, out isNew);
          if ((!reference.IsQueueable || pn) && isNew)
            Record.AddObjects(name, value);
          else {
            if (isNew)
              context.SerializationQueue.Enqueue(value);
            Record.AddObjects(name, reference);
          }
        }
      }
      else
        foreach (var value in items)
          Record.AddValues(name, value);
    }

    #endregion

    #region GetValue(s), GetObject(s) methods

    /// <summary>
    /// Retrieves a value from the <see cref="SerializationData"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="name">The name associated with the value to retrieve.</param>
    /// <returns>The value associated with the <paramref name="name"/>.</returns>
    public T GetValue<T>(string name) 
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      SerializationContext.Current.EnsureFormatterProcessTypeIs(FormatterProcessType.Deserialization);
      EnsureIsValueType<T>();

      return Record.GetValue<T>(name);
    }

    /// <summary>
    /// Retrieves an set of values from the <see cref="SerializationData"/> store.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve.</typeparam>
    /// <param name="name">The name associated with the set of values to retrieve.</param>
    public IEnumerable<T> GetValues<T>(string name) 
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      SerializationContext.Current.EnsureFormatterProcessTypeIs(FormatterProcessType.Deserialization);
      EnsureIsValueType<T>();

      while (true) {
        T value;
        if (!Record.GetValues(name, out value))
          yield break;
        yield return value;
      }
    }

    /// <summary>
    /// Retrieves an object from the <see cref="SerializationData"/>.
    /// </summary>
    /// <param name="name">The name associated with the object to retrieve.</param>
    /// <remarks>Referenced object may be not deserialized yet.</remarks>
    public IReference GetObject(string name) 
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      SerializationContext.Current.EnsureFormatterProcessTypeIs(FormatterProcessType.Deserialization);

      Record innerRecord = Record.GetRecord(name);
      if (innerRecord == null)
        return Reference.Null;
      return SerializationContext.Current.Formatter.Deserialize(innerRecord);
    }

    /// <summary>
    /// Retrieves an set of <see cref="IReference"/> instances from the <see cref="SerializationData"/> store.
    /// </summary>
    /// <param name="name">The name associated with the set of object to retrieve.</param>
    public IEnumerable<IReference> GetObjects(string name) 
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      SerializationContext context = SerializationContext.Current;
      context.EnsureFormatterProcessTypeIs(FormatterProcessType.Deserialization);
      while (true) {
        Record innerRecord;
        if (!Record.GetRecords(name, out innerRecord))
          yield break;
        yield return context.Formatter.Deserialize(innerRecord);
      }
    }

    #endregion

    #region AddFixup(s) methods

    /// <summary>
    /// Sets a fixup action to an object associated with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name associated with the object to retrieve.</param>
    /// <param name="action">The action to be executed when the referenced object will be deserialized.</param>
    /// <param name="target">Object to execute <paramref name="action"/> on.</param>
    /// <typeparam name="T">Type of the <paramref name="target"/>.</typeparam>
    public void AddFixup<T>(string name, T target, Action<IReference, T> action) 
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      ArgumentValidator.EnsureArgumentNotNull(action, "action");
      SerializationContext.Current.EnsureFormatterProcessTypeIs(FormatterProcessType.Deserialization);

      IReference reference = GetObject(name);
      if (reference.IsNull())
        return;
      object obj = reference.Resolve();
      if (obj!=null)
        action(reference, target);
      else
        SerializationContext.Current.FixupQueue.Enqueue(reference, target, action);
    }

    /// <summary>
    /// Sets a fixup action to a set of object associated with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name associated with the set of object to retrieve.</param>
    /// <param name="action">The action to be executed with each element when the set of objects will be deserialized.</param>
    /// <param name="target">Object to execute <paramref name="action"/> on.</param>
    /// <typeparam name="T">Type of the <paramref name="target"/>.</typeparam>
    public void AddFixups<T>(string name, T target, Action<IReference, T> action) 
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      ArgumentValidator.EnsureArgumentNotNull(action, "action");
      SerializationContext.Current.EnsureFormatterProcessTypeIs(FormatterProcessType.Deserialization);

      SerializationContext context = SerializationContext.Current;
      foreach (IReference objRef in GetObjects(name))
        if (!objRef.IsNull())
          context.FixupQueue.Enqueue(objRef, target, action);
    }

    #endregion

    #region Private \ internal methods

    /// <summary>
    /// Indicates if type <typeparamref name="T"/> has associated <see cref="IObjectSerializer"/>;
    /// otherwise is must have associated <see cref="IValueSerializer{TStream}"/>.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <returns><see langword="True"/> if <typeparamref name="T"/> has associated 
    /// <see cref="IObjectSerializer"/>;
    /// otherwise is must have associated <see cref="IValueSerializer{TStream}"/>.</returns>
    private static bool IsReferenceType<T>() 
    {
      var dictionary = SerializationContext.Current.IsObjectOrValueType;
      var type = typeof (T);
      if (dictionary.ContainsKey(type))
        return dictionary[type];
      var result = ObjectSerializerProvider.Default.GetSerializer(type) != null;
      dictionary.Add(type, result);
      return result;
    }

    /// <exception cref="InvalidOperationException"><typeparamref name="T"/> is associated with 
    /// <see cref="IValueSerializer{TStream}"/>.</exception>
    private static void EnsureIsReferenceType<T>() 
    {
      if (!IsReferenceType<T>())
        throw new InvalidOperationException(string.Format(
          Strings.ExInvalidSerializerType,
          typeof(IValueSerializer<>).GetShortName(),
          typeof(IObjectSerializer).GetShortName()));
    }

    /// <exception cref="InvalidOperationException"><typeparamref name="T"/> is associated with 
    /// <see cref="IObjectSerializer"/>.</exception>
    private static void EnsureIsValueType<T>() 
    {
      if (IsReferenceType<T>())
        throw new InvalidOperationException(string.Format(
          Strings.ExInvalidSerializerType,
          typeof(IObjectSerializer).GetShortName(),
          typeof(IValueSerializer<>).GetShortName()));
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>.
    /// </summary>
    /// <param name="record"><see cref="Record"/> to base on.</param>
    public SerializationData(Record record) 
    {
      this.record = record;
    }
  }
}