// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.09

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Orm.Logging;
using Xtensive.Reflection;
using Xtensive.Orm.Tests;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.Core.Reflection
{
  internal struct ExecutionData
  {
    public int CallCount;
    public IList<Type> CalledForTypes;

    public static ExecutionData Create()
    {
      ExecutionData result = default(ExecutionData);
      result.CalledForTypes = new List<Type>();
      return result;
    }
  }

  [TestFixture]
  public class DelegateHelperCreateDelegateTest
  {
    public int passCount = 1000;

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceTest()
    {
      passCount *= 10000;
      try {
        RegularTest();
      }
      finally {
        passCount /= 10000;
      }
    }

    [Test]
    public void RegularTest()
    {
      TestTypes(new Type[] {});
      TestTypes(new Type[] {
        typeof(bool),
        });
      TestTypes(new Type[] {
        typeof(string),
        });
      TestTypes(new Type[] {
        typeof(int),
        typeof(int?),
        });
      TestTypes(new Type[] {
        typeof(long),
        typeof(Guid),
        typeof(string),
        typeof(int),
        typeof(int),
        });
    }

    private void TestTypes(Type[] types)
    {
      TupleDescriptor descriptor = TupleDescriptor.Create(types);
      TestLog.Info("Testing sequence {0}:", descriptor);
      using (IndentManager.IncreaseIndent()) {
        // Logic test
        LogicTest(types, this, GetType(), "SequenceAStep");
        LogicTest(types, null, GetType(), "SequenceAStaticStep");

        // Performance tests
        ExecutionData data = ExecutionData.Create();
        ExecutionSequenceHandler<ExecutionData>[] delegates;

        int count = passCount / 1000;
        delegates = DelegateHelper.CreateDelegates<ExecutionSequenceHandler<ExecutionData>>(
          this, GetType(), "SequenceBStep", types);
        TestHelper.CollectGarbage();
        using (new Measurement("Creating delegates", count))
          for (int i = 0; i<count; i++)
            delegates = DelegateHelper.CreateDelegates<ExecutionSequenceHandler<ExecutionData>>(
              this, GetType(), "SequenceBStep", types);

        count = passCount;
        DelegateHelper.ExecuteDelegates(delegates, ref data, Direction.Positive);
        data = ExecutionData.Create();
        TestHelper.CollectGarbage();
        using (new Measurement("Executing delegates", count))
          for (int i = 0; i<count; i++)
            DelegateHelper.ExecuteDelegates(delegates, ref data, Direction.Positive);
        Assert.AreEqual(count*types.Length, data.CallCount);
      }
    }

    private static void LogicTest(Type[] types, object callTarget, Type type, string methodName)
    {
      ExecutionSequenceHandler<ExecutionData>[] delegates =
        DelegateHelper.CreateDelegates<ExecutionSequenceHandler<ExecutionData>>(
          callTarget, type, methodName, types);

      Assert.AreEqual(types.Length, delegates.Length);
      for (int i = 0; i<types.Length; i++) {
        ExecutionSequenceHandler<ExecutionData> d =
          DelegateHelper.CreateDelegate<ExecutionSequenceHandler<ExecutionData>>(
            callTarget, type, methodName, types[i]);
        Assert.AreSame(d.Target, delegates[i].Target);
        Assert.AreSame(d.Method, delegates[i].Method);
      }

      ExecutionData data = ExecutionData.Create();
      DelegateHelper.ExecuteDelegates(delegates, ref data, Direction.Positive);
      Assert.IsTrue(AdvancedComparer<IEnumerable<Type>>.Default.Equals(types, data.CalledForTypes));

      data = ExecutionData.Create();
      DelegateHelper.ExecuteDelegates(delegates, ref data, Direction.Negative);
      Assert.IsTrue(AdvancedComparer<IEnumerable<Type>>.Default.Equals(types, Reverse(data.CalledForTypes)));
    }

    public static IEnumerable<TItem> Reverse<TItem>(IList<TItem> list)
    {
      ArgumentValidator.EnsureArgumentNotNull(list, "list");
      for (int i = list.Count-1; i>=0; i--)
        yield return list[i];
    }

    private bool SequenceAStep<T>(ref ExecutionData data, int index)
    {
      data.CalledForTypes.Add(typeof(T));
      return false;
    }

    private static bool SequenceAStaticStep<T>(ref ExecutionData data, int index)
    {
      data.CalledForTypes.Add(typeof(T));
      return false;
    }

    private bool SequenceBStep<T>(ref ExecutionData data, int index)
    {
      data.CallCount++;
      return false;
    }

    bool Execute<TFieldType>(ref ExecutionData actionData, int fieldIndex)
    {
      actionData.CallCount++;
      return false;
    }
  }
}