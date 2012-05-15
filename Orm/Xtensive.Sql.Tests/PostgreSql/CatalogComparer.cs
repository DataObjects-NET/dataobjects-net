// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using NUnit.Framework;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Tests.PostgreSql
{
  internal class CatalogComparer
  {
    public CatalogComparer(SqlConnection conn)
    {
      connection = conn;
    }

    private readonly SqlConnection connection;


    public void CompareCatalogs(Catalog created, Catalog extracted)
    {
      Assert.AreEqual(created.Name, extracted.Name);
      // Extracted schema count is greater, because it includes the "public" schema
      Assert.AreEqual(created.Schemas.Count, extracted.Schemas.Count - 1);

      foreach (Schema s1 in created.Schemas) {
        Schema s2 = extracted.Schemas[s1.Name];
        Assert.IsNotNull(s2);
        CompareSchemas(s1, s2);
      }
    }

    private void CompareSchemas(Schema s1, Schema s2)
    {
      Assert.IsNotNull(s1);
      Assert.IsNotNull(s2);
      Assert.IsNotNull(s1.Name);
      Assert.IsNotNull(s2.Name);
      Assert.AreEqual(s1.Name, s2.Name);
      //The created model does not assign Owner, it is assigned by PgSql implicitly
      //Assert.IsNotNull(s1.Owner);
      Assert.IsNotNull(s2.Owner);
      
      Assert.AreEqual(s1.Domains.Count, s2.Domains.Count);
      Assert.AreEqual(s1.Sequences.Count, s2.Sequences.Count);
      Assert.AreEqual(s1.Tables.Count, s2.Tables.Count);
      Assert.AreEqual(s1.Views.Count, s2.Views.Count);

      foreach (Domain d1 in s1.Domains) {
        Domain d2 = s2.Domains[d1.Name];
        Assert.IsNotNull(d2);

        CompareDomains(d1, d2);
      }

      foreach (Sequence sq1 in s1.Sequences) {
        Sequence sq2 = s2.Sequences[sq1.Name];
        Assert.IsNotNull(sq2);

        CompareSequences(sq1, sq2);
      }

      foreach (Table t1 in s1.Tables) {
        Table t2 = s2.Tables[t1.Name];
        Assert.IsNotNull(t2);

        CompareTables(t1, t2);
      }

      foreach (View v1 in s1.Views) {
        View v2 = s2.Views[v1.Name];
        Assert.IsNotNull(v2);

        CompareViews(v1, v2);
      }
    }

    private void CompareDomains(Domain d1, Domain d2)
    {
      Assert.IsNotNull(d1.Name);
      Assert.IsNotNull(d2.Name);
      Assert.AreEqual(d1.Name, d2.Name);
      CompareSqlValueTypes(d1.DataType, d2.DataType);
      Assert.AreEqual(d1.Schema.Name, d2.Schema.Name);

      foreach (DomainConstraint dc1 in d1.DomainConstraints) {
        DomainConstraint dc2 = d2.DomainConstraints[dc1.Name];
        Assert.IsNotNull(dc2);
        CompareDomainConstraints(dc1, dc2);
      }
    }

    private void CompareDomainConstraints(DomainConstraint dc1, DomainConstraint dc2)
    {
      Assert.IsNotNull(dc1.Domain);
      Assert.AreEqual(dc1.Domain, dc2.Domain);
      Assert.AreEqual(dc1.IsDeferrable, dc2.IsDeferrable);
      Assert.AreEqual(dc1.IsInitiallyDeferred, dc2.IsInitiallyDeferred);
      Assert.IsNotNull(dc1.Owner);
      Assert.AreEqual(dc1.Owner.Name, dc2.Owner.Name);
    }

    private void CompareSequences(Sequence sq1, Sequence sq2)
    {
      Assert.IsNotNull(sq1);
      Assert.IsNotNull(sq2);
      Assert.IsNotNull(sq2.Name);
      Assert.AreEqual(sq1.Name, sq2.Name);
      Assert.AreEqual(sq1.Schema.Name, sq2.Schema.Name);
      //When creating the model, these values are not specified, but gets default values by PgSql
      //if(sq1.SequenceDescriptor.Increment!=null)
      Assert.AreEqual(sq1.SequenceDescriptor.Increment ?? 1, sq2.SequenceDescriptor.Increment);
      //if(sq1.SequenceDescriptor.IsCyclic != null)
      Assert.AreEqual(sq1.SequenceDescriptor.IsCyclic ?? false, sq2.SequenceDescriptor.IsCyclic);
      if (sq1.SequenceDescriptor.MaxValue!=null)
        Assert.AreEqual(sq1.SequenceDescriptor.MaxValue, sq2.SequenceDescriptor.MaxValue);
      if (sq1.SequenceDescriptor.MinValue!=null)
        Assert.AreEqual(sq1.SequenceDescriptor.MinValue, sq2.SequenceDescriptor.MinValue);
      //start value cannot be extracted
      /*
      if(sq1.SequenceDescriptor.StartValue != null)
        Assert.AreEqual(sq1.SequenceDescriptor.StartValue, sq2.SequenceDescriptor.StartValue);
      /**/
    }

    private void CompareSqlValueTypes(SqlValueType t1, SqlValueType t2)
    {
      Assert.IsNotNull(t1);
      Assert.IsNotNull(t2);
      Assert.AreEqual(t1.Type, t2.Type);
      Assert.AreEqual(t1.Precision, t2.Precision);
      Assert.AreEqual(t1.Scale, t2.Scale);
      Assert.AreEqual(t1.Length, t2.Length);
    }

    private void CompareTables(Table t1, Table t2)
    {
      Assert.IsNotNull(t1);
      Assert.IsNotNull(t2);
      Assert.AreEqual(t1.Filegroup, t2.Filegroup);
      Assert.AreEqual(t1.Schema.Name, t2.Schema.Name);
      Assert.AreEqual(t1.TableColumns.Count, t2.TableColumns.Count);
      Assert.AreEqual(t1.TableConstraints.Count, t2.TableConstraints.Count);

      foreach (TableColumn c1 in t1.TableColumns) {
        TableColumn c2 = t2.TableColumns[c1.Name];
        Assert.IsNotNull(c2);
        CompareTableColumns(c1, c2);
      }

      foreach (Index i1 in t1.Indexes) {
        Index i2 = t2.Indexes[i1.Name];
        Assert.IsNotNull(i2);
        CompareTableIndexes(i1, i2);
      }

      foreach (TableConstraint tc1 in t1.TableConstraints) {
        TableConstraint tc2 = t2.TableConstraints[tc1.Name];
        Assert.IsNotNull(tc2);
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
      Assert.IsNotNull(c1);
      Assert.IsNotNull(c2);
      Assert.AreEqual(c1.DataTable.Name, c2.DataTable.Name);
      CompareSqlValueTypes(c1.DataType, c2.DataType);
      Assert.IsTrue(c1.Domain==null && c2.Domain==null || c1.Domain!=null && c2.Domain!=null && c1.Domain.Name==c2.Domain.Name);
      Assert.AreEqual(c1.IsNullable, c2.IsNullable);
      Assert.AreEqual(c1.Table.Name, c2.Table.Name);
    }

    private void CompareTableIndexes(Index i1, Index i2)
    {
      Assert.IsNotNull(i1);
      Assert.IsNotNull(i2);
      Assert.AreEqual(i1.DataTable.Name, i2.DataTable.Name);
      Assert.AreEqual(i1.Filegroup, i2.Filegroup);

      Version ver = connection.Driver.CoreServerInfo.ServerVersion;
      if (ver.Major * 100 + ver.Minor >= 802) {
        if (i1.FillFactor!=null)
          Assert.AreEqual(i1.FillFactor, i2.FillFactor);
      }
      Assert.AreEqual(i1.IsBitmap, i2.IsBitmap);
      Assert.AreEqual(i1.IsClustered, i2.IsClustered);
      Assert.AreEqual(i1.IsUnique, i2.IsUnique);

      Assert.AreEqual(i1.Columns.Count, i2.Columns.Count);
      foreach (IndexColumn ic1 in i1.Columns) {
        IndexColumn ic2 = i2.Columns[ic1.Name];
        Assert.IsNotNull(ic2);
        CompareIndexColumns(ic1, ic2);
      }
    }

    private void CompareIndexColumns(IndexColumn ic1, IndexColumn ic2)
    {
      Assert.IsNotNull(ic1);
      Assert.IsNotNull(ic2);
      Assert.IsNotNull(ic2.Name);
      Assert.AreEqual(ic1.Name, ic2.Name);
      Assert.AreEqual(ic1.Ascending, ic2.Ascending);
      Assert.AreEqual(ic1.Column.Name, ic2.Column.Name);
      Assert.AreEqual(ic1.Index.Name, ic2.Index.Name);
    }

    private void CompareTableConstraints(TableConstraint tc1, TableConstraint tc2)
    {
      //Assert.AreEqual(tc1.IsDeferrable, tc2.IsDeferrable);
      //Assert.AreEqual(tc1.IsInitiallyDeferred, tc2.IsInitiallyDeferred);
      Assert.AreEqual(tc1.Name, tc2.Name);
      Assert.AreEqual(tc1.Owner.Name, tc2.Owner.Name);
      Assert.AreEqual(tc1.Table.Name, tc2.Table.Name);
    }

    private void CompareCheckConstraints(CheckConstraint cc1, CheckConstraint cc2)
    {
      CompareTableConstraints(cc1, cc2);
    }

    private void CompareUniqueConstraints(UniqueConstraint cc1, UniqueConstraint cc2)
    {
      CompareTableConstraints(cc1, cc2);

      Assert.AreEqual(cc1.Columns.Count, cc2.Columns.Count);
      foreach (TableColumn tc1 in cc1.Columns) {
        TableColumn tc2 = cc2.Columns[tc1.Name];
        Assert.IsNotNull(tc2);
        Assert.AreEqual(tc1.Table.Schema.Name, tc2.Table.Schema.Name);
        Assert.AreEqual(tc1.Table.Name, tc2.Table.Name);
      }
    }

    private void ComparePrimaryKeys(PrimaryKey pk1, PrimaryKey pk2)
    {
      CompareUniqueConstraints(pk1, pk2);
    }

    private void CompareForeignKeys(ForeignKey fk1, ForeignKey fk2)
    {
      CompareTableConstraints(fk1, fk2);
      Assert.AreEqual(fk1.IsDeferrable ?? false, fk2.IsDeferrable);
      Assert.AreEqual(fk1.IsInitiallyDeferred ?? false, fk2.IsInitiallyDeferred);
      Assert.AreEqual(fk1.MatchType, fk2.MatchType);
      Assert.AreEqual(fk1.OnDelete, fk2.OnDelete);
      Assert.AreEqual(fk1.OnUpdate, fk2.OnUpdate);
      Assert.AreEqual(fk1.Table.Schema.Name, fk2.Table.Schema.Name);
      Assert.AreEqual(fk1.Table.Name, fk2.Table.Name);
      //columns
      Assert.AreEqual(fk1.Columns.Count, fk2.Columns.Count);
      foreach (TableColumn tc1 in fk1.Columns) {
        TableColumn tc2 = fk2.Columns[tc1.Name];
        Assert.IsNotNull(tc2);
        Assert.AreEqual(tc1.Table.Schema.Name, tc2.Table.Schema.Name);
        Assert.AreEqual(tc1.Table.Name, tc2.Table.Name);
      }

      Assert.AreEqual(fk1.ReferencedTable.Schema.Name, fk2.ReferencedTable.Schema.Name);
      Assert.AreEqual(fk1.ReferencedTable.Name, fk2.ReferencedTable.Name);
      //referenced columns
      Assert.AreEqual(fk1.ReferencedColumns.Count, fk2.ReferencedColumns.Count);
      foreach (TableColumn tc1 in fk1.ReferencedColumns) {
        TableColumn tc2 = fk2.ReferencedColumns[tc1.Name];
        Assert.IsNotNull(tc2);
        Assert.AreEqual(tc1.Table.Schema.Name, tc2.Table.Schema.Name);
        Assert.AreEqual(tc1.Table.Name, tc2.Table.Name);
      }
    }

    private void CompareViews(View v1, View v2)
    {
      Assert.IsNotNull(v1);
      Assert.IsNotNull(v2);
      Assert.AreEqual(v1.Name, v2.Name);
      Assert.AreEqual(v1.Schema.Name, v2.Schema.Name);
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
  }
}