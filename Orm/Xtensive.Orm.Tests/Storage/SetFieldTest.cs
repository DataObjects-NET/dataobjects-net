// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage.SetFieldTest
{
  [Serializable]
  [HierarchyRoot]
  public class Book : Entity
  {
    [Key, Field]
    public Guid Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    public DateTime Date { get; set; }

#if DO_DATEONLY
    [Field]
    public DateOnly DateOnly { get; set; }
#endif

    [Field]
    public Direction? NullableDirection { get; set; }

    [Field]
    public byte[] Image { get; set; }

    [Field]
    public Book Pair { get; set; }
  }

  [TestFixture]
  public class SetFieldTest : AutoBuildTest
  {
    private int fieldSetCallCount;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Book).Assembly, typeof(Book).Namespace);
      return configuration;
    }

    [Test]
    public void CombinedTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.Events.EntityFieldValueSetting += (sender, e) => {
          fieldSetCallCount++;
        };

        var book = new Book();
        AssertIsCalled(() => { book.Title = "A"; });
        AssertIsNotCalled(() => { book.Title = "A"; });
        AssertIsCalled(() => { book.Date = new DateTime(1, 2, 3); });
        AssertIsNotCalled(() => { book.Date = new DateTime(1, 2, 3); });

#if DO_DATEONLY
        AssertIsCalled(() => { book.DateOnly = new DateOnly(1, 2, 3); });
        AssertIsNotCalled(() => { book.DateOnly = new DateOnly(1, 2, 3); });
#endif

        var image = new byte[] { 1, 2, 3 };
        AssertIsCalled(() => { book.Image = image; });
        AssertIsCalled(() => { book.Image = image; });

        AssertIsNotCalled(() => { book.Pair = null; });
        AssertIsCalled(() => { book.Pair = book; });
        AssertIsNotCalled(() => { book.Pair = book; });
        AssertIsCalled(() => { book.Pair = null; });
        AssertIsNotCalled(() => { book.Pair = null; });
      }
    }

    [Test]
    public void NullableEnumSetTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var book = new Book();
        book.NullableDirection = Direction.Positive;
        book.NullableDirection = null;
      }
    }

    private void AssertIsCalled(Action action)
    {
      int oldCallCount = fieldSetCallCount;
      action.Invoke();
      if (fieldSetCallCount == oldCallCount)
        Assert.Fail("Expected event didn't occur.");
    }

    private void AssertIsNotCalled(Action action)
    {
      int oldCallCount = fieldSetCallCount;
      action.Invoke();
      if (fieldSetCallCount != oldCallCount)
        Assert.Fail("Event occurred, although it shouldn't.");
    }
  }
}