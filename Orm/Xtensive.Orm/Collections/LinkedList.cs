// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.19

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Collections
{
  /// <summary>
  /// Single-linked immutable list.
  /// </summary>
  /// <typeparam name="T">The type of elements.</typeparam>
  [Serializable]
  public sealed class LinkedList<T> : IEnumerable<T>
  {
    /// <summary>
    /// Gets the empty <see cref="LinkedList{T}"/>.
    /// </summary>
    public static LinkedList<T> Empty { get; private set; }
    private T head;

    /// <summary>
    /// Gets the value of the head node.
    /// </summary>
    public T Head
    {
      get {
        if (Count == 0)
          throw new NotSupportedException("Current instance is empty.");
        return head;
      }
    }

    /// <summary>
    /// Gets the tail of the current insttance.
    /// </summary>
    public LinkedList<T> Tail { get; private set; }

    /// <inheritdoc/>
    public long Count { get; private set; }


    /// <summary>
    /// Appends the head value and returns new instance of <see cref="LinkedList{T}"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>New instance of <see cref="LinkedList{T}"/> with provided <paramref name="value"/> as a head node.</returns>
    public LinkedList<T> AppendHead(T value)
    {
      return new LinkedList<T>(value, this);
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
      if (Count == 0)
        yield break;
      var list = this;
      while (list != Empty) {
        yield return list.head;
        list = list.Tail;
      }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    private LinkedList()
    {
      head = default(T);
      Tail = null;
      Count = 0;
    }

    /// <summary>
    ///  <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="value">The value.</param>
    public LinkedList(T value)
    {
      head = value;
      Tail = Empty;
      Count = 1;
    }

    /// <summary>
    ///  <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The source elements.</param>
    public LinkedList(IEnumerable<T> source)
    {
      using (var enumerator = source.GetEnumerator()) {
        if (!enumerator.MoveNext()) 
          return;
        head = enumerator.Current;
        Tail = enumerator.MoveNext()
          ? new LinkedList<T>(enumerator)
          : Empty;
        Count = 1 + Tail.Count;
      }
    }

    private LinkedList(IEnumerator<T> source)
    {
      head = source.Current;
      Tail = source.MoveNext() 
        ? new LinkedList<T>(source) 
        : Empty;
      Count = 1 + Tail.Count;
    }

    private LinkedList(T head, LinkedList<T> tail)
    {
      this.head = head;
      Tail = tail;
      Count = Tail.Count + 1;
    }

    /// <summary>
    ///  <see cref="ClassDocTemplate.TypeInitializer" copy="true"/>
    /// </summary>
    static LinkedList()
    {
      Empty = new LinkedList<T>();
    }
  }
}