using System;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;

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
      int removedCount = set.RemoveWhere(delegate(int match) { return match < removeCount; });
      Assert.AreEqual(removeCount, removedCount);
    }


    [Test]
    public void CombinedTest()
    {
      SetSlim<string> A = new SetSlim<string>();
      SetSlim<string> B = new SetSlim<string>();

      A.Add("a");
      A.Add("b");
      A.Add("c");
      A.Add("d");

      B.Add("c");
      B.Add("d");
      B.Add("e");
      B.Add("f");

      SetSlim<string> C = A.Intersect<string, SetSlim<string>>(B);
      SetSlim<string> D = A.SymmetricExcept<string, SetSlim<string>>(B);

      if (C.Count!=2) {
        throw new Exception("SetBase<T>.Intersection");
      }

      if (D.Count!=4) {
        throw new Exception("SetBase<T>.SymmetricDifference");
      }

      SetSlim<string> AB = A.Union<string, SetSlim<string>>(B);
      SetSlim<string> CD = C.Union<string, SetSlim<string>>(D);

      if (!AB.IsEqualTo(CD)) {
        throw new Exception("SetBase<T>.Union");
      }

      if (!C.IsDisjointWith(D)) {
        throw new Exception("SetBase<T>.IsDisjointWith");
      }

      if (!C.IsProperSubsetOf(A)) {
        throw new Exception("SetBase<T>.IsProperSubsetOf");
      }

      if (D.IsProperSubsetOf(A)) {
        throw new Exception("SetBase<T>.IsProperSubsetOf");
      }

      if (!A.IsProperSupersetOf(C)) {
        throw new Exception("SetBase<T>.IsProperSupersetOf");
      }

      if (A.IsProperSupersetOf(D)) {
        throw new Exception("SetBase<T>.IsProperSupersetOf");
      }

      if (A.IsProperSubsetOf(A) || A.IsProperSupersetOf(A)) {
        throw new Exception("SetBase<T>.IsProper...Of");
      }

      SetSlim<string> A1 = A.Except<string, SetSlim<string>>(B);
      SetSlim<string> B1 = B.Except<string, SetSlim<string>>(A);

      if (!D.IsEqualTo(A1.Union<string, SetSlim<string>>(B1))) {
        throw new Exception("SetBase<T>.Difference");
      }
    }

    [Test]
    public void Add()
    {
      SetSlim<string> A = new SetSlim<string>();
      string[] strings = {"abc", "dfg", "ag", "abc"};
      A.UnionWith(strings);

      SetSlim<string> C = new SetSlim<string>(strings);

      SetSlim<int> B = new SetSlim<int>();
      B.Add(1);
      B.Add(2);
      B.Add(2);
      B.Add(3);

      if (A.Count!=3 || B.Count!=3 || C.Count!=3) {
        throw new Exception("Set.AddRange");
      }
    }

