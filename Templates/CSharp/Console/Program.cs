using System;
using System.Linq;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using $safeprojectname$.Model;

namespace $safeprojectname$
{
	class Program
	{
		static void Main(string[] args)
		{
			// Loading configuration section for local SQL Server database. 
			// See other cases in App.config file.
			var config = DomainConfiguration.Load("Default");
			var domain = Domain.Build(config);

			using (var session = domain.OpenSession()) {
				using (var transactionScope = session.OpenTransaction()) {
					// Creating new persistent object
					var helloWorld = new MyEntity(session) {
						Text = "Hello World!"
					};
					// Committing transaction
					transactionScope.Complete();
				}
			}

			// Reading all persisted objects from another Session
			using (var session = domain.OpenSession()) {
				using (var transactionScope = session.OpenTransaction()) {
					foreach (var myEntity in session.Query.All<MyEntity>())
						Console.WriteLine(myEntity.Text);
					transactionScope.Complete();
				}
			}
			Console.ReadKey();
		}
	}
}
