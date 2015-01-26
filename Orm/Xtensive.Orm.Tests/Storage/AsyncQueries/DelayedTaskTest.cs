#if NET45
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Tests.Storage.AsyncQueries.Model;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries
{
  public class DelayedTaskTest : AsyncQueriesBaseTest
  {
    [Test]
    public async void ConversionToTaskTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var delayedTask = session.Query.ExecuteDelayedAsync(query => query.All<Discepline>());
        Assert.AreEqual(TaskStatus.Created, delayedTask.Status);
        var task = delayedTask.ToTask();
        Assert.AreEqual(TaskStatus.Running, delayedTask.Status);
        await task;
      }
    }

    [Test]
    public void GetResultTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var delayedTask = session.Query.ExecuteDelayedAsync(query => query.All<Discepline>());
        Assert.AreEqual(TaskStatus.Created, delayedTask.Status);
        var result = delayedTask.Result;
        Assert.AreEqual(TaskStatus.RanToCompletion, delayedTask.Status);
      }
    }

    [Test]
    public void RunSynchroniouslyTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var delayedTask = session.Query.ExecuteDelayedAsync(query => query.All<Discepline>());
        Assert.AreEqual(TaskStatus.Created, delayedTask.Status);
        delayedTask.RunSyncroniously();
        Assert.AreEqual(TaskStatus.RanToCompletion, delayedTask.Status);
      }
    }

    [Test]
    public async void StatusTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var delayedTask1 = session.Query.ExecuteDelayedAsync(query => query.All<Discepline>());
        Assert.AreEqual(TaskStatus.Created, delayedTask1.Status);
        await delayedTask1;
        Assert.AreEqual(TaskStatus.RanToCompletion, delayedTask1.Status);
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var delayedTask1 = session.Query.ExecuteDelayedAsync(query => query.All<Discepline>());
        var delayedTask2 = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>());
        Assert.AreEqual(TaskStatus.Created, delayedTask1.Status);
        Assert.AreEqual(TaskStatus.Created, delayedTask2.Status);
        await delayedTask1;
        Assert.AreEqual(TaskStatus.RanToCompletion, delayedTask1.Status);
        Assert.AreEqual(TaskStatus.RanToCompletion, delayedTask2.Status);
      }
    }

    [Test]
    public void WaitTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var delayedTask = session.Query.ExecuteDelayedAsync(query => query.All<Discepline>());
        Assert.AreEqual(TaskStatus.Created, delayedTask.Status);
        delayedTask.Wait();
        Assert.AreEqual(TaskStatus.RanToCompletion, delayedTask.Status);
      }
    }

    [Test]
    public void WaitWithMillisecondsTimeoutTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var delayedTask = session.Query.ExecuteDelayedAsync(query => query.All<Discepline>());
        Assert.AreEqual(TaskStatus.Created, delayedTask.Status);
        delayedTask.Wait(1);
        Assert.AreEqual(TaskStatus.RanToCompletion, delayedTask.Status);
      }
    }

    [Test]
    public void WaitWithTimeoutTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var delayedTask = session.Query.ExecuteDelayedAsync(query => query.All<Discepline>());
        Assert.AreEqual(TaskStatus.Created, delayedTask.Status);
        delayedTask.Wait(TimeSpan.FromMilliseconds(1));
        Assert.AreEqual(TaskStatus.RanToCompletion, delayedTask.Status);
      }
    }

    [Test]
    [ExpectedException(typeof (InvalidOperationException))]
    public async void AwaitAlreadyCompletdedDelayedTaskTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var allCurrentYearDisceplinesTask = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year).Select(d => d.Discepline));
        var allLastYearDisceplinesTask = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year ==DateTime.Now.Year - 1).Select(d => d.Discepline));
        var currentYearTeachersTask = session.Query.ExecuteDelayedAsync(query => query.All<DisceplinesOfCourse>().Where(d => d.Course.Year==DateTime.Now.Year).Select(d => d.Teacher));
        var allCurrentYearDisceplines = await allCurrentYearDisceplinesTask;
        var allLastYearDisceplines = await allLastYearDisceplinesTask;
      }
    }
  }
}
#endif