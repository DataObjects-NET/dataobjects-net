// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.01

using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Tuples;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Tests.Performance
{
  [TestFixture]
  public class TuplePerformanceTest
  {
    public const int BaseCount = 10000000;
    private bool warmup  = false;
    private bool profile = false;

    [Test]
    public void RegularTest()
    {
      warmup = true;
      CombinedTest(10);
      warmup = false;
      CombinedTest(BaseCount);
    }

    [Test]
    [Explicit]
    [Category("Profile")]
    public void ProfileTest()
    {
      CombinedTest(BaseCount);
    }

    private void CombinedTest(int count)
    {
      Tuple t = Tuple.Create(1, 2L, 3, "4", (object)null);
      TupleTest(t, count);
      t = t.ToFastReadOnly();
      TupleTest(t, count);

}

    private void TupleTest(Tuple t, int count)
    {
      using (Log.InfoRegion("Testing {0}", t.GetType().GetShortName())) {
        using (warmup ? null : new Measurement("GetAllValues", count * t.Count))
          GetAllValuesTest(t, count);
        for (int i = 0; i<t.Count; i++)
        using (warmup ? null : new Measurement(string.Format(
          "GetValue({0}) ({1})", i, t.Descriptor[i].GetShortName()), 
          count * t.Count))
          GetValueTest(t, count, i);
        using (warmup ? null : new Measurement("ToFastReadOnly", count))
          ToFastReadOnlyTest(t, count);
      }
    }

    private void GetAllValuesTest(Tuple t, int count)
    {
      int fieldCount = t.Count;
      for (int i = 0; i < count; i++)
        for (int j = 0; j < fieldCount; j++) {
          TupleFieldState state;
          t.GetValue(j, out state);
        }
    }

    private void GetValueTest(Tuple t, int count, int index)
    {
      for (int i = 0; i < count; i++) {
        TupleFieldState state;
        t.GetValue(index, out state);
      }
    }

    private void ToFastReadOnlyTest(Tuple t, int count)
    {
      for (int i = 0; i < count; i++)
        t.ToFastReadOnly();
    }
  }
}