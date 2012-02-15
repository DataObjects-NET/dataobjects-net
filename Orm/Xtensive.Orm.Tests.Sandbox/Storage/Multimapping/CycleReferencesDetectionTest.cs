// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.14

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Orm.Tests.Storage.Multimapping.CycleReferencesDetectionModel.Namespace1;
using Xtensive.Orm.Tests.Storage.Multimapping.CycleReferencesDetectionModel.Namespace2;
using Xtensive.Orm.Tests.Storage.Multimapping.CycleReferencesDetectionModel.Namespace3;
using Xtensive.Orm.Tests.Storage.Multimapping.CycleReferencesDetectionModel.Namespace4;
using Xtensive.Testing;

namespace Xtensive.Orm.Tests.Storage.Multimapping
{
  namespace CycleReferencesDetectionModel
  {
    namespace Namespace1
    {
      [HierarchyRoot]
      public class Type1 : Entity
      {
        [Key, Field]
        public int Id { get; private set; }

        [Field]
        public Type2 T2 { get; set; }

        [Field]
        public Type4 T4 { get; set; }
      }
    }

    namespace Namespace2
    {
      [HierarchyRoot]
      public class Type2 : Entity
      {
        [Key, Field]
        public int Id { get; private set; }

        [Field]
        public Type3 T3 { get; set; }
      }
    }

    namespace Namespace3
    {
      [HierarchyRoot]
      public class Type3 : Entity
      {
        [Key, Field]
        public int Id { get; private set; }
      }
    }

    namespace Namespace4
    {
      [HierarchyRoot]
      public class Type4 : Entity
      {
        [Key, Field]
        public int Id { get; private set; }

        [Field]
        public Type2 T2 { get; set; }
      }
    }
  }

  [TestFixture]
  public class CycleReferencesDetectionTest : MultidatabaseTest
  {
    private const string Database1Name = "database1";
    private const string Database2Name = "database2";
    private const string Database3Name = "database3";
    private const string Database4Name = "database4";

    protected Dictionary<int, string> DatabaseNames = new Dictionary<int, string> {
      {1, Database1Name},
      {2, Database2Name},
      {3, Database3Name},
      {4, Database4Name},
    };

    private void BuildDomain(params int?[] databaseIndexes)
    {
      var configuration = BuildConfiguration();

      var types = new[] {typeof (Type1), typeof (Type2), typeof (Type3), typeof (Type4)};
      var names = new string[types.Length];

      if (types.Length!=databaseIndexes.Length)
        throw new InvalidOperationException();

      for (int i = 0; i < types.Length; i++) {
        var index = databaseIndexes[i];
        if (index==null)
          continue;
        var name = DatabaseNames[index.Value];
        configuration.Types.Register(types[i]);
        configuration.MappingRules.Map(types[i].Namespace).ToDatabase(name);
      }

      var domain = Domain.Build(configuration);

      Assert.That(domain.Model.IsMultidatabase);

      for (int i = 0; i < types.Length; i++) {
        var expected = names[i];
        if (expected==null)
          continue;
        var actual = domain.Model.Types[types[i]].MappingDatabase;
        Assert.That(actual, Is.EqualTo(expected));
      }

      domain.Dispose();
    }

    [Test]
    public void NoCycles1Test()
    {
      BuildDomain(1, 1, 1, 1);
    }

    [Test]
    public void NoCycles2Test()
    {
      BuildDomain(1, 2, 3, 4);
    }

    [Test]
    public void NoCycles3Test()
    {
      BuildDomain(1, 1, 2, 1);
    }

    [Test]
    public void NoCycles4Test()
    {
      BuildDomain(1, 2, 2, 2);
    }

    [Test]
    public void NoCycles5Test()
    {
      BuildDomain(1, 2, 2, 1);
    }

    [Test]
    public void Cycle1Test()
    {
      AssertEx.Throws<DomainBuilderException>(() => BuildDomain(1, 1, 2, 2));
    }

    [Test]
    public void Cycle2Test()
    {
      AssertEx.Throws<DomainBuilderException>(() => BuildDomain(1, 1, 2, 2));
    }
  }
}