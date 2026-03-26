// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueGitHub0070_IncludeProviderParameterConflictWithinParameterContextModel;

namespace Xtensive.Orm.Tests.Issues.IssueGitHub0070_IncludeProviderParameterConflictWithinParameterContextModel
{
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class Token: Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }

    [Field]
    public long Salt { get; set; }

    public Token(Session session, Guid id)
      : base(session, id)
    { }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueGitHub0070_IncludeProviderParameterConflictWithinParameterContext : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.RegisterCaching(typeof(Token).Assembly, typeof(Token).Namespace);
      return config;
    }

    [Test]
    public void TwoDifferentInStatementsInTheSameQuery()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var tokenId = Guid.NewGuid();
        var token = new Token(session, tokenId) {
          Salt = -1
        };
        session.SaveChanges();
        var query = session.Query.All<Token>()
          .Where(t => t.Salt.In(-1, 1) && t.Id.In(tokenId, Guid.Empty));
        Assert.AreSame(token, query.FirstOrDefault());
      }
    }
  }
}