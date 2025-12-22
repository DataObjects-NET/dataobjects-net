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
  public class TypeHelperGetGenericTypeTests
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
    public void ParameterizedGenericTypeIsDiscoverable_ByItself() =>
      Assert.That(typeof(List<int>), Is.SameAs(typeof(List<int>).GetGenericType(typeof(List<>))));

    [Test]
    public void ParameterizedGenericTypeIsDiscoverable_ByItsDirectNonGenericAncestor() =>
      Assert.That(typeof(List<int>), Is.SameAs(typeof(ListInt).GetGenericType(typeof(List<>))));

    [Test]
    public void ParameterizedGenericTypeIsDiscoverable_ByItsIndirectNonGenericAncestor() =>
      Assert.That(typeof(List<int>), Is.SameAs(typeof(ListIntLvl1).GetGenericType(typeof(List<>))));

    [Test]
    public void ParameterizedGenericTypeIsDiscoverable_ByItsDirectGenericAncestor() =>
      Assert.That(typeof(List<int>), Is.SameAs(typeof(GenericList<int>).GetGenericType(typeof(List<>))));

    [Test]
    public void ParameterizedGenericTypeIsDiscoverable_ByItsIndirectGenericAncestor() =>
      Assert.That(typeof(List<int>), Is.SameAs(typeof(GenericListLvl1<int>).GetGenericType(typeof(List<>))));

    [Test]
    public void ParameterizedGenericTypeIsDiscoverable_ByAnAncestorOfItsDirectGenericAncestor() =>
      Assert.That(typeof(List<int>), Is.SameAs(typeof(GenericListIntLvl1).GetGenericType(typeof(List<>))));

    [Test]
    public void ParameterizedGenericTypeIsDiscoverable_ByAnAncestorOfItsIndirectGenericAncestor() =>
      Assert.That(typeof(List<int>), Is.SameAs(typeof(GenericListLvl1Int).GetGenericType(typeof(List<>))));

    [Test]
    public void ParameterizedGenericInterfaceIsDiscoverable_ByItself() =>
      Assert.That(typeof(IList<int>), Is.SameAs(typeof(IList<int>).GetGenericType(typeof(IList<>))));

    [Test]
    public void ParameterizedGenericInterfaceIsNotDiscoverable_ByItsImplementation() =>
      Assert.That(typeof(List<int>).GetGenericType(typeof(IList<>)), Is.Null);

    [Test]
    public void NullIsReturnedIfNoMatchFound() =>
      Assert.That(typeof(Stack<int>).GetGenericType(typeof(List<>)), Is.Null);

    [Test]
    public void NullIsAcceptedAsFirstParameter() =>
      Assert.That(TypeHelper.GetGenericType(null, typeof(List<>)), Is.Null);
  }
}