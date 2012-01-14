// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using NUnit.Framework;
using Xtensive.Comparison;
using System.Linq;
using Xtensive.Diagnostics;
using Xtensive.Serialization;
using Xtensive.Serialization.Binary;
using Xtensive.Testing;

namespace Xtensive.Tests.Serialization
{
  #region Test classes

  [Serializable]
  public class Node : ISerializable,
    IEnumerable<Node>,
    IEquatable<Node>
  {
    public int Id { get; set; }
    public Node Left { get; set; }
    public Node Right { get; set; }
    public Node Parent { get; set; }

    public int Count {
      get {
        return 1 +
          (Left!=null ? Left.Count : 0) +
          (Right!=null ? Right.Count : 0);
      }
    }

    public bool Equals(Node obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (Id==obj.Id)
        if ((Left==null)==(obj.Left==null))
          if ((Right==null)==(obj.Right==null)) {
            if (Left!=null && !Left.Equals(obj.Left))
              return false;
            if (Right!=null && !Right.Equals(obj.Right))
              return false;
            return true;
          }
      return false;
    }

    public override bool Equals(object obj)
    {
      return Equals(obj as Node);
    }

    public override string ToString()
    {
      return string.Format("{0}({1},{2})", 
        Id, 
        Left!=null ? Left.ToString() : string.Empty,
        Right!=null ? Right.ToString() : string.Empty);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<Node> GetEnumerator()
    {
      yield return this;
      if (Left!=null)
        foreach (var node in Left)
          yield return node;
      if (Right!=null)
        foreach (var node in Right)
          yield return node;
    }


    // Constructors

    public Node(int id, Node parent, Node left, Node right)
    {
      Id = id;
      Left = left;
      Right = right;
      Parent = parent;
    }

    public Node(int id, Node parent, Random random, int count)
    {
      Id = id++;
      Parent = parent;
      if (--count<=0)
        return;
      int lr = 1 + random.Next(3);
      bool createLeft  = (lr & 1)!=0;
      bool createRight = (lr & 2)!=0;
      if (createLeft) {
        int leftCount = createRight ? random.Next(count) : count;
        if (leftCount>0) {
          Left = new Node(id, this, random, leftCount);
          count -= leftCount;
          id += leftCount;
        }
      }
      if (createRight && count>0)
        Right = new Node(id, this, random, count);
    }

    #if NET40
    [SecurityCritical]
    #else
    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
    #endif
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Id", Id);
      info.AddValue("Parent", Parent);
      info.AddValue("Left",   Left);
      info.AddValue("Right",  Right);
    }

    protected Node(SerializationInfo info, StreamingContext context)
    {
      Id = info.GetInt32("Id");
      Parent = (Node) info.GetValue("Parent", typeof (object));
      Left   = (Node) info.GetValue("Left",   typeof (object));
      Right  = (Node) info.GetValue("Right",  typeof (object));
    }
  }

  public class NodeSerializer : ObjectSerializerBase<Node>
  {
    public override Node CreateObject(Type type)
    {
      return new Node(0, null, null, null);
    }

    public override void GetObjectData(Node source, Node origin, SerializationData data)
    {
      base.GetObjectData(source, origin, data);
      data.AddValue("Id", source.Id, Int32Serializer);
      data.AddObject<Node>("Parent", o => o.Parent);
      data.AddObject<Node>("Left", o => o.Left);
      data.AddObject<Node>("Right", o => o.Right);
    }

    public override Node SetObjectData(Node source, SerializationData data)
    {
      source.Id = data.GetValue("Id", Int32Serializer);
      data.UpdateSource(source);
      data.AddFixup<Node>("Parent", (n,r) => n.Parent = r.Resolve<Node>());
      data.AddFixup<Node>("Left",   (n,r) => n.Left   = r.Resolve<Node>());
      data.AddFixup<Node>("Right",  (n,r) => n.Right  = r.Resolve<Node>());
      return source;
    }

    public NodeSerializer(IObjectSerializerProvider provider)
      : base(provider)
    {
    }
  }

