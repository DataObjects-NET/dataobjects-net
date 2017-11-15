// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.19

using System;
using NUnit.Framework;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Comparer;
using Xtensive.Sql.Dom.Database.Providers;
using Assertion=Xtensive.Sql.Dom.Database.Assertion;

namespace Xtensive.Sql.Dom.Tests.MsSql
{
  [TestFixture]
  public class ComparerTests
  {
    public const string ConnectionString1 = @"mssql2005://localhost/AdventureWorks";
    public const string ConnectionString2 = @"mssql2005://localhost/AdventureWorksDW";

    private ComparisonResultNavigator navigator;

    [Test]
    [Explicit]
    [Category("Debug")]
    public void CatalogCompare()
    {
      Database.Model model1 = GetModel(ConnectionString1);
      Database.Model model2 = GetModel(ConnectionString2);
      var comparer = new SqlComparer();
      var catalogComparisonResult = comparer.Compare(model1.DefaultServer.DefaultCatalog, model2.DefaultServer.DefaultCatalog, null);
    }

    [Test]
    [Explicit]
    [Category("Debug")]
    public void CompareEqualNodes()
    {
      Database.Model model1 = GetModel(ConnectionString1);
      var comparer = new SqlComparer();
      comparer.Compare(model1.DefaultServer.DefaultCatalog, model1.DefaultServer.DefaultCatalog, null);
      var singleResult = comparer.Compare(model1.DefaultServer.DefaultCatalog, model1.DefaultServer.DefaultCatalog, null);
      foreach (IComparisonResult comparisonResult in singleResult.NestedComparisons) {
        Assert.IsTrue(comparisonResult.ResultType == ComparisonResultType.Unchanged);
      }
    }

    [Test]
    [Explicit]
    [Category("Debug")]
    public void NavigatorTest()
    {
      Database.Model model1 = GetModel(ConnectionString1);
      Database.Model model2 = GetModel(ConnectionString2);
      Catalog catalog1 = model1.DefaultServer.DefaultCatalog;
      Catalog catalog2 = model2.DefaultServer.DefaultCatalog;
      var result = new SqlComparer().Compare(catalog1, catalog2, null);
      navigator = new ComparisonResultNavigator(result);
      CheckCatalog(catalog1, true);
      CheckCatalog(catalog2, false);
    }

    private void CheckCatalog(Catalog catalog, bool isOriginal)
    {
      CheckNode(catalog, isOriginal);
      CheckNode(catalog.DefaultSchema, isOriginal);
      foreach (Schema schema in catalog.Schemas) {
        CheckNode(schema, isOriginal);
        foreach (Assertion assertion in schema.Assertions)
          CheckNode(assertion, isOriginal);
        foreach (CharacterSet characterSet in schema.CharacterSets)
          CheckNode(characterSet, isOriginal);
        foreach (Collation collation in schema.Collations)
          CheckNode(collation, isOriginal);
        foreach (Domain domain in schema.Domains) {
          CheckNode(domain, isOriginal);
          foreach (DomainConstraint constraint in domain.DomainConstraints)
            CheckNode(constraint, isOriginal);
        }
        foreach (Sequence sequence in schema.Sequences)
          CheckNode(sequence, isOriginal);
        foreach (Table table in schema.Tables) {
          CheckNode(table, isOriginal);
          foreach (DataTableColumn column in table.Columns)
            CheckNode(column, isOriginal);
          foreach (TableConstraint constraint in table.TableConstraints)
            CheckNode(constraint, isOriginal);
          foreach (Index index in table.Indexes)
            CheckNode(index, isOriginal);
        }
        foreach (Translation translation in schema.Translations)
          CheckNode(translation, isOriginal);
        foreach (View view in schema.Views) {
          CheckNode(view, isOriginal);
          foreach (DataTableColumn column in view.Columns)
            CheckNode(column, isOriginal);
          foreach (Index index in view.Indexes)
            CheckNode(index, isOriginal);
        }
      }
    }

    private void CheckNode(Node node, bool isOriginal)
    {
      var result = navigator.Find(node);
      Assert.IsNotNull(result);
      Assert.IsTrue(isOriginal ? result.OriginalValue==node : result.NewValue==node);
      Assert.IsTrue((result.ResultType & ComparisonResultType.Modified | (isOriginal ? ComparisonResultType.Removed : ComparisonResultType.Added)) > 0);
    }

    private Model GetModel(string connectionString)
    {
      var provider = new SqlConnectionProvider();
      using (var connection = provider.CreateConnection(connectionString) as SqlConnection) {
        if (connection==null)
          throw new InvalidOperationException(string.Format("Unable to connect to {0}", connectionString));
        connection.Open();
        var modelProvider = new SqlModelProvider(connection);
        return Model.Build(modelProvider);
      }
    }
  }
}