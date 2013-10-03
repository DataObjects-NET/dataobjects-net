// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2013.08.19

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Collections;

namespace Xtensive.Tests.Collections
{
  [TestFixture]
  public class ChainedBufferTest
  {
    [Test]
    public void EmptyTest()
    {
      var buffer = new ChainedBuffer<int>();
      Assert.That(buffer.Count, Is.EqualTo(0));
    }

    [Test]
    public void AddTest()
    {
      int count = 40;
      int index = 0;
      var buffer1 = new ChainedBuffer<int>();
      var buffer2 = new ChainedBuffer<string>();

      for (int i = 0; i < count; i++) {
        buffer1.Add(i);
        buffer2.Add(null);
      }
        
      Assert.That(buffer1.Count, Is.EqualTo(count));
      Assert.That(buffer2.Count, Is.EqualTo(count));

      foreach (var b in buffer1) {
        Assert.That(buffer1.Contains(index));
        ++index;
      }

      Assert.That(buffer2.Contains(null)); 
    }


    [Test]
    public void ClearTest()
    {
      int count = 40;
      var buffer1 = new ChainedBuffer<string>();
      for (int i = 0; i < count; i++)
        buffer1.Add(i.ToString());

      buffer1.Clear();

      Assert.That(buffer1.Count, Is.EqualTo(0));
    }

    [Test]
    public void ContainsTest()
    {
      int count = 40;
      int index = 0;

      var buffer1 = new ChainedBuffer<int>();
      var buffer2 = new ChainedBuffer<string>();

      for (int i = 0; i < count; i++) {
        buffer1.Add(i);
        buffer2.Add(null);
      }

      foreach (var buf in buffer1) {
        Assert.That(buffer1.Contains(index));
        ++index;
      }

      Assert.That(buffer2.Contains(null));
    }

    [Test]
    public void CopyToTest()
    {
      int count = 40;
      int arrayIndex = 10;
      int[] intArray = new int[count+arrayIndex];
      var buffer1 = new ChainedBuffer<int>();
      bool flag = true;

      for (int i = 0; i < count; i++)
        buffer1.Add(i);
      
      buffer1.CopyTo(intArray, arrayIndex);

      for (int i = 0; i < count; i++)
        if (intArray[i + arrayIndex] != i)
          flag = false;
      
      Assert.That(buffer1.Count + arrayIndex, Is.EqualTo(intArray.Length));
      Assert.That(flag);
    }

    [Test]
    public void AddRangeTest()
    {
      int count = 40;
      int arrayCount = 10;
      int index = 0;
      int[] intArray = new int[arrayCount];
      var buffer1 = new ChainedBuffer<int>();

      for (int i = 0; i < arrayCount; i++)
        intArray[i] = i + count;
      
      for (int i = 0; i < count; i++)
        buffer1.Add(i);

      buffer1.AddRange(intArray);

      foreach (var buf in buffer1) {
        Assert.That(buffer1.Contains(index));
        ++index;
      }
    }

    [Test]
    public void GetEnumeratorTest()
    {
      int count = 40;
      int index = 0;
      var buffer1 = new ChainedBuffer<int>();

      for (int i = 0; i < count; i++)
        buffer1.Add(i);

      Assert.AreEqual(count, buffer1.Count);
      
      foreach (var buf in buffer1) {
        Assert.That(buf, Is.EqualTo(index));
        ++index;
      }
    }

    [Test]
    public void ConstructorIntArrayTest()
    {
      int index = 0;
      int arrayCount = 40;
      int[] intArray = new int[arrayCount];
      
      for (int i = 0; i < arrayCount; i++)
        intArray[i] = i;

      var buffer1 = new ChainedBuffer<int>(intArray);

      Assert.AreEqual(arrayCount, buffer1.Count);

      foreach (var buf in buffer1) {
        Assert.That(buf, Is.EqualTo(index));
        ++index;
      }
    }

    [Test]
    public void ConstructorIntArrayAndMaxNodeSizeTest()
    {
      int index = 0;
      int arrayCount = 40;
      int maxNodeSize = 20;
      int[] intArray = new int[arrayCount];

      for (int i = 0; i < arrayCount; i++)
        intArray[i] = i;

      var buffer1 = new ChainedBuffer<int>(intArray, maxNodeSize);

      Assert.AreEqual(arrayCount, buffer1.Count);

      foreach (var buf in buffer1) {
        Assert.That(buf, Is.EqualTo(index));
        ++index;
      }
    }

    [Test]
    public void ConstructorMaxNodeSizeTest()
    {
      int count = 40;
      int index = 0;
      int maxNodeSize = 10;
      var buffer1 = new ChainedBuffer<int>(maxNodeSize);

      for (int i = 0; i < count; i++)
        buffer1.Add(i);

      Assert.AreEqual(count, buffer1.Count);

      foreach (var buf in buffer1) {
        Assert.That(buf, Is.EqualTo(index));
        ++index;
      }
    }
  }
}