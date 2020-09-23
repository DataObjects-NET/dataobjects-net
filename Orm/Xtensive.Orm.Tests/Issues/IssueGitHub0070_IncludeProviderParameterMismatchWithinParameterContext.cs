using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.GithubIssue_xxx_Model;

namespace Xtensive.Orm.Tests.Issues.GithubIssue_xxx_Model
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
  public class IssueGitHub0070_IncludeProviderParameterMismatchWithinParameterContext : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Token).Assembly, typeof(Token).Namespace);
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

    [Test]
    public void TwoSimilarInStatementsInTheSameQuery()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var tokenId = Guid.NewGuid();
        var token = new Token(session, tokenId) {
          Salt = -1
        };
        session.SaveChanges();
        var query = session.Query.All<Token>()
          .Where(t => t.Salt.In(-1, 1) || t.Salt.In(4, 5));
        Assert.AreSame(token, query.FirstOrDefault());
      }
    }
  }
}