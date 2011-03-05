// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.15

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0402_WrongEntitySetQuery_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0402_WrongEntitySetQuery_Model
{
  [Serializable]
  [HierarchyRoot]
  public class Node : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public Node Parent { get; set;}

    [Field]
    [Association(PairTo = "Parent")]
    public EntitySet<Node> Children { get; private set; }

    public Node(string name)
    {
      Name = name;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0402_WrongEntitySetQuery : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Node).Assembly, typeof (Node).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      Key key;
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var root1 = new Node("1");
          key = root1.Key;
          var child11 = new Node("11");
          var child12 = new Node("12");
          root1.Children.Add(child11);
          root1.Children.Add(child12);
          child11.Children.Add(new Node("111"));
          child11.Children.Add(new Node("112"));
          child12.Children.Add(new Node("121"));
          child12.Children.Add(new Node("122"));

          var root2 = new Node("2");
          var child21 = new Node("21");
          var child22 = new Node("22");
          root2.Children.Add(child21);
          root2.Children.Add(child22);
          child21.Children.Add(new Node("211"));
          child21.Children.Add(new Node("212"));
          child22.Children.Add(new Node("221"));
          child22.Children.Add(new Node("222"));

          t.Complete();
        }
      }
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var root1 = session.Query.Single<Node>(key);
          Console.WriteLine("Direct query");
          var directQuery = session.Query
            .All<Node>()
            .Where(node => root1.Children.Contains(node.Parent));
          var enumerable = session.Query
            .All<Node>()
            .AsEnumerable();
          var directQueryExpected = enumerable
            .Where(node => root1.Children.AsEnumerable().Contains(node.Parent));
          foreach (var node in directQuery)
            Console.WriteLine(node.Name);
          Assert.AreEqual(0, directQueryExpected.Except(directQuery).Count());


          Console.WriteLine("Query through EntitySet");
          var entitySet = root1.Children;
          var entitySetQuery = session.Query
            .All<Node>()
            .Where(node => entitySet.Contains(node.Parent));
          var entitySetQueryExpected = session.Query
            .All<Node>()
            .AsEnumerable()
            .Where(node => entitySet.AsEnumerable().Contains(node.Parent));
          foreach (var node in entitySetQuery)
            Console.WriteLine(node.Name);
          Assert.AreEqual(0, entitySetQueryExpected.Except(entitySetQuery).Count());
        }
      }
    }
  }
}