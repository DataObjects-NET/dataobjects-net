// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Index = Xtensive.Sql.Model.Index;

namespace Xtensive.Orm.Tests.Sql.PostgreSql
{
  internal class CatalogComparer
  {
    private readonly SqlConnection connection;

    public void CompareCatalogs(Catalog created, Catalog extracted)
    {
      Assert.That(extracted.Name, Is.EqualTo(created.Name));

      foreach (var s1 in created.Schemas) {
        var s2 = extracted.Schemas[s1.Name];
        Assert.That(s2, Is.Not.Null);
        CompareSchemas(s1, s2);
      }
    }

    private void CompareSchemas(Schema s1, Schema s2)
    {
      Assert.That(s1, Is.Not.Null);
      Assert.That(s2, Is.Not.Null);
      Assert.That(s1.Name, Is.Not.Null);
      Assert.That(s2.Name, Is.Not.Null);
      Assert.That(s2.Name, Is.EqualTo(s1.Name));
      //The created model does not assign Owner, it is assigned by PgSql implicitly
      //Assert.IsNotNull(s1.Owner);
      Assert.That(s2.Owner, Is.Not.Null);

      Assert.That(s2.Domains.Count, Is.EqualTo(s1.Domains.Count));
      Assert.That(s2.Sequences.Count, Is.EqualTo(s1.Sequences.Count));
      Assert.That(s2.Tables.Count, Is.EqualTo(s1.Tables.Count));
      Assert.That(s2.Views.Count, Is.EqualTo(s1.Views.Count));

      foreach (var d1 in s1.Domains) {
        var d2 = s2.Domains[d1.Name];
        Assert.That(d2, Is.Not.Null);

        CompareDomains(d1, d2);
      }

      foreach (var sq1 in s1.Sequences) {
        var sq2 = s2.Sequences[sq1.Name];
        Assert.That(sq2, Is.Not.Null);

        CompareSequences(sq1, sq2);
      }

      foreach (var t1 in s1.Tables) {
        var t2 = s2.Tables[t1.Name];
        Assert.That(t2, Is.Not.Null);

        CompareTables(t1, t2);
      }

      foreach (var v1 in s1.Views) {
        var v2 = s2.Views[v1.Name];
        Assert.That(v2, Is.Not.Null);

        CompareViews(v1, v2);
      }
    }

    private void CompareDomains(Xtensive.Sql.Model.Domain d1, Xtensive.Sql.Model.Domain d2)
    {
      Assert.That(d1.Name, Is.Not.Null);
      Assert.That(d2.Name, Is.Not.Null);
      Assert.That(d2.Name, Is.EqualTo(d1.Name));
      CompareSqlValueTypes(d1.DataType, d2.DataType);
      Assert.That(d2.Schema.Name, Is.EqualTo(d1.Schema.Name));

      foreach (DomainConstraint dc1 in d1.DomainConstraints) {
        DomainConstraint dc2 = d2.DomainConstraints[dc1.Name];
        Assert.That(dc2, Is.Not.Null);
        CompareDomainConstraints(dc1, dc2);
      }
    }

    private void CompareDomainConstraints(DomainConstraint dc1, DomainConstraint dc2)
    {
      Assert.That(dc1.Domain, Is.Not.Null);
      Assert.That(dc2.Domain, Is.EqualTo(dc1.Domain));
      Assert.That(dc2.IsDeferrable, Is.EqualTo(dc1.IsDeferrable));
      Assert.That(dc2.IsInitiallyDeferred, Is.EqualTo(dc1.IsInitiallyDeferred));
      Assert.That(dc1.Owner, Is.Not.Null);
      Assert.That(dc2.Owner.Name, Is.EqualTo(dc1.Owner.Name));
    }

    private void CompareSequences(Sequence sq1, Sequence sq2)
    {
      Assert.That(sq1, Is.Not.Null);
      Assert.That(sq2, Is.Not.Null);
      Assert.That(sq2.Name, Is.Not.Null);
      Assert.That(sq2.Name, Is.EqualTo(sq1.Name));
      Assert.That(sq2.Schema.Name, Is.EqualTo(sq1.Schema.Name));
      //When creating the model, these values are not specified, but gets default values by PgSql
      //if(sq1.SequenceDescriptor.Increment!=null)
      Assert.That(sq2.SequenceDescriptor.Increment, Is.EqualTo(sq1.SequenceDescriptor.Increment ?? 1));
      //if(sq1.SequenceDescriptor.IsCyclic != null)
      Assert.That(sq2.SequenceDescriptor.IsCyclic, Is.EqualTo(sq1.SequenceDescriptor.IsCyclic ?? false));
      if (sq1.SequenceDescriptor.MaxValue != null)
        Assert.That(sq2.SequenceDescriptor.MaxValue, Is.EqualTo(sq1.SequenceDescriptor.MaxValue));
      if (sq1.SequenceDescriptor.MinValue != null)
        Assert.That(sq2.SequenceDescriptor.MinValue, Is.EqualTo(sq1.SequenceDescriptor.MinValue));
      //start value cannot be extracted
      /*
      if(sq1.SequenceDescriptor.StartValue != null)
        Assert.AreEqual(sq1.SequenceDescriptor.StartValue, sq2.SequenceDescriptor.StartValue);
      /**/
    }

