// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.28

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  [Explicit("Requires PostgreSQL and MSSQL servers")]
  public class AggregateTest : CrossStorageTest
  {
    protected override DomainConfiguration BuildConfiguration(string provider)
    {
      var configuration = base.BuildConfiguration(provider);
      configuration.Types.Register(typeof (X).Assembly, typeof (X).Namespace);
      return configuration;
    }

    protected override void FillData(Domain domain)
    {
      for (int i = 0; i < 10; i++) {
        var x = new X();
        x.FByte = (byte) i;
        x.FSByte = (sbyte) i;
        x.FShort = (short) i;
        x.FUShort = (ushort) i;
        x.FInt = i;
        x.FUInt = (uint) i;
        x.FLong = i;
        x.FULong = (ulong) i;
        x.FDecimal = i;
        x.FFloat = i;
        x.FDouble = i;
        x.FDateTime = new DateTime(2009, 1, i+1);
        x.FTimeSpan = new TimeSpan(i, 0, 0, 0);
        x.FString = i.ToString();
      }
    }

    [Test]
    public void MinTest()
    {
      RunTest(MinTest);
    }

    [Test]
    public void MaxTest()
    {
      RunTest(MaxTest);
    }

    [Test]
    public void SumTest()
    {
      RunTest(SumTest);
    }

    [Test]
    public void AvgTest()
    {
      RunTest(AvgTest);
    }

    #region Field names

    private string[] allFields = new string[]
      {
        "FByte",
        "FSByte",
        "FShort",
        "FUShort",
        "FInt",
        "FUInt",
        "FLong",
        "FULong",
        "FDecimal",
        "FFloat",
        "FDouble",
        "FDateTime",
        "FTimeSpan",
        "FString",
      };

    private string[] numericFields = new string[]
      {
        "FByte",
        "FSByte",
        "FShort",
        "FUShort",
        "FInt",
        "FUInt",
        "FLong",
        "FULong",
        "FDecimal",
        "FFloat",
        "FDouble",
      };

    #endregion

    private RecordSet MinTest(Domain domain)
    {
      var descriptors = GetFields(domain, allFields)
        .Select(f => new AggregateColumnDescriptor(f.First + "_Min", f.Second, AggregateType.Min));
      return GetOriginRecordSet(domain).Aggregate(null, descriptors.ToArray());
    }

    private RecordSet MaxTest(Domain domain)
    {
      var descriptors = GetFields(domain, allFields)
        .Select(f => new AggregateColumnDescriptor(f.First + "_Max", f.Second, AggregateType.Max));
      return GetOriginRecordSet(domain).Aggregate(null, descriptors.ToArray());
    }

    private RecordSet SumTest(Domain domain)
    {
      var descriptors = GetFields(domain, numericFields)
        .Select(f => new AggregateColumnDescriptor(f.First + "_Sum", f.Second, AggregateType.Sum));
      return GetOriginRecordSet(domain).Aggregate(null, descriptors.ToArray());
    }

    private RecordSet AvgTest(Domain domain)
    {
      var descriptors = GetFields(domain, numericFields)
        .Select(f => new AggregateColumnDescriptor(f.First + "_Avg", f.Second, AggregateType.Avg));
      return GetOriginRecordSet(domain).Aggregate(null, descriptors.ToArray());
    }

    #region Helper methods

    private static RecordSet GetOriginRecordSet(Domain domain)
    {
      return domain.Model.Types[typeof(X)].Indexes.PrimaryIndex.ToRecordSet();
    }

    private static Pair<string, int>[] GetFields(Domain domain, IEnumerable<string> fields)
    {
      var typeInfo = domain.Model.Types[typeof(X)];
      var origin = typeInfo.Indexes.PrimaryIndex.ToRecordSet();
      return fields.Select(f => new Pair<string, int>(f, origin.Header.IndexOf(typeInfo.Fields[f].Column.Name))).ToArray();
    }

    #endregion
  }
}