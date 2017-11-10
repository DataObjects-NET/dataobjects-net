using System;
using NUnit.Framework;
using Xtensive.Collections;

namespace Xtensive.Orm.Tests.Core.Collections
{
  [TestFixture]
  public class SetTestA
  {
    [Test]
    public void RemoveAt()
    {
      int count = 10;
      int removeCount = 5;
      Assert.LessOrEqual(removeCount, count);
      ISet<int> set = new Set<int>();
      for (int i = 0; i < count; i++)
        set.Add(i);
      Assert.AreEqual(count, set.Count);
    }

    [Test]
    public void Add()
    {
      string[] strings = {"abc", "dfg", "ag", "abc"};

      SetSlim<string> C = new SetSlim<string>(strings);

      SetSlim<int> B = new SetSlim<int>();
      B.Add(1);
      B.Add(2);
      B.Add(2);
      B.Add(3);

      if (B.Count!=3 || C.Count!=3) {
        throw new Exception("Set.AddRange");
      }
    }
  }
}