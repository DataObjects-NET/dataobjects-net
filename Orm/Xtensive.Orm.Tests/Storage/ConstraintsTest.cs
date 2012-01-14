// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.18

using System.Reflection;
using NUnit.Framework;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.BookAuthorModel;

namespace Xtensive.Orm.Tests.Storage
{
  public class ConstraintsTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.Storage.BookAuthorModel");
      return config;
    }

    [Test]
    public void NotNullableViolationTest()
    {
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var book = new Book();

          // Text is nullable so it's OK
          book.Text = null;

          // Title has length constraint (10 symbols) so InvalidOperationException is expected
          AssertEx.ThrowsInvalidOperationException(() => book.Title = "01234567890");
        }
      }
    }

    [Test]
    public void SessionBoundaryViolationTest()
    {
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          Author author = new Author();
          using (var session2 = Domain.OpenSession()) {
            using (session2.OpenTransaction()) {
              Book book = new Book();

              // Author is bound to another session
              AssertEx.ThrowsInvalidOperationException(() => book.Author = author);
            }
          }
        }
      }
    }
  }
}