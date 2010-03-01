// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.28

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Tuples;

namespace Xtensive.Core.Tests.Tuples
{
  internal class TestTupleActionHandler: ITupleActionHandler<Pair<int, int>>
  {
    public IList<Type> CalledForTypes = new List<Type>();

    public bool Execute<TFieldType>(ref Pair<int, int> actionData, int fieldIndex)
    {
      Assert.IsTrue(actionData.First<=fieldIndex);
      Assert.IsTrue(actionData.Second>=fieldIndex);
      CalledForTypes.Add(typeof(TFieldType));
      return false;
    }
  }

  internal class TestTupleFunctionHandler: ITupleFunctionHandler<TestTupleFunctionHandler.TestFunctionData, Type[]>
  {
    public struct TestFunctionData: ITupleFunctionData<Type[]>
    {
      public TupleDescriptor Descriptor;
      public IList<Type> CalledForTypes;

      public Type[] Result {
        get { return CalledForTypes.ToArray(); }
      }
    }

    public bool Execute<TFieldType>(ref TestFunctionData data, int fieldIndex)
    {
      Assert.IsTrue(data.Descriptor[fieldIndex]==typeof(TFieldType));
      data.CalledForTypes.Add(typeof(TFieldType));
      return false;
    }

    public TestFunctionData CreateData(TupleDescriptor descriptor)
    {
      var data = new TestFunctionData();
      data.Descriptor = descriptor;
      data.CalledForTypes = new List<Type>();
      return data;
    }

    public Type[] Execute(TupleDescriptor descriptor, Direction direction)
    {
      TestFunctionData data = CreateData(descriptor);
      return descriptor.Execute(this, ref data, direction);
    }
  }
}