// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Core.Collections;

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// A helper struct allowing to serialize <see cref="Tuple"/> objects.
  /// </summary>
  [Serializable]
  public struct SerializableTuple : IEquatable<SerializableTuple>
  {
    [Serializable] private sealed class NotAvailable { }
    private readonly static NotAvailable notAvailableInstance = new NotAvailable();
    [NonSerialized]
    private Tuple tuple;

    private object value;

    /// <summary>
    /// Gets the wrapped <see cref="Tuple"/>.
    /// </summary>
    public Tuple Value {
      [DebuggerStepThrough]
      get
      {
        if (tuple==null && value!=null)
          Deserialize();
        return tuple;
      }
    }

    #region Equals, GetHashCode, ==, !=

    /// <inheritdoc/>
    public bool Equals(SerializableTuple other)
    {
      return AdvancedComparerStruct<Tuple>.System.Equals(Value, other.Value);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (obj.GetType()!=typeof (SerializableTuple))
        return false;
      return Equals((SerializableTuple) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return Value!=null ? Value.GetHashCode() : 0;
    }

    /// <see cref="ClassDocTemplate.OperatorEq" copy="true" />
    public static bool operator ==(SerializableTuple left, SerializableTuple right)
    {
      return left.Equals(right);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq" copy="true" />
    public static bool operator !=(SerializableTuple left, SerializableTuple right)
    {
      return !left.Equals(right);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return Value!=null ? Value.ToString() : string.Empty;
    }


    // Constructors
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="value">The <see cref="Value"/> property value.</param>
    public SerializableTuple(Tuple value)
    {
      tuple = value;
      this.value = null;
    }


    // Serialization

    [OnSerializing]
    internal void OnSerializing(StreamingContext context)
    {
      var tuple = Value;
      if (tuple==null)
        return;

      var count = tuple.Count;
      var p = new Pair<Type[], object[]>(tuple.Descriptor.ToArray(), new object[count]);
      for (int i = 0; i < count; i++) {
        TupleFieldState state;
        var fieldValue = tuple.GetValue(i, out state);
        p.Second[i] = state.IsAvailable()
          ? state.IsNull()
            ? null
            : fieldValue
          : notAvailableInstance;
      }
      value = p;
    }

    [OnSerialized]
    internal void OnSerialized(StreamingContext context)
    {
      value = null;
    }

    [OnDeserialized]
    internal void OnDeserialized(StreamingContext context)
    {
      Deserialize();
    }

    private void Deserialize()
    {
      if (value==null || value.GetType()!=typeof(Pair<Type[], object[]>))
        return;
      var p = (Pair<Type[], object[]>) value;
      var tuple = Tuple.Create(p.First);
      var count = tuple.Count;
      for (int i = 0; i < count; i++) {
        if (p.Second[i]==null) {
          tuple.SetValue(i, null);
          continue;
        }
        if (p.Second[i].GetType()==typeof(NotAvailable))
          continue;
        tuple.SetValue(i, p.Second.GetValue(i));
      }
      this.tuple = tuple;
      value = null;
    }
  }
}