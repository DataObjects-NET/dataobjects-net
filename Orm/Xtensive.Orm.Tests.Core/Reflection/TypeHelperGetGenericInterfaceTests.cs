using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Reflection;

namespace Xtensive.Orm.Tests.Core.Reflection
{
  [TestFixture]
  public class TypeHelperGetGenericInterfaceTests
  {
    private class ListInt : List<int>
    { }

    private class ListIntLvl1 : ListInt
    { }

    private class GenericList<T> : List<T>
    { }

    private class GenericListLvl1<T> : GenericList<T>
    { }

    private class GenericListLvl1Int : GenericListLvl1<int>
    { }

    private class GenericListInt : GenericList<int>
    { }

    private class GenericListIntLvl1 : GenericListInt
    { }

    [Test]
    public void InterfaceIsDiscoverable_OnOpenGenericType() =>
      Assert.IsNotNull(typeof(List<>).GetGenericInterface(typeof(IList<>)));

    [Test]
    public void InterfaceIsDiscoverable_OnOpenGenericInterface() =>
      Assert.IsNotNull(typeof(IList<>).GetGenericInterface(typeof(ICollection<>)));

    [Test]
    public void InterfaceIsDiscoverable_OnClosedGenericType() =>
      Assert.AreSame(typeof(List<int>).GetGenericInterface(typeof(IList<>)), typeof(IList<int>));

    [Test]
    public void InterfaceIsDiscoverable_OnClosedGenericInterface() =>
      Assert.AreSame(typeof(IList<int>).GetGenericInterface(typeof(ICollection<>)), typeof(ICollection<int>));

    [Test]
    public void InterfaceIsDiscoverable_OnItself() =>
      Assert.AreSame(typeof(IList<int>).GetGenericInterface(typeof(IList<>)), typeof(IList<int>));

    [TestCase(typeof(ListInt))]
    [TestCase(typeof(ListIntLvl1))]
    [TestCase(typeof(GenericList<int>))]
    [TestCase(typeof(GenericListLvl1<int>))]
    [TestCase(typeof(GenericListLvl1Int))]
    [TestCase(typeof(GenericListInt))]
    [TestCase(typeof(GenericListIntLvl1))]
    public void InterfaceIsDiscoverable_OnAnyAncestorOfAnImplementor(Type type) =>
      Assert.AreSame(type.GetGenericInterface(typeof(IList<>)), typeof(IList<int>));

    [Test]
    public void NullIsReturnedIfNoMatchFound() =>
      Assert.IsNull(typeof(ICollection<int>).GetGenericInterface(typeof(IList<>)));

    [Test]
    public void NullIsAcceptedAsFirstParameter() =>
      Assert.IsNull(TypeHelper.GetGenericInterface(null, typeof(List<>)));
  }
}