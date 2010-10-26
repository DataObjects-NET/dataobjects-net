using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues_Issue0806_ColumnNamingError;

namespace Xtensive.Orm.Tests.Issues_Issue0806_ColumnNamingError
{
  [HierarchyRoot]
  public class Kalina2RngGasObjectMatchGuess : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public int KalinaPipeAbonentID { get; set; }

    [Field]
    public int RngGasObjectID { get; set; }

    [Field]
    public int? Rank { get; set; }
  }

  [HierarchyRoot]
  public class PipeAbonent : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }

  [HierarchyRoot]
  public class GasObject : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [HierarchyRoot]
  public class PipeAbonentOwnershipInfo : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public DateTime? BeginDate { get; set; }

    [Field]
    public DateTime? EndDate { get; set; }

    [Field]
    public PipeAbonent Abonent { get; set; }

    [Field]
    public string Patronymic { get; set; }

    [Field]
    public string Surname { get; set; }
  }

  [HierarchyRoot]
  public class UnifiedGasObject : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public PipeAbonent KalinaGasObject { get; set; }

    [Field]
    public GasObject RngGasObject { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{

  public enum MatchStatus
  {
    NotConfirmed,
    Confirmed
  }

  public class Kalina2RngGasObjectMatchListItem
  {
    public int LeftID { get; set; }

    public string LeftName { get; set; }

    public object LeftPatronymic { get; set; }

    public object LeftSurname { get; set; }

    public MatchStatus Status { get; set; }

    public PipeAbonent Right { get; set; }
  }

  [TestFixture]
  public class Issue0806_ColumnNamingError : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Kalina2RngGasObjectMatchGuess).Assembly, typeof (Kalina2RngGasObjectMatchGuess).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      var today = DateTime.Now;
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query =
            from kalinaGasObject in session.Query.All<PipeAbonent>()
            let matchGuess = session.Query.All<Kalina2RngGasObjectMatchGuess>().Where(g => g.KalinaPipeAbonentID==kalinaGasObject.Id).FirstOrDefault()
            let matchGuessObject = session.Query.All<GasObject>().Where(go => go.Id==matchGuess.RngGasObjectID).FirstOrDefault()
            let owner = session.Query.All<PipeAbonentOwnershipInfo>().Where(
              ao =>
                ao.Abonent==kalinaGasObject &&
                  (ao.BeginDate==null || ao.BeginDate <= today) &&
                    (ao.EndDate==null || ao.EndDate >= today)).FirstOrDefault()
            let unifiedGasObject = session.Query.All<UnifiedGasObject>().Where(
              ua => ua.KalinaGasObject==kalinaGasObject && ua.RngGasObject!=null).FirstOrDefault()
            orderby matchGuess.Rank ?? 0 descending
            select new Kalina2RngGasObjectMatchListItem {
              LeftID = kalinaGasObject.Id,
              LeftName = owner.Name ?? kalinaGasObject.Name,
              LeftPatronymic = owner.Patronymic,
              LeftSurname = owner.Surname,
              Status = unifiedGasObject==null ? MatchStatus.NotConfirmed : MatchStatus.Confirmed,
              Right = unifiedGasObject==null
                ? GetMatch(matchGuessObject)
                : GetMatch(unifiedGasObject.RngGasObject)
            };

          var result = query.Count();
        }
      }
    }

    [Test]
    public void SimplifiedTest()
    {
      var today = DateTime.Now;
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query =
            from kalinaGasObject in session.Query.All<PipeAbonent>()
            let matchGuess = session.Query.All<Kalina2RngGasObjectMatchGuess>().FirstOrDefault()
            let owner = session.Query.All<PipeAbonentOwnershipInfo>().FirstOrDefault()
            orderby matchGuess.Rank ?? 0 descending
            select matchGuess;

          var result = query.ToList();
        }
      }
    }

    private PipeAbonent GetMatch(GasObject rngGasObject)
    {
      return null;
    }
  }
}