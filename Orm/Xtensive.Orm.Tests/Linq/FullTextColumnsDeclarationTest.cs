using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  public class FullTextColumnsDeclarationTest : ChinookDOModelTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    [Test]
    public void CorrectDirectFulltextFieldTest()
    {
      var columns = MakeUpColumns<Track>(t => t.Name);
      RunQuery(columns);
    }

    [Test]
    public void CorrectDirectFulltextFieldsSetTest()
    {
      var columns = MakeUpColumns<Track>(t => t.Name, t=>t.Composer);
      RunQuery(columns);
    }

    [Test]
    public void StructureFulltextFieldTest()
    {
      var columns = MakeUpColumns<Employee>(t => t.Address.StreetAddress);
      RunQuery(columns);
    }

    [Test]
    public void StructureNonFulltextFieldTest()
    {
      var columns = MakeUpColumns<Employee>(t => t.Address.Country);
      Assert.Throws<QueryTranslationException>(() => RunQuery(columns));
    }

    [Test]
    public void NonPersistentFieldTest()
    {
      var columns = MakeUpColumns<Employee>(t => t.FirstName);
      Assert.Throws<QueryTranslationException>(() => RunQuery(columns));
    }

    [Test]
    public void NonPersistentStructureFieldTest()
    {
      var columns = MakeUpColumns<Employee>(t => t.Address.JustAProperty);
      Assert.Throws<QueryTranslationException>(() => RunQuery(columns));
    }

    [Test]
    public void EntitySetFieldTest()
    {
      var columns = MakeUpColumns<Customer>(t => t.Invoices);
      Assert.Throws<QueryTranslationException>(() => RunQuery(columns));
    }

    [Test]
    public void MethodAsColumnTest()
    {
      var columns = MakeUpColumns<Employee>(t => t.GetAge());
      Assert.Throws<QueryTranslationException>(() => RunQuery(columns));
    }

    [Test]
    public void ReferencedEntityFieldTest()
    {
      var columns = MakeUpColumns<Track>(t => t.Album.Title);
      Assert.Throws<QueryTranslationException>(() => RunQuery(columns));
    }

    [Test]
    public void EmptyColumnsArrayTest()
    {
      var columns = new Expression<Func<Track, object>>[0];
      Session.Query.ContainsTable(e => e.SimpleTerm("abc"), columns).Run();
    }

    [Test]
    public void NullColumnsArray()
    {
      Expression<Func<Track, object>>[] columns = null;
      Assert.Throws<ArgumentNullException>(() => Session.Query.ContainsTable(e => e.SimpleTerm("abc"), columns).Run());
    }

    private void RunQuery<T>(Expression<Func<T, object>>[] columns)
      where T:Entity
    {
      Session.Query.ContainsTable<T>(e => e.SimpleTerm("abc"), columns).Run();
    }

    private Expression<Func<T, object>>[] MakeUpColumns<T>(Expression<Func<T, object>> column, params Expression<Func<T, object>>[] additionalColumns)
      where T : Entity
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "column");
      var columns = new List<Expression<Func<T, object>>>();
      columns.Add(column);
      if (additionalColumns!=null) {
        columns.AddRange(additionalColumns);
      }
      return columns.ToArray();
    }
  }
}
