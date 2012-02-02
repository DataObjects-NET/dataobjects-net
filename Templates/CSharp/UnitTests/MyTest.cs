using System.Linq;
using NUnit.Framework;
using Xtensive.Orm;
using $safeprojectname$.Model;

namespace $safeprojectname$
{
	public class MyTest : AutoBuildTest
	{
		[Test]
		public void Test()
		{
			// Creating entity
			using (var session = Domain.OpenSession()) {
				using (var transactionScope = session.OpenTransaction()) {
					var helloWorld = new MyEntity(session) {
						Text = "Hello World!"
					};
					transactionScope.Complete();
				}
			}

			// Checking the result entity
			using (var session = Domain.OpenSession()) {
				using (var transactionScope = session.OpenTransaction()) {
					foreach (var myEntity in session.Query.All<MyEntity>())
						Assert.AreEqual("Hello World!", myEntity.Text);
					Assert.AreEqual(1, session.Query.All<MyEntity>().Count());
					transactionScope.Complete();
				}
			}
		
		// Writing message to log
		Log.Info("Test passed.");
		}
	}
}