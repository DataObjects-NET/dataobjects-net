// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.09.06

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue_TypeCastInContain_Model;

namespace Xtensive.Storage.Tests.Issues.Issue_TypeCastInContain_Model
{
    [HierarchyRoot]
    public class BaseClass : Entity
    {
        public BaseClass(Session session)
            : base(session)
        {
        }

        [Field, Key]
        public int Id { get; private set; }
    }

    class Children : BaseClass
    {
        public Children(Session session)
            : base(session)
        {
        }

        [Field]
        public bool SomeBool { get; set; }
    }
}

namespace Xtensive.Storage.Tests.Issues
{
    public class Issue_TypeCastInContain : AutoBuildTest
    {
        protected override DomainConfiguration BuildConfiguration()
        {
            var config = base.BuildConfiguration();
            config.Types.Register(typeof(BaseClass).Assembly, typeof(BaseClass).Namespace);
            return config;
        }

        [Test]
        public void MainTest()
        {
            using (var session = Session.Open(Domain))
            {
                using (var transactionScope = Transaction.Open())
                {
                    var t = Query.All<BaseClass>().ToArray();
                    var k = Query.All<Children>().Where(a => t.Contains(a as BaseClass)).ToArray();
                    var m = Query.All<Children>().Where(a => t.Contains(a)).ToArray();
                }
            }
        }
    }
}