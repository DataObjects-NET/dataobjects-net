using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  public class TraceTest : ChinookDOModelTest
  {
    private IQueryable<Customer> CreateQuery1() => Session.Query.All<Customer>().Trace();

    private IQueryable<Customer> CreateQuery2() => Session.Query.All<Customer>().Trace();

    [Test]
    public void TraceMethodDataIsUsedInDbCommandExecutingEvent()
    {
      TraceData traceData = null;
      Session.Events.DbCommandExecuting += (sender, args) => {
        traceData = args.TraceData;
      };

      var subquery = CreateQuery1();
      var query = CreateQuery2()
        .Where(c => c==Session.Query.Single<Customer>(subquery.FirstOrDefault().Key));

      Assert.IsNotEmpty(query.ToArray());
      Assert.AreEqual(nameof(CreateQuery2), traceData.CallerMemberName);
      Assert.IsNotEmpty(traceData.CallerFilePath);
      Assert.True(traceData.CallerLineNumber > 0);
    }
  }
}