using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Linq
{
  public class FullTextColumnsDeclarationTest : NorthwindDOModelTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    [Test]
    public void CorrectDirectFulltextFieldTest()
    {
      var columns = MakeUpColumns<Category>(t => t.CategoryName);
      RunQuery(columns);
    }

    [Test]
    public void CorrectDirectFulltextFieldsSetTest()
    {
      var columns = MakeUpColumns<Category>(t => t.CategoryName, t=>t.Description);
      RunQuery(columns);
    }

    [Test]
    public void StructureFulltextFieldTest()
    {
      var columns = MakeUpColumns<Employee>(t => t.Address.StreetAddress);
      RunQuery(columns);
    }

    [Test]
    [ExpectedException(typeof (QueryTranslationException))]
    public void StructureNonFulltextFieldTest()
    {
      var columns = MakeUpColumns<Employee>(t => t.Address.Region);
      RunQuery(columns);
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void NonPersistentFieldTest()
    {
      var columns = MakeUpColumns<Employee>(t => t.FullName);
      RunQuery(columns);
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void NonPersistentStructureFieldTest()
    {
      var columns = MakeUpColumns<Employee>(t => t.Address.JustAProperty);
      RunQuery(columns);
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void EntitySetFieldTest()
    {
      var columns = MakeUpColumns<Employee>(t => t.Orders);
      RunQuery(columns);
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void MethodAsColumnTest()
    {
      var columns = MakeUpColumns<Employee>(t => t.GetAge());
      RunQuery(columns);
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void ReferencedEntityFieldTest()
    {
      var columns = MakeUpColumns<Product>(t => t.Category.Description);
      RunQuery(columns);
    }

    [Test]
    public void EmptyColumnsArrayTest()
    {
      var columns = new Expression<Func<Category, object>>[0];
      Session.Query.ContainsTable<Category>(e => e.SimpleTerm("abc"), columns).Run();
    }

    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void NullColumnsArray()
    {
      Expression<Func<Category, object>>[] columns = null;
      Session.Query.ContainsTable<Category>(e => e.SimpleTerm("abc"), columns).Run();
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
