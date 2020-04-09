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
      Assert.AreSame(typeof(List<int>).GetGenericType(typeof(List<>)), typeof(List<int>));

    [Test]
    public void ParameterizedGenericTypeIsDiscoverable_ByItsDirectNonGenericAncestor() =>
      Assert.AreSame(typeof(ListInt).GetGenericType(typeof(List<>)), typeof(List<int>));

    [Test]
    public void ParameterizedGenericTypeIsDiscoverable_ByItsIndirectNonGenericAncestor() =>
      Assert.AreSame(typeof(ListIntLvl1).GetGenericType(typeof(List<>)), typeof(List<int>));

    [Test]
    public void ParameterizedGenericTypeIsDiscoverable_ByItsDirectGenericAncestor() =>
      Assert.AreSame(typeof(GenericList<int>).GetGenericType(typeof(List<>)), typeof(List<int>));

    [Test]
    public void ParameterizedGenericTypeIsDiscoverable_ByItsIndirectGenericAncestor() =>
      Assert.AreSame(typeof(GenericListLvl1<int>).GetGenericType(typeof(List<>)), typeof(List<int>));

    [Test]
    public void ParameterizedGenericTypeIsDiscoverable_ByAnAncestorOfItsDirectGenericAncestor() =>
      Assert.AreSame(typeof(GenericListIntLvl1).GetGenericType(typeof(List<>)), typeof(List<int>));

    [Test]
    public void ParameterizedGenericTypeIsDiscoverable_ByAnAncestorOfItsIndirectGenericAncestor() =>
      Assert.AreSame(typeof(GenericListLvl1Int).GetGenericType(typeof(List<>)), typeof(List<int>));

    [Test]
    public void ParameterizedGenericInterfaceIsDiscoverable_ByItself() =>
      Assert.AreSame(typeof(IList<int>).GetGenericType(typeof(IList<>)), typeof(IList<int>));

    [Test]
    public void ParameterizedGenericInterfaceIsNotDiscoverable_ByItsImplementation() =>
      Assert.IsNull(typeof(List<int>).GetGenericType(typeof(IList<>)));

    [Test]
    public void NullIsReturnedIfNoMatchFound() =>
      Assert.IsNull(typeof(Stack<int>).GetGenericType(typeof(List<>)));

    [Test]
    public void NullIsAcceptedAsFirstParameter() =>
      Assert.IsNull(TypeHelper.GetGenericType(null, typeof(List<>)));
  }
}