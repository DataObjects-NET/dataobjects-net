using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;
using Xtensive.Orm.Tracing;

namespace Xtensive.Orm.Tests.Linq
{
  public class TraceTest : ChinookDOModelTest
  {
    private IQueryable<Customer> CreateQuery1() => Session.Query.All<Customer>().Trace();

    private IQueryable<Customer> CreateQuery2() => Session.Query.All<Customer>().Trace();

    [Test]
    public void TraceMethodDataIsUsedInDbCommandExecutingEvent()
    {
      TraceInfo traceInfo = null;
      Session.Events.DbCommandExecuting += (sender, args) => {
        traceInfo = args.TraceInfo;
      };

      var subquery = CreateQuery1();
      var query = CreateQuery2()
        .Where(c => c==Session.Query.Single<Customer>(subquery.FirstOrDefault().Key));

      Assert.IsNotEmpty(query.ToArray());
      Assert.AreEqual(nameof(CreateQuery2), traceInfo.CallerMemberName);
      Assert.IsNotEmpty(traceInfo.CallerFilePath);
      Assert.True(traceInfo.CallerLineNumber > 0);
    }
  }
}