//    [Test]
//    [ExpectedException(typeof(System.InvalidCastException))]
//    public void AddRange()
//    {
//      Set<string> set = new Set<string>();
//      object[] obj = new object[3];
//      int i = 3;
//      string s = "Hello";
//      byte b = 129;
//      obj[0] = i;
//      obj[1] = s;
//      obj[2] = b;
//      set.UnionWith(obj);
//    }

    public void PopulateSet(SetSlim<string> set, int count)
    {
      Guid g;
      for (int i = 0; i < count; i++) {
        g = Guid.NewGuid();
        set.Add(g.ToString());
      }
    }

    [Test]
    public void Difference()
    {
      SetSlim<string> setA = new SetSlim<string>();
      PopulateSet(setA, 5);
      SetSlim<string> setB = new SetSlim<string>();
      PopulateSet(setB, 5);
      SetSlim<string> setC = new SetSlim<string>();
      PopulateSet(setC, 5);

      setA.UnionWith(setC);
      setB.UnionWith(setC);

      setA.ExceptWith(setC);
      if (setA.Count!=5) {
        throw new Exception("Set.ExceptWith");
      }

      setB = setB.Except<string, SetSlim<string>>(setC);
      if (setB.Count!=5) {
        throw new Exception("Set.Difference");
      }
    }

    [Test]
    public void Intersection()
    {
      SetSlim<string> setA = new SetSlim<string>();
      PopulateSet(setA, 5);
      SetSlim<string> setB = new SetSlim<string>();
      PopulateSet(setB, 5);
      SetSlim<string> setC = new SetSlim<string>();
      PopulateSet(setC, 5);

      setA.UnionWith(setC);
      setB.UnionWith(setC);

      if (!setA.Intersect<string, SetSlim<string>>(setB).IsEqualTo(setC)) {
        throw new Exception("Set.Intersection");
      }

      setA.IntersectWith(setB);
      if (!setA.IsEqualTo(setC)) {
        throw new Exception("Set.Intersection");
      }
    }

    [Test]
    public void DisjointFrom()
    {
      SetSlim<string> setA = new SetSlim<string>();
      PopulateSet(setA, 5);
      SetSlim<string> setB = new SetSlim<string>();
      PopulateSet(setB, 5);

      if (!setA.IsDisjointWith(setB)) {
        throw new Exception("Set.IsDisjointWith");
      }
    }

    [Test]
    public void ProperSubsetOf()
    {
      SetSlim<string> setA = new SetSlim<string>();
      PopulateSet(setA, 5);
      SetSlim<string> setC = new SetSlim<string>();
      PopulateSet(setC, 5);

      setA.UnionWith(setC);

      if (setA.IsProperSubsetOf(setA)) {
        throw new Exception("Set.IsProperSubsetOf");
      }

      if (!setC.IsProperSubsetOf(setA)) {
        throw new Exception("Set.IsProperSubsetOf");
      }
    }

    [Test]
    public void ProperSupersetOf()
    {
      SetSlim<string> setA = new SetSlim<string>();
      PopulateSet(setA, 5);
      SetSlim<string> setC = new SetSlim<string>();
      PopulateSet(setC, 5);

      setA.UnionWith(setC);

      if (!setA.IsProperSupersetOf(setC)) {
        throw new Exception("Set.IsProperSupersetOf");
      }

      if (setA.IsProperSupersetOf(setA)) {
        throw new Exception("Set.IsProperSupersetOf");
      }
    }

    [Test]
    public void SubsetOf()
    {
      SetSlim<string> setA = new SetSlim<string>();
      PopulateSet(setA, 5);
      SetSlim<string> setC = new SetSlim<string>();
      PopulateSet(setC, 5);

      setA.UnionWith(setC);

      if (!setA.IsSubsetOf(setA)) {
        throw new Exception("Set.IsSubsetOf");
      }

      if (!setC.IsSubsetOf(setA)) {
        throw new Exception("Set.IsSubsetOf");
      }
    }

    [Test]
    public void SupersetOf()
    {
      SetSlim<string> setA = new SetSlim<string>();
      PopulateSet(setA, 5);
      SetSlim<string> setC = new SetSlim<string>();
      PopulateSet(setC, 5);

      setA.UnionWith(setC);

      if (!setA.IsSupersetOf(setC)) {
        throw new Exception("Set.IsSupersetOf");
      }

      if (!setA.IsSupersetOf(setA)) {
        throw new Exception("Set.IsSupersetOf");
      }
    }

//    [Test]
//    public void RemoveRange()
//    {
//      Set<string> setA = new Set<string>();
//      AddToSet(setA, 5);
//      Set<string> setB = new Set<string>();
//      AddToSet(setB, 5);
//      Set<string> setC = new Set<string>();
//      AddToSet(setC, 5);
//
//      setA.UnionWith(setC);
//      setB.UnionWith(setC);
//
//      setA.RemoveRange(setB);
//
//      if (setA.Count != 5) {
//        throw new Exception("Set.RemoveRange");
//      }
//    }

    [Test]
    public void SymmetricDifference()
    {
      SetSlim<string> setA = new SetSlim<string>();
      PopulateSet(setA, 5);
      SetSlim<string> setB = new SetSlim<string>();
      PopulateSet(setB, 5);
      SetSlim<string> setC = new SetSlim<string>();
      PopulateSet(setC, 5);

      setA.UnionWith(setC);
      setB.UnionWith(setC);

      if (!setA.SymmetricExcept<string, SetSlim<string>>(setB).IsDisjointWith(setC)) {
        throw new Exception("Set.SymmetricDifference");
      }

      setA.SymmetricExceptWith(setB);

      if (!setA.IsDisjointWith(setC)) {
        throw new Exception("Set.SymmetricDifference");
      }
    }
  }
}