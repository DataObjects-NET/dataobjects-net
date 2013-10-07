// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.19

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Collections
{
  /// <summary>
  /// Singly-linked immutable list.
  /// </summary>
  /// <typeparam name="T">The type of elements.</typeparam>
  [Serializable]
  public sealed class SinglyLinkedList<T> : IEnumerable<T>
  {
    /// <summary>
    /// Gets the empty <see cref="SinglyLinkedList{T}"/>.
    /// </summary>
    public static SinglyLinkedList<T> Empty { get; private set; }

    private readonly T head;

    /// <summary>
    /// Gets the value of the head node.
    /// </summary>
    public T Head
    {
      get
      {
        if (Count==0)
          throw new NotSupportedException(Strings.ExInstanceIsEmpty);
        return head;
      }
    }

    /// <summary>
    /// Gets the tail of the current insttance.
    /// </summary>
    public SinglyLinkedList<T> Tail { get; private set; }

    /// <inheritdoc/>
    public long Count { get; private set; }


    /// <summary>
    /// Appends the head value and returns new instance of <see cref="SinglyLinkedList{T}"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>New instance of <see cref="SinglyLinkedList{T}"/> with provided <paramref name="value"/> as a head node.</returns>
    public SinglyLinkedList<T> Add(T value)
    {
      return new SinglyLinkedList<T>(value, this);
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
      if (Count==0)
        yield break;
      var list = this;
      while (list!=Empty) {
        yield return list.head;
        list = list.Tail;
      }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    // Constructors

    private SinglyLinkedList()
    {
      head = default(T);
      Tail = null;
      Count = 0;
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="value">The value.</param>
    public SinglyLinkedList(T value)
    {
      head = value;
      Tail = Empty;
      Count = 1;
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="source">The source elements.</param>
    public SinglyLinkedList(IEnumerable<T> source)
    {
      using (var enumerator = source.GetEnumerator()) {
        if (!enumerator.MoveNext()) 
          return;
        head = enumerator.Current;
        Tail = enumerator.MoveNext()
          ? new SinglyLinkedList<T>(enumerator)
          : Empty;
        Count = 1 + Tail.Count;
      }
    }

    private SinglyLinkedList(IEnumerator<T> source)
    {
      head = source.Current;
      Tail = source.MoveNext() 
        ? new SinglyLinkedList<T>(source) 
        : Empty;
      Count = 1 + Tail.Count;
    }

    private SinglyLinkedList(T head, SinglyLinkedList<T> tail)
    {
      this.head = head;
      Tail = tail;
      Count = Tail.Count + 1;
    }

    /// <summary>
    /// Initializes this type.
    /// </summary>
    static SinglyLinkedList()
    {
      Empty = new SinglyLinkedList<T>();
    }
  }
}