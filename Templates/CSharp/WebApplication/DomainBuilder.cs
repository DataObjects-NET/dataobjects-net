using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using $safeprojectname$.Model;

namespace $safeprojectname$
{
	internal static class DomainBuilder
	{
		private static Domain domain;

		public static Domain Build()
		{
			var config = DomainConfiguration.Load("Default");
			domain = Domain.Build(config);
			using (var session = domain.OpenSession())
				PopulateData(session, true);
			return domain;
		}

		public static void PopulateData(Session session, bool checkIfPopulated)
		{
			using (var transactionScope = session.OpenTransaction()) {
				using (session.DisableValidation()) {
					if (checkIfPopulated)
					{
						// See ASP.NET samples for examples 
						// of implementing such a logic.
					}
					new MyEntity(session) {
						Text = "Hello World!"
					};
				}
				transactionScope.Complete();
			}
		}
	}
}