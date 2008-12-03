// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.25

using System;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Compilation.Expressions.Visitors;
using Xtensive.Storage.Tests.Storage.SnakesModel;

namespace Xtensive.Storage.Tests.Expressions
{
  [TestFixture]
  public class VisitorsTest : AutoBuildTest
  {
    // Column names (they can be different for different storage providers)
    private string cID;
    private string cName;
    private string cLength;

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.SnakesModel");
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      Xtensive.Storage.Domain result = base.BuildDomain(configuration);

      Xtensive.Storage.Model.FieldInfo field;
      field = result.Model.Types[typeof(Creature)].Fields["ID"];
      cID = field.Column.Name;
      field = result.Model.Types[typeof(Creature)].Fields["Name"];
      cName = field.Column.Name;
      field = result.Model.Types[typeof(Snake)].Fields["Length"];
      cLength = field.Column.Name;

      return result;
    }

    [Test]
    public void PreprocessorTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          TypeInfo snakeType = Domain.Model.Types[typeof (Snake)];
          RecordSet rsSnakePrimary = snakeType.Indexes.GetIndex("ID").ToRecordSet();
          string name = "90";
          int len = 10;
          var pLen = new Parameter<int>();
          Expression<Func<Tuple, bool>> a = tuple => tuple.GetValue<string>(rsSnakePrimary.Header.IndexOf(cName)).Contains(name);
          Expression<Func<Tuple, bool>> b = tuple => tuple.GetValue<string>(rsSnakePrimary.Header.IndexOf(cName)).EndsWith(name);
          Expression<Func<Tuple, bool>> c = tuple => tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cLength)) > 10;
          Expression<Func<Tuple, bool>> d = tuple => tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cLength)) * 2 * tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cID)) > len * 3 * pLen.Value;
          Expression<Func<Tuple, bool>> e = tuple => tuple.GetValue<int>(rsSnakePrimary.Header.IndexOf(cLength)) > pLen.Value;
          Expression<Func<Tuple, bool>> f = tuple => tuple.GetValue<string>(rsSnakePrimary.Header.IndexOf(cName)).Length > pLen.Value;

          Console.Out.WriteLine("Original");
          Console.Out.WriteLine(ExpressionWriter.WriteToString(a));
          Console.Out.WriteLine("Processed");
          Console.Out.WriteLine(ExpressionWriter.WriteToString(QueryPreprocessor.Translate(a, Domain.Model)));
          Console.Out.WriteLine();
          Console.Out.WriteLine("Original");
          Console.Out.WriteLine(ExpressionWriter.WriteToString(b));
          Console.Out.WriteLine("Processed");
          Console.Out.WriteLine(ExpressionWriter.WriteToString(QueryPreprocessor.Translate(b, Domain.Model)));
          Console.Out.WriteLine();
          Console.Out.WriteLine("Original");
          Console.Out.WriteLine(ExpressionWriter.WriteToString(c));
          Console.Out.WriteLine("Processed");
          Console.Out.WriteLine(ExpressionWriter.WriteToString(QueryPreprocessor.Translate(c, Domain.Model)));
          Console.Out.WriteLine();
          Console.Out.WriteLine("Original");
          Console.Out.WriteLine(ExpressionWriter.WriteToString(d));
          Console.Out.WriteLine("Processed");
          Console.Out.WriteLine(ExpressionWriter.WriteToString(QueryPreprocessor.Translate(d, Domain.Model)));
          Console.Out.WriteLine();
          Console.Out.WriteLine("Original");
          Console.Out.WriteLine(ExpressionWriter.WriteToString(e));
          Console.Out.WriteLine("Processed");
          Console.Out.WriteLine(ExpressionWriter.WriteToString(QueryPreprocessor.Translate(e, Domain.Model)));
          Console.Out.WriteLine();
          Console.Out.WriteLine("Original");
          Console.Out.WriteLine(ExpressionWriter.WriteToString(f));
          Console.Out.WriteLine("Processed");
          Console.Out.WriteLine(ExpressionWriter.WriteToString(QueryPreprocessor.Translate(f, Domain.Model)));
        }
      }
    }

  }
}