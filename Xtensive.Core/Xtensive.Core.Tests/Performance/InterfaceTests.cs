// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Anton U. Rogozhin
// Created:    2007.06.29

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Xtensive.Core.Tests.Performance
{
  public class InterfaceTests
  {
    /// <summary>
    ///  Test an ICollection that should contain the given values, possibly in order.
    /// </summary>
    /// <param name="coll">ICollection to test. </param>
    /// <param name="valueArray">The values that should be in the collection.</param>
    /// <param name="mustBeInOrder">Must the values be in order?</param>
    public static void TestCollection<T>(ICollection coll, T[] valueArray, bool mustBeInOrder)
    {
      T[] values = (T[])valueArray.Clone(); // clone the array so we can destroy it.

      // Check ICollection.Count.
      Assert.AreEqual(values.Length, coll.Count);

      // Check ICollection.GetEnumerator().
      int i = 0, j;

      foreach (T s in coll) {
        if (mustBeInOrder) {
          Assert.AreEqual(values[i], s);
        }
        else {
          bool found = false;

          for (j = 0; j < values.Length; ++j) {
            if (Equals(values[j], s)) {
              found = true;
              values[j] = default(T);
              break;
            }
          }

          Assert.IsTrue(found);
        }

        ++i;
      }

      // Check IsSyncronized, SyncRoot.
      Assert.IsFalse(coll.IsSynchronized);
      Assert.IsNotNull(coll.SyncRoot);

      // Check CopyTo.
      values = (T[])valueArray.Clone(); // clone the array so we can destroy it.

      T[] newKeys = new T[coll.Count + 2];

      coll.CopyTo(newKeys, 1);
      for (i = 0, j = 1; i < coll.Count; ++i, ++j) {
        if (mustBeInOrder) {
          Assert.AreEqual(values[i], newKeys[j]);
        }
        else {
          bool found = false;

          for (int k = 0; k < values.Length; ++k) {
            if (Equals(values[k], newKeys[j])) {
              found = true;
              values[k] = default(T);
              break;
            }
          }

          Assert.IsTrue(found);
        }
      }

      // Shouldn't have disturbed the values around what was filled in.
      Assert.AreEqual(default(T), newKeys[0]);
      Assert.AreEqual(default(T), newKeys[coll.Count + 1]);

      // Check CopyTo exceptions.
      if (coll.Count > 0) {
        try {
          coll.CopyTo(null, 0);
          Assert.Fail("Copy to null should throw exception");
        }
        catch (Exception e) {
          Assert.IsTrue(e is ArgumentNullException);
        }
        try {
          coll.CopyTo(newKeys, 3);
          Assert.Fail("CopyTo should throw argument exception");
        }
        catch (Exception e) {
          Assert.IsTrue(e is ArgumentException);
        }
        try {
          coll.CopyTo(newKeys, -1);
          Assert.Fail("CopyTo should throw argument out of range exception");
        }
        catch (Exception e) {
          Assert.IsTrue(e is ArgumentOutOfRangeException);
        }
      }
    }

    /// /// <summary>
    ///  Test an ICollection&lt;string&gt; that should contain the given values, possibly in order. Checks only the following items:
    ///     GetEnumerator, CopyTo, Count, Contains
    /// </summary>
    /// <param name="coll">ICollection to test. </param>
    /// <param name="values">The elements that should be in the collection.</param>
    /// <param name="mustBeInOrder">Must the elements be in order?</param>
    /// <param name="equals">Predicate to test for equality; null for default.</param>
    public static void TestCollectionGeneric<T>(ICollection<T> coll, T[] values, bool mustBeInOrder,
                                                 Predicate<T, T> equals)
    {
      if (equals == null)
        equals = delegate(T x, T y) { return Equals(x, y); };

      bool[] used = new bool[values.Length];

      // Check ICollection.Count.
      Assert.AreEqual(values.Length, coll.Count);

      // Check ICollection.GetEnumerator().
      int i = 0, j;

      foreach (T s in coll) {
        if (mustBeInOrder) {
          Assert.IsTrue(equals(values[i], s));
        }
        else {
          bool found = false;

          for (j = 0; j < values.Length; ++j) {
            if (!used[j] && equals(values[j], s)) {
              found = true;
              used[j] = true;
              break;
            }
          }

          Assert.IsTrue(found);
        }

        ++i;
      }

      // Check Contains
      foreach (T s in values) {
        Assert.IsTrue(coll.Contains(s));
      }

      // Check CopyTo.
      used = new bool[values.Length];

      T[] newKeys = new T[coll.Count + 2];

      coll.CopyTo(newKeys, 1);
      for (i = 0, j = 1; i < coll.Count; ++i, ++j) {
        if (mustBeInOrder) {
          Assert.IsTrue(equals(values[i], newKeys[j]));
        }
        else {
          bool found = false;

          for (int k = 0; k < values.Length; ++k) {
            if (!used[k] && equals(values[k], newKeys[j])) {
              found = true;
              used[k] = true;
              break;
            }
          }

          Assert.IsTrue(found);
        }
      }

      // Shouldn't have distubed the values around what was filled in.
      Assert.IsTrue(equals(default(T), newKeys[0]));
      Assert.IsTrue(equals(default(T), newKeys[coll.Count + 1]));

      if (coll.Count != 0) {
        // Check CopyTo exceptions.
        try {
          coll.CopyTo(null, 0);
          Assert.Fail("Copy to null should throw exception");
        }
        catch (Exception e) {
          Assert.IsTrue(e is ArgumentNullException);
        }
        try {
          coll.CopyTo(newKeys, 3);
          Assert.Fail("CopyTo should throw argument exception");
        }
        catch (Exception e) {
          Assert.IsTrue(e is ArgumentException);
        }
        try {
          coll.CopyTo(newKeys, -1);
          Assert.Fail("CopyTo should throw argument out of range exception");
        }
        catch (Exception e) {
          Assert.IsTrue(e is ArgumentOutOfRangeException);
        }
      }
    }


    /// <summary>
    ///  Test a read-write ICollection&lt;string&gt; that should contain the given values, possibly in order. Destroys the collection in the process.
    /// </summary>
    /// <param name="coll">ICollection to test. </param>
    /// <param name="valueArray">The values that should be in the collection.</param>
    /// <param name="mustBeInOrder">Must the values be in order?</param>
    public static void TestReadWriteCollectionGeneric<T>(ICollection<T> coll, T[] valueArray, bool mustBeInOrder)
    {
      TestReadWriteCollectionGeneric<T>(coll, valueArray, mustBeInOrder, null);
    }

    public static void TestReadWriteCollectionGeneric<T>(ICollection<T> coll, T[] valueArray, bool mustBeInOrder,
                                                         Predicate<T, T> equals)
    {
      TestCollectionGeneric<T>(coll, valueArray, mustBeInOrder, equals);

      // Test read-only flag.
      Assert.IsFalse(coll.IsReadOnly);

      // Clear and Count.
      coll.Clear();
      Assert.AreEqual(0, coll.Count);

      // Add all the items back.
      foreach (T item in valueArray)
        coll.Add(item);
      Assert.AreEqual(valueArray.Length, coll.Count);
      TestCollectionGeneric<T>(coll, valueArray, mustBeInOrder, equals);

      // Remove all the items again.
      foreach (T item in valueArray)
        coll.Remove(item);
      Assert.AreEqual(0, coll.Count);
    }
  }
}