  #endregion


  [TestFixture]
  public class BinarySerializerTest
  {
    private static bool warmup = false;
    private static bool profile = false;
    private static int  baseSize = 4000;
    private static int  iterationCount = 1000;

    [Test, Explicit]
    public void CombinedTest()
    {
      Random r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);

      TestCloning("1", 1, iterationCount);
      TestCloning("int [3]", new [] {1,2,3}, iterationCount / 10);

      var ints = InstanceGenerationUtils<int>.GetInstances(r, 0).Take(baseSize).ToArray();
      TestCloning(string.Format("int [{0}]", ints.Length), ints, iterationCount / 10);

      var guids = InstanceGenerationUtils<Guid>.GetInstances(r, 0).Take(baseSize).ToArray();
      TestCloning(string.Format("Guid [{0}]", guids.Length), guids, iterationCount / 10);

      var objects = InstanceGenerationUtils<int>.GetInstances(r, 0).Take(baseSize).Cast<object>().ToArray();
      TestCloning(string.Format("Object [{0}]", objects.Length), objects, iterationCount / 100);

      var nodes = new Node(1, null, r, baseSize);
      TestCloning(string.Format("Node [{0}]", nodes.Count), nodes, iterationCount / 100);
    }

    [Test]
    [Explicit]
    [Category("Profile")]
    public void ProfileTest()
    {
      profile = true;
      Random r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);

//      var nodes = new Node(1, null, r, 1000);
//      TestCloning(string.Format("Node [{0}]", nodes.Count), nodes, iterationCount / 100);

//      var ints = InstanceGenerationUtils<int>.GetInstances(r, 0).Take(baseSize).ToArray();
//      TestCloning(string.Format("int [{0}]", ints.Length), ints, 1);

      var objects = InstanceGenerationUtils<int>.GetInstances(r, 0).Take(baseSize).Cast<object>().ToArray();
      TestCloning(string.Format("Object [{0}]", objects.Length), objects, 1);
    }

    private static void TestCloning<T>(string title, T source)
    {
      TestCloning(title, source, 1);
    }

    private static void TestCloning<T>(string title, T source, int iterationCount)
    {
      using (Log.InfoRegion("Cloning: {0}", title)) {
        if (!profile) {
          warmup = true;
          InnerTestCloning(source, iterationCount);
          warmup = false;
        }
        InnerTestCloning(source, iterationCount);
      }
    }

    private static void InnerTestCloning<T>(T source, int iterationCount)
    {
      var c1 = profile ? source : CloneByNativeSerialization(source, iterationCount);
      var c2 = CloneByOurSerialization(source, iterationCount);
      if (!profile)
        Assert.AreEqual(source, c1);
    }

    private static T CloneByOurSerialization<T>(T source, int iterationCount)
    {
      var ms = new MemoryStream();
      var s = BinarySerializer.Instance;
      TestHelper.CollectGarbage();
      T result = default(T);
      long size = 0;
      using (warmup ? null : new Measurement("Xtensive cloning", iterationCount)) {
        for (int i = 0; i<iterationCount; i++) {
          ms.Position = 0;
          s.Serialize(ms, source);
          if (i==0)
            size = ms.Length;
          ms.Position = 0;
          result = (T) s.Deserialize(ms);
        }
      }
      if (!warmup)
        Log.Info("  Data size: {0}", size);
      return result;
    }

    private static T CloneByNativeSerialization<T>(T source, int iterationCount)
    {
      var ms = new MemoryStream();
      var s = LegacyBinarySerializer.Instance;
      TestHelper.CollectGarbage();
      T result = default(T);
      long size = 0;
      using (warmup ? null : new Measurement("Native cloning  ", iterationCount)) {
        for (int i = 0; i<iterationCount; i++) {
          ms.Position = 0;
          s.Serialize(ms, source);
          if (i==0)
            size = ms.Length;
          ms.Position = 0;
          result = (T) s.Deserialize(ms);
        }
      }
      if (!warmup)
        Log.Info("  Data size: {0}", size);
      return result;
    }
  }
}