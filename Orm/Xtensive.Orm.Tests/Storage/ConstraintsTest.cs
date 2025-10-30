// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.08.18

using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.BookAuthorModel;

namespace Xtensive.Orm.Tests.Storage
{
  public class ConstraintsTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.RegisterCaching(Assembly.GetExecutingAssembly(), typeof(Book).Namespace);
      return config;
    }

    [Test]
    public void NotNullableViolationTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite);// no column length supported so no exception thrown
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var book = new Book();

          // Text is nullable so it's OK
          book.Text = null;

          // Title has length constraint (10 symbols) so InvalidOperationException is expected
          AssertEx.Throws<StorageException>(() => {
            book.Title = "01234567890";
            session.SaveChanges();
          });
        }
      }
    }

    [Test]
    public void SessionBoundaryViolationTest()
    {
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var author = new Author();
          using (var session2 = Domain.OpenSession()) {
            using (session2.OpenTransaction()) {
              var book = new Book();

              // Author is bound to another session
              AssertEx.ThrowsInvalidOperationException(() => book.Author = author);
            }
          }
        }
      }
    }
  }
}