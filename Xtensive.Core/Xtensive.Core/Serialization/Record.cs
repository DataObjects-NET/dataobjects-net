// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.19

using System.Collections.Generic;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Stores all the data needed to serialize or deserialize an object.
  /// </summary>
  public abstract class Record
  {
    private RecordType type;
    private RecordDescriptor descriptor;
    private RecordRef reference;

    public RecordType Type
    {
      get { return type; }
      protected set { type = value; }
    }

    public RecordDescriptor Descriptor
    {
      get { return descriptor; }
      protected set { descriptor = value; }
    }

    public RecordRef Reference
    {
      get { return reference; }
      protected set { reference = value; }
    }

    /// <summary>
    /// Determines whether the <see cref="Record"/> contains an element associated with 
    /// the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="Record"/> contains 
    /// an element associated with the specified <paramref name="name"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public abstract bool Contains(string name);

    /// <summary>
    /// Adds the value.
    /// </summary>
    /// <typeparam name="T">The type of the value to add.</typeparam>
    /// <param name="name">The name to associate with the value, so it can be deserialized later.</param>
    /// <param name="value">The value to serialize.</param>
    public abstract void AddValue<T>(string name, T value);

    /// <summary>
    /// Retrieves a value from the <see cref="Record"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="name">The name associated with the value to retrieve.</param>
    /// <returns>The value associated with the <paramref name="name"/>.</returns>
    public abstract T GetValue<T>(string name);

    /// <summary>
    /// Adds the record.
    /// </summary>
    /// <param name="name">The name to associate with the object, so it can be deserialized later.</param>
    /// <param name="record">The object to serialize.</param>
    public abstract void AddRecord(string name, Record record);

    /// <summary>
    /// Retrieves the first nested <see cref="Record"/> instance with the specified name from the <see cref="Record"/>.
    /// </summary>
    /// <param name="name">The name associated with the record to retrieve.</param>
    /// <returns>The <see cref="Record"/> instance associated with the <paramref name="name"/>.</returns>
    public abstract Record GetRecord(string name);

    /// <summary>
    /// Adds the set of items.
    /// </summary>
    /// <typeparam name="T">The type of the item to add.</typeparam>
    /// <param name="items">The items to serialize.</param>
    public abstract void AddRecords<T>(IEnumerable<T> items);

    /// <summary>
    /// Gets the nested records.
    /// </summary>
    /// <param name="name">The name associated with the group of nested records.</param>
    /// <returns>
    /// The <see cref="IEnumerable{T}"/> of nested <see cref="Record"/> instances.
    /// </returns>
    public abstract IEnumerable<Record> GetRecords(string name);
  }
}