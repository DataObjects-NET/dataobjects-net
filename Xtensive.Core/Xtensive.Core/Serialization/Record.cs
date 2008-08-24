// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.19

using System;
using System.Collections.Generic;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Stores all the data needed to serialize or deserialize an object.
  /// </summary>
  public abstract class Record
  {
    /// <summary>
    /// Gets the descriptor of this record.
    /// </summary>
    public RecordDescriptor Descriptor { get; protected set; }

    /// <summary>
    /// Gets the <see cref="IReference"/> associated with this record.
    /// </summary>
    public IReference Reference { get; protected set; }

    /// <summary>
    /// Adds a value to the record.
    /// </summary>
    /// <typeparam name="T">The type of the value to add.</typeparam>
    /// <param name="name">The name to associate with the value, so it can be deserialized later.</param>
    /// <param name="value">The value to serialize.</param>
    public abstract void AddValue<T>(string name, T value);

    /// <summary>
    /// Adds an object to the record.
    /// </summary>
    /// <typeparam name="T">The type of the value to add.</typeparam>
    /// <param name="name">The name to associate with the object, so it can be deserialized later.</param>
    /// <param name="value">The object to serialize.</param>
    public abstract void AddObject<T>(string name, T value);

    /// <summary>
    /// Adds a set of items to the record.
    /// </summary>
    /// <typeparam name="T">The type of the item to add.</typeparam>
    /// <param name="name">The name to associate with the objects.</param>
    /// <param name="item">The item from the set to serialize.</param>
    public abstract void AddValues<T>(string name, T item);

    /// <summary>
    /// Adds a set of objects to the record.
    /// </summary>
    /// <typeparam name="T">The type of the item to add.</typeparam>
    /// <param name="name">The name to associate with the objects.</param>
    /// <param name="item">The object from the set to serialize.</param>
    public abstract void AddObjects<T>(string name, T item);

    /// <summary>
    /// Retrieves a value from the <see cref="Record"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="name">The name associated with the value to retrieve.</param>
    /// <returns>The value associated with the <paramref name="name"/>.</returns>
    public abstract T GetValue<T>(string name);

    /// <summary>
    /// Retrieves a set of values stored in the <see cref="Record"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="name">The name associated with the set of values to retrieve from.</param>
    /// <param name="value">The value from the set associated with the <paramref name="name"/>.</param>
    /// <returns><see langword="true"/> if succeed, <see langword="else"/> otherwise.</returns>
    public abstract bool GetValues<T>(string name, out T value);

    /// <summary>
    /// Retrieves the first nested <see cref="Record"/> instance with the specified name 
    /// from the record.
    /// </summary>
    /// <param name="name">The name associated with the record to retrieve.</param>
    /// <returns>The <see cref="Record"/> instance associated with the <paramref name="name"/>.</returns>
    public abstract Record GetRecord(string name);

    /// <summary>
    /// Gets the group of <see cref="Record"/>s nested in the inner record 
    /// with name <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name associated with the group of nested records.</param>
    /// <param name="record">The <see cref="Record"/> from the set of nested <see cref="Record"/>s associated with the <paramref name="name"/>
    /// or <see langword="null"/> if none.</param>
    /// <returns><see langword="true"/> if succeed, <see langword="else"/> otherwise.</returns>
    public abstract bool GetRecords(string name, out Record record);
  }
}