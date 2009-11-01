// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Xtensive.Core;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Collection of parameters relevant to a <see cref="Command"/>.
  /// </summary>
  /// <typeparam name="T"><para>A type of the <see cref="ParameterCollection{T}"/> items.</para>
  /// <para>The specified <typeparamref name="T"/> must be <see cref="Parameter"/> class descendant.</para></typeparam>
  [Serializable]
  public class ParameterCollection<T>
    : DbParameterCollection, IList<T>
    where T : Parameter, new()
  {
    protected List<T> items = new List<T>();

    /// <inheritdoc/>
    public override int Add(object value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ArgumentValidator.EnsureArgumentIs<T>(value, "value");
      items.Add((T)value);
      return items.Count-1;
    }

    /// <inheritdoc/>
    public void Add(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      ArgumentValidator.EnsureArgumentIs<T>(item, "item");
      items.Add(item);
    }

    /// <summary>
    /// Adds a <see cref="Parameter"/> to the <see cref="ParameterCollection{T}"/> 
    /// given the parameter name and the data type.
    /// </summary>
    /// <returns>
    /// The <see cref="Parameter"/> object that added to the collection.
    /// </returns>
    /// <param name="parameterName">A name of the parameter.</param>
    /// <param name="dbType">One of the <see cref="DbType"/> values.</param>
    public T Add(string parameterName, DbType dbType)
    {
      T parameter = new T();
      parameter.ParameterName = parameterName;
      parameter.DbType = dbType;
      Add(parameter);
      return parameter;
    }

    /// <summary>
    /// Adds a <see cref="Parameter"/> to the <see cref="ParameterCollection{T}"/> 
    /// given the parameter name and the value.
    /// </summary>
    /// <returns>
    /// The <see cref="Parameter"/> object that added to the collection.
    /// </returns>
    /// <param name="parameterName">A name of the parameter.</param>
    /// <param name="value">A value of the parameter.</param>
    public T Add(string parameterName, object value)
    {
      T parameter = new T();
      parameter.ParameterName = parameterName;
      parameter.Value = value;
      Add(parameter);
      return parameter;
    }

    /// <summary>
    /// Adds a <see cref="Parameter"/> to the <see cref="ParameterCollection{T}"/> 
    /// given the parameter name, the data type and the column length.
    /// </summary>
    /// <returns>
    /// The <see cref="Parameter"/> object that added to the collection.
    /// </returns>
    /// <param name="parameterName">A name of the parameter.</param>
    /// <param name="dbType">One of the <see cref="DbType"/> values.</param>
    /// <param name="size">A column length.</param>
    public T Add(string parameterName, DbType dbType, int size)
    {
      T parameter = new T();
      parameter.ParameterName = parameterName;
      parameter.DbType = dbType;
      parameter.Size = size;
      Add(parameter);
      return parameter;
    }

    /// <summary>
    /// Adds a <see cref="Parameter"/> to the <see cref="ParameterCollection{T}"/> 
    /// given the parameter name, the data type, the column length and the source
    /// column name.
    /// </summary>
    /// <returns>
    /// The <see cref="Parameter"/> object that added to the collection.
    /// </returns>
    /// <param name="parameterName">A name of the parameter.</param>
    /// <param name="dbType">One of the <see cref="DbType"/> values.</param>
    /// <param name="size">A column length.</param>
    /// <param name="sourceColumn">A source column name.</param>
    public T Add(string parameterName, DbType dbType, int size, string sourceColumn)
    {
      T parameter = new T();
      parameter.ParameterName = parameterName;
      parameter.DbType = dbType;
      parameter.Size = size;
      parameter.SourceColumn = sourceColumn;
      Add(parameter);
      return parameter;
    }

    /// <inheritdoc/>
    public override void AddRange(Array values)
    {
      ArgumentValidator.EnsureArgumentNotNull(values, "values");
      foreach (object value in values) {
        ArgumentValidator.EnsureArgumentIs<T>(value, "value");
        items.Add(value as T);
      }
    }

    /// <summary>
    /// Adds an array of items with the specified values 
    /// to the <see cref="ParameterCollection{T}"/>.
    /// </summary>
    /// <param name="values">An array of values of type <typeparamref name="T"/>
    /// to be added to the <see cref="ParameterCollection{T}"/>.</param>
    public void AddRange(T[] values)
    {
      ArgumentValidator.EnsureArgumentNotNull(values, "values");
      foreach (T value in values) {
        items.Add(value);
      }
    }

    /// <inheritdoc/>
    public override bool Contains(object value)
    {
      return IndexOf(value)!=-1;
    }

    /// <inheritdoc/>
    public bool Contains(T item)
    {
      return IndexOf(item)!=-1;
    }

    /// <inheritdoc/>
    public override bool Contains(string parameterName)
    {
      return IndexOf(parameterName)!=-1;
    }

    /// <inheritdoc/>
    public override void CopyTo(Array array, int index)
    {
      ((IList)items).CopyTo(array, index);
    }

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
      ((IList)items).CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      items.Clear();
    }

    /// <inheritdoc/>
    public override IEnumerator GetEnumerator()
    {
      return items.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return items.GetEnumerator();
    }

    /// <inheritdoc/>
    public override int IndexOf(object value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ArgumentValidator.EnsureArgumentIs<T>(value, "value");
      return items.IndexOf(value as T);
    }

    /// <inheritdoc/>
    public int IndexOf(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return items.IndexOf(item);
    }

    /// <inheritdoc/>
    public override int IndexOf(string parameterName)
    {
      if (items.Count==0)
        return -1;
      int index = 0;
      foreach (T p in items) {
        if (p.ParameterName==parameterName)
          return index;
        index++;
      }
      return -1;
    }

    /// <inheritdoc/>
    public override void Insert(int index, object value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ArgumentValidator.EnsureArgumentIs<T>(value, "value");
      items.Insert(index, value as T);
    }

    /// <inheritdoc/>
    public void Insert(int index, T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      items.Insert(index, item);
    }

    /// <inheritdoc/>
    public override void Remove(object value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ArgumentValidator.EnsureArgumentIs<T>(value, "value");
      items.Remove(value as T);
    }

    /// <inheritdoc/>
    public bool Remove(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return items.Remove(item);
    }

    /// <inheritdoc/>
    public override void RemoveAt(int index)
    {
      items.RemoveAt(index);
    }

    /// <inheritdoc/>
    public override void RemoveAt(string parameterName)
    {
      items.RemoveAt(IndexOf(parameterName));
    }

    /// <inheritdoc/>
    public override int Count
    {
      get { return items.Count; }
    }

    /// <inheritdoc/>
    public override bool IsFixedSize
    {
      get { return ((IList)items).IsFixedSize; }
    }

    /// <inheritdoc/>
    public override bool IsReadOnly
    {
      get { return ((IList)items).IsReadOnly; }
    }

    /// <inheritdoc/>
    public override bool IsSynchronized
    {
      get { return ((IList)items).IsSynchronized; }
    }

    /// <inheritdoc/>
    public override object SyncRoot
    {
      get { return ((IList)items).SyncRoot; }
    }

    /// <inheritdoc/>
    public new T this[int index]
    {
      get { return (T)GetParameter(index); }
      set { SetParameter(index, value); }
    }

    /// <summary>
    /// Gets and sets the <see cref="DbParameter"/> with the specified name.
    /// </summary>
    /// <value></value>
    public new T this[string parameterName]
    {
      get { return (T)GetParameter(parameterName); }
      set { SetParameter(parameterName, value); }
    }

    /// <inheritdoc/>
    protected override DbParameter GetParameter(int index)
    {
      return items[index];
    }

    /// <inheritdoc/>
    protected override DbParameter GetParameter(string parameterName)
    {
      return items[IndexOf(parameterName)];
    }

    /// <inheritdoc/>
    protected override void SetParameter(int index, DbParameter value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ArgumentValidator.EnsureArgumentIs<T>(value, "value");
      items[index] = value as T;
    }

    /// <inheritdoc/>
    protected override void SetParameter(string parameterName, DbParameter value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ArgumentValidator.EnsureArgumentIs<T>(value, "value");
      items[IndexOf(parameterName)] = value as T;
    }
  }
}