    private void CompareSqlValueTypes(SqlValueType t1, SqlValueType t2)
    {
      Assert.That(t1, Is.Not.Null);
      Assert.That(t2, Is.Not.Null);
      Assert.That(t2.Type, Is.EqualTo(t1.Type));
      Assert.That(t2.Precision, Is.EqualTo(t1.Precision));
      Assert.That(t2.Scale, Is.EqualTo(t1.Scale));
      Assert.That(t2.Length, Is.EqualTo(t1.Length));
    }

    private void CompareTables(Table t1, Table t2)
    {
      Assert.That(t1, Is.Not.Null);
      Assert.That(t2, Is.Not.Null);
      Assert.That(t2.Filegroup, Is.EqualTo(t1.Filegroup));
      Assert.That(t2.Schema.Name, Is.EqualTo(t1.Schema.Name));
      Assert.That(t2.TableColumns.Count, Is.EqualTo(t1.TableColumns.Count));
      Assert.That(t2.TableConstraints.Count, Is.EqualTo(t1.TableConstraints.Count));

      foreach (var c1 in t1.TableColumns) {
        var c2 = t2.TableColumns[c1.Name];
        Assert.That(c2, Is.Not.Null);
        CompareTableColumns(c1, c2);
      }

      foreach (var i1 in t1.Indexes) {
        var i2 = t2.Indexes[i1.Name];
        Assert.That(i2, Is.Not.Null);
        CompareTableIndexes(i1, i2);
      }

      foreach (TableConstraint tc1 in t1.TableConstraints) {
        var tc2 = t2.TableConstraints[tc1.Name];
        Assert.That(tc2, Is.Not.Null);
        if (tc1 is CheckConstraint)
          CompareCheckConstraints(tc1 as CheckConstraint, tc2 as CheckConstraint);
        else if (tc1 is PrimaryKey)
          ComparePrimaryKeys(tc1 as PrimaryKey, tc2 as PrimaryKey);
        else if (tc1 is UniqueConstraint)
          CompareUniqueConstraints(tc1 as UniqueConstraint, tc2 as UniqueConstraint);
        else if (tc1 is ForeignKey)
          CompareForeignKeys(tc1 as ForeignKey, tc2 as ForeignKey);
      }
    }

    private void CompareTableColumns(TableColumn c1, TableColumn c2)
    {
      Assert.That(c1, Is.Not.Null);
      Assert.That(c2, Is.Not.Null);
      Assert.That(c2.DataTable.Name, Is.EqualTo(c1.DataTable.Name));
      CompareSqlValueTypes(c1.DataType, c2.DataType);
      Assert.That(c1.Domain == null && c2.Domain == null
        || c1.Domain != null && c2.Domain != null && c1.Domain.Name == c2.Domain.Name, Is.True);
      Assert.That(c2.IsNullable, Is.EqualTo(c1.IsNullable));
      Assert.That(c2.Table.Name, Is.EqualTo(c1.Table.Name));
    }

    private void CompareTableIndexes(Index i1, Index i2)
    {
      Assert.That(i1, Is.Not.Null);
      Assert.That(i2, Is.Not.Null);
      Assert.That(i2.DataTable.Name, Is.EqualTo(i1.DataTable.Name));
      Assert.That(i2.Filegroup, Is.EqualTo(i1.Filegroup));

      var ver = connection.Driver.CoreServerInfo.ServerVersion;
      if (ver.Major * 100 + ver.Minor >= 802) {
        if (i1.FillFactor != null) {
          Assert.That(i2.FillFactor, Is.EqualTo(i1.FillFactor));
        }
      }
      Assert.That(i2.IsBitmap, Is.EqualTo(i1.IsBitmap));
      Assert.That(i2.IsClustered, Is.EqualTo(i1.IsClustered));
      Assert.That(i2.IsUnique, Is.EqualTo(i1.IsUnique));

      Assert.That(i2.Columns.Count, Is.EqualTo(i1.Columns.Count));
      foreach (var ic1 in i1.Columns) {
        var ic2 = i2.Columns[ic1.Name];
        Assert.That(ic2, Is.Not.Null);
        CompareIndexColumns(ic1, ic2);
      }
    }

