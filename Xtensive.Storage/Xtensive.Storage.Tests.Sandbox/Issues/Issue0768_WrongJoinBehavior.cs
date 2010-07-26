// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.07.26

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using NUnit.Framework;
using System.Linq;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Issues.Issue0768_WrongJoinBehavior_Model;
using Node = Xtensive.Storage.Tests.Issues.Issue0768_WrongJoinBehavior_Model.Node;

namespace Xtensive.Storage.Tests.Issues
{
  namespace Issue0768_WrongJoinBehavior_Model
  {
    [Serializable]
    [HierarchyRoot]
    public class Node : Entity
    {
      public Node(Guid id)
        : base(id)
      {
      }

      [Field, Key]
      public Guid Id { get; private set; }

      [Field, Version]
      public DateTime Version { get; set; }

      /// <summary>
      /// Gets or sets Name.
      /// </summary>
      [Field]
      public string Name { get; set; }

      [Field(Nullable = false)]
      public Reference Ref { get; set; }

      [Field]
      public Node Parent { get; set; }
    }

    [Serializable]
    [HierarchyRoot]
    public class Reference : Entity
    {
      public Reference(Guid id)
        : base(id)
      {
      }

      [Field, Key]
      public Guid Id { get; private set; }

      [Field, Version]
      public DateTime Version { get; set; }

      /// <summary>
      /// Gets or sets Name.
      /// </summary>
      [Field]
      public string Name { get; set; }
    }

  }

  [Serializable]
  public class Issue0768_WrongJoinBehavior : AutoBuildTest
  {

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof(Node).Assembly, typeof(Node).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var ref1 = new Reference(Guid.NewGuid()) { Name = "q", Version = DateTime.Now };
        var node1 = new Node(Guid.NewGuid()) { Name = "w", Ref = ref1, Version = DateTime.Now };

        var ref2 = new Reference(Guid.NewGuid()) { Name = "p5", Version = DateTime.Now };
        var node2 = new Node(Guid.NewGuid()) { Name = "pp", Ref = ref1, Version = DateTime.Now };

        var ref3 = new Reference(Guid.NewGuid()) { Name = "q2", Version = DateTime.Now };
        var node3 = new Node(Guid.NewGuid()) { Name = "w2", Ref = ref1, Version = DateTime.Now, Parent = node2 };

        t.Complete();
      }
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var list = Query.All<Node>().Select(node => new { Parent = node.Parent.Ref.Name, node.Name }).ToList();
        Assert.AreEqual(3, list.Count());

        // Current wrong way
        /*Join (Inner, #a.Ref.Id == #b.Id)
            Join (LeftOuter, Parent.Id == #a.Id)
              Index (Index 'PK_Node.TYPED' @ Node)
              Alias (#a)
                Index (Index 'PK_Node.TYPED' @ Node)
            Alias (#b)
              Index (Index 'PK_Reference.TYPED' @ Reference)*/
        
        // Easy way ***** IMPLEMENTED *****
        /*Join (LeftOuter, #a.Ref.Id == #b.Id)
            Join (LeftOuter, Parent.Id == #a.Id)
              Index (Index 'PK_Node.TYPED' @ Node)
              Alias (#a)
                Index (Index 'PK_Node.TYPED' @ Node)
            Alias (#b)
              Index (Index 'PK_Reference.TYPED' @ Reference)*/

        // True way
        /*  Join (LeftOuter, Parent.Id == #a.Id)
              Index (Index 'PK_Node.TYPED' @ Node)
              Alias (#a)
                Join (Inner, #a.Ref.Id == #b.Id)
                  Index (Index 'PK_Node.TYPED' @ Node)
                  Alias (#b)
                    Index (Index 'PK_Reference.TYPED' @ Reference)
         */


        t.Complete();
      }

    }
  }
}