// Copyright (C) 2020-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Issues.IssueGitHub0132_IncorrectInsertForSingleTableHierarchyModel;

namespace Xtensive.Orm.Tests.Issues.IssueGitHub0132_IncorrectInsertForSingleTableHierarchyModel
{
  [HierarchyRoot(InheritanceSchema.SingleTable)]
  public class A : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    public A(Session session)
      : base(session)
    {
    }
  }

  public class B1 : A
  {
    [Field]
    public EntitySet<C> ConflictingField { get; set; }

    public B1(Session session)
      : base(session)
    {
    }
  }

  public class B2 : A
  {
    [Field]
    public double ConflictingField { get; set; }

    public B2(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class C : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    [Association(PairTo = nameof(B1.ConflictingField))]
    public B1 Ref { get; set; }

    public C(Session session)
      : base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueGitHub0132_IncorrectInsertForSingleTableHierarchy : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(A).Assembly, typeof(A).Namespace);
      return config;
    }

    [Test]
    public void WriteToWrongColumnTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var aId = new A(session) { Name = "AName" }.Id;
        var b = new B1(session) { Name = "B1Name" };
        _ = b.ConflictingField.Add(new C(session));
        var b1Id = b.Id;
        var b2Id = new B2(session) { Name = "B2Name", ConflictingField = 5.0 }.Id;
        Assert.DoesNotThrow(session.SaveChanges);
        var accessor = session.Services.Get<DirectSqlAccessor>();
        using (var command = accessor.CreateCommand()) {
          command.CommandText = GetCommandText(session);
          using (var reader = command.ExecuteReader()) {
            while (reader.Read()) {
              var id = reader.GetInt32(0);
              var conflictingField = reader.GetDouble(1);
              if (id != b2Id) {
                Assert.That(conflictingField, Is.EqualTo(0.0));
              }
              else {
                Assert.That(conflictingField, Is.EqualTo(5.0));
              }
            }
          }
        }
      }
    }

    private string GetCommandText(Session session)
    {
      var typeInfo = Domain.Model.Types[typeof(A)];
      var table = session.StorageNode.Mapping[typeInfo];

      var tableRef = Sql.SqlDml.TableRef(table);
      var select = Sql.SqlDml.Select(tableRef);
      select.Columns.Add(tableRef[nameof(A.Id)]);
      select.Columns.Add(tableRef[nameof(B2.ConflictingField)]);

      var queryBuilder = session.Services.Get<QueryBuilder>();

      var compileResult = queryBuilder.CompileQuery(select);
      return compileResult.GetCommandText();
    }
  }
}