    private void CompareIndexColumns(IndexColumn ic1, IndexColumn ic2)
    {
      Assert.That(ic1, Is.Not.Null);
      Assert.That(ic2, Is.Not.Null);
      Assert.That(ic2.Name, Is.Not.Null);
      Assert.That(ic2.Name, Is.EqualTo(ic1.Name));
      Assert.That(ic2.Ascending, Is.EqualTo(ic1.Ascending));
      Assert.That(ic2.Column.Name, Is.EqualTo(ic1.Column.Name));
      Assert.That(ic2.Index.Name, Is.EqualTo(ic1.Index.Name));
    }

    private void CompareTableConstraints(TableConstraint tc1, TableConstraint tc2)
    {
      Assert.That(tc2.Name, Is.EqualTo(tc1.Name));
      Assert.That(tc2.Owner.Name, Is.EqualTo(tc1.Owner.Name));
      Assert.That(tc2.Table.Name, Is.EqualTo(tc1.Table.Name));
    }

    private void CompareCheckConstraints(CheckConstraint cc1, CheckConstraint cc2)
    {
      CompareTableConstraints(cc1, cc2);
    }

    private void CompareUniqueConstraints(UniqueConstraint cc1, UniqueConstraint cc2)
    {
      CompareTableConstraints(cc1, cc2);

      Assert.That(cc2.Columns.Count, Is.EqualTo(cc1.Columns.Count));
      foreach (var tc1 in cc1.Columns) {
        var tc2 = cc2.Columns[tc1.Name];
        Assert.That(tc2, Is.Not.Null);
        Assert.That(tc2.Table.Schema.Name, Is.EqualTo(tc1.Table.Schema.Name));
        Assert.That(tc2.Table.Name, Is.EqualTo(tc1.Table.Name));
      }
    }

    private void ComparePrimaryKeys(PrimaryKey pk1, PrimaryKey pk2)
    {
      CompareUniqueConstraints(pk1, pk2);
    }

    private void CompareForeignKeys(ForeignKey fk1, ForeignKey fk2)
    {
      CompareTableConstraints(fk1, fk2);
      Assert.That(fk2.IsDeferrable, Is.EqualTo(fk1.IsDeferrable ?? false));
      Assert.That(fk2.IsInitiallyDeferred, Is.EqualTo(fk1.IsInitiallyDeferred ?? false));
      Assert.That(fk2.MatchType, Is.EqualTo(fk1.MatchType));
      Assert.That(fk2.OnDelete, Is.EqualTo(fk1.OnDelete));
      Assert.That(fk2.OnUpdate, Is.EqualTo(fk1.OnUpdate));
      Assert.That(fk2.Table.Schema.Name, Is.EqualTo(fk1.Table.Schema.Name));
      Assert.That(fk2.Table.Name, Is.EqualTo(fk1.Table.Name));
      //columns
      Assert.That(fk2.Columns.Count, Is.EqualTo(fk1.Columns.Count));
      foreach (var tc1 in fk1.Columns) {
        var tc2 = fk2.Columns[tc1.Name];
        Assert.That(tc2, Is.Not.Null);
        Assert.That(tc2.Table.Schema.Name, Is.EqualTo(tc1.Table.Schema.Name));
        Assert.That(tc2.Table.Name, Is.EqualTo(tc1.Table.Name));
      }

      Assert.That(fk2.ReferencedTable.Schema.Name, Is.EqualTo(fk1.ReferencedTable.Schema.Name));
      Assert.That(fk2.ReferencedTable.Name, Is.EqualTo(fk1.ReferencedTable.Name));
      //referenced columns
      Assert.That(fk2.ReferencedColumns.Count, Is.EqualTo(fk1.ReferencedColumns.Count));
      foreach (var tc1 in fk1.ReferencedColumns) {
        var tc2 = fk2.ReferencedColumns[tc1.Name];
        Assert.That(tc2, Is.Not.Null);
        Assert.That(tc2.Table.Schema.Name, Is.EqualTo(tc1.Table.Schema.Name));
        Assert.That(tc2.Table.Name, Is.EqualTo(tc1.Table.Name));
      }
    }

    private void CompareViews(View v1, View v2)
    {
      Assert.That(v1, Is.Not.Null);
      Assert.That(v2, Is.Not.Null);
      Assert.That(v2.Name, Is.EqualTo(v1.Name));
      Assert.That(v2.Schema.Name, Is.EqualTo(v1.Schema.Name));
      //columns
      //In the created model no columns are created.
      /*
      //Column counts are not equal, created view has 0 columns
      Assert.AreEqual(v1.ViewColumns.Count, v2.ViewColumns.Count);
      foreach(ViewColumn vc1 in v1.ViewColumns)
      {
        ViewColumn vc2 = v2.ViewColumns[vc1.Name];
        Assert.IsNotNull(vc2);
        Assert.AreEqual(vc1.DataTable.Name, vc2.DataTable.Name);
        Assert.AreEqual(vc1.View.Name, vc2.View.Name);
      }
      /**/
    }

    public CatalogComparer(SqlConnection conn)
    {
      connection = conn;
    }
  }
}
