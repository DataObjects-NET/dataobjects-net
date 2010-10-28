// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.11

using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using System.Linq;

namespace Xtensive.Orm.Tests.Issues.KeyFormatBug
{
  [Serializable]
  [HierarchyRoot]
  public class Base : Entity
  {
    [Field, Key]
    public long Id { get; private set; }
  }

  [Serializable]
  public class Child : Base
  {
  }

  namespace Nested
  {
    [Serializable]
    [HierarchyRoot]
    public class Base : Entity
    {
      [Field, Key]
      public long Id { get; private set; }
    }

    [Serializable]
    public class Child : Base
    {
    }
  }

  [TestFixture]
  public class KeyFormatBugTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Base).Assembly, typeof (Base).Namespace);
      var nestedBaseType = typeof (Nested.Base);
      config.Types.Register(nestedBaseType.Assembly, nestedBaseType.Namespace);
      config.NamingConvention.NamespacePolicy = NamespacePolicy.Synonymize;

      var persistentType = typeof(Persistent);
      var namespaces = (
        from type in config.Types
        where persistentType.IsAssignableFrom(type)
        select type.Namespace
        ).Distinct().ToArray();
      var prefixes = new[] {
          "Xtensive.Storage",
          "Xtensive.Storage.Metadata",
          GetType().Namespace,
        };
      var synonyms =
        from ns in namespaces
        from prefix in prefixes
        where ns.StartsWith(prefix)
        let tail = ns.Substring(prefix.Length)
        let synonym = tail.StartsWith(".") ? tail.Substring(1) : tail
        group synonym by ns into g
        let shortestSynonymLength = g.Min(s => s.Length)
        let shortestSynonym = g.First(s => s.Length==shortestSynonymLength)
        select new KeyValuePair<string, string>(g.Key, shortestSynonym);
      foreach (var synonym in synonyms)
        config.NamingConvention.NamespaceSynonyms.Add(synonym);
      return config;
    }

    [Test]
    public void CombinedTest()
    {
      using (var session = Domain.OpenSession()) 
      using(var tx = session.OpenTransaction()) {

        var entity = new Child();
        var key = entity.Key;

        string formattedKey = key.Format();
        Log.Info("Key.ToString() result: {0}", key.ToString());
        Log.Info("Key.Format()   result: {0}", formattedKey);
        Assert.IsFalse(formattedKey.Contains("Child"));
        Assert.IsTrue(formattedKey.Contains("Base"));
        
        var parsedKey = Key.Parse(formattedKey);
        Assert.AreEqual(key, parsedKey);
      }
    }
  }
}