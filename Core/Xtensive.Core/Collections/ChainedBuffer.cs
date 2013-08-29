// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2013.08.19

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Resources;
using System.Linq;

namespace Xtensive.Collections
{
  /// <summary>
  /// ChainedBuffer(<see cref="ICollection{T}"/>)
  /// </summary>
  /// <typeparam name="T">The type of collection items.</typeparam>

  public class ChainedBuffer<T> : ICollection<T>
  {
    private sealed class Node<T>
    {
      public readonly T[] Items;
      public Node<T> Next;

      // Constructors

      public Node(int size)
      {
        Items = new T[size];
      }
    }

    private static readonly IEqualityComparer<T> Comparer = EqualityComparer<T>.Default; 

    private int totalSize;
    private int maxNodeSize;
    private int tailNodeSize;   
    private Node<T> headNode;
    private Node<T> tailNode;

    /// <inheritdoc/>
    public int Count { get { return totalSize; } }

    /// <inheritdoc/>
    public bool IsReadOnly { get  { return false; } }

    #region GetEnumerator<...> methods

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
      if (headNode==null)
        yield break;

      var currentNode = headNode;

      while (currentNode!=tailNode) {
        foreach (var item in currentNode.Items)
          yield return item;
        currentNode = currentNode.Next;
      }

      for (int i = 0; i < tailNodeSize; ++i)
        yield return currentNode.Items[i];
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    #region Modification methods: Add, Clear, Contains, Remove, AddRange

    /// <inheritdoc/>
    public void Add(T item)
    {
      if (headNode==null) {
        headNode = tailNode = new Node<T>(maxNodeSize);
        tailNodeSize = 0;
      }
      else if (tailNodeSize==maxNodeSize) {
        var newTailNode = new Node<T>(maxNodeSize);
        tailNodeSize = 0;
        tailNode.Next = newTailNode;
        tailNode = newTailNode;
      }
      tailNode.Items[tailNodeSize++] = item;
      ++totalSize;
    }

    /// <inheritdoc/>
    public void Clear()
    {
      headNode = null;
      tailNode = null;
      totalSize = 0;
      maxNodeSize = 0;
      tailNodeSize = 0;
    }

    /// <inheritdoc/>
    public bool Contains(T item)
    {
      return this.Any(itemNode => Comparer.Equals(itemNode, item));
    }

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
      ArgumentValidator.EnsureArgumentNotNull(array, "array");
      ArgumentValidator.EnsureArgumentIsInRange(arrayIndex, 0, int.MaxValue, "arrayIndex");

      if (array.Length < totalSize + arrayIndex)
        throw new ArgumentException();

      foreach (var item in this)
        array[arrayIndex++] = item;
    }

    /// <inheritdoc/>
    public bool Remove(T item)
    {
      throw new NotSupportedException(Strings.ExChainedBufferRemoveMethodIsNotSupported);
    }

    /// <inheritdoc/>
    public void AddRange(IEnumerable<T> items)
    {
      ArgumentValidator.EnsureArgumentNotNull(items, "items");
      foreach (var item in items)
        Add(item);
    }

    #endregion

    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    public ChainedBuffer()
    {
      const int largeObjectHeapItemSize = 85000;
      maxNodeSize = largeObjectHeapItemSize / IntPtr.Size;
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="maxNodeSize">The maximal node size.</param>
    public ChainedBuffer(int maxNodeSize)
    {
      ArgumentValidator.EnsureArgumentIsInRange(maxNodeSize, 1, int.MaxValue, "maxNodeSize");
      this.maxNodeSize = maxNodeSize;
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="items">The items to add to this collection.</param>   
    public ChainedBuffer(IEnumerable<T> items)
      : this()
    {
      AddRange(items);
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="items">The items to add to this collection.</param>      
    /// <param name="maxNodeSize">The maximal node size.</param>
    public ChainedBuffer(IEnumerable<T> items, int maxNodeSize)
      : this(maxNodeSize)
    {
      AddRange(items);
    }
  }
}