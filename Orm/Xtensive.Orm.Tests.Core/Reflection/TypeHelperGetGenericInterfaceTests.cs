// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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
      Assert.That(typeof(List<>).GetGenericInterface(typeof(IList<>)), Is.Not.Null);

    [Test]
    public void InterfaceIsDiscoverable_OnOpenGenericInterface() =>
      Assert.That(typeof(IList<>).GetGenericInterface(typeof(ICollection<>)), Is.Not.Null);

    [Test]
    public void InterfaceIsDiscoverable_OnClosedGenericType() =>
      Assert.That(typeof(IList<int>), Is.SameAs(typeof(List<int>).GetGenericInterface(typeof(IList<>))));

    [Test]
    public void InterfaceIsDiscoverable_OnClosedGenericInterface() =>
      Assert.That(typeof(ICollection<int>), Is.SameAs(typeof(IList<int>).GetGenericInterface(typeof(ICollection<>))));

    [Test]
    public void InterfaceIsDiscoverable_OnItself() =>
      Assert.That(typeof(IList<int>), Is.SameAs(typeof(IList<int>).GetGenericInterface(typeof(IList<>))));

    [TestCase(typeof(ListInt))]
    [TestCase(typeof(ListIntLvl1))]
    [TestCase(typeof(GenericList<int>))]
    [TestCase(typeof(GenericListLvl1<int>))]
    [TestCase(typeof(GenericListLvl1Int))]
    [TestCase(typeof(GenericListInt))]
    [TestCase(typeof(GenericListIntLvl1))]
    public void InterfaceIsDiscoverable_OnAnyAncestorOfAnImplementor(Type type) =>
      Assert.That(typeof(IList<int>), Is.SameAs(type.GetGenericInterface(typeof(IList<>))));

    [Test]
    public void NullIsReturnedIfNoMatchFound() =>
      Assert.That(typeof(ICollection<int>).GetGenericInterface(typeof(IList<>)), Is.Null);

    [Test]
    public void NullIsAcceptedAsFirstParameter() =>
      Assert.That(TypeHelper.GetGenericInterface(null, typeof(List<>)), Is.Null);
  }
}