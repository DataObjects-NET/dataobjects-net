using Xtensive.Storage;
using Xtensive.Storage.Configuration;

namespace $safeprojectname$
{
	internal static class DomainBuilder
	{
		private static Domain domain;

		public static Domain Build()
		{
			var config = DomainConfiguration.Load("mssql");
			// config.Types.Register(typeof (MyEntity).Assembly);
			domain = Domain.Build(config);
			// using (Session.Open(domain))
			//	 PopulateData(true);
			return domain;
		}

		public static void PopulateData(bool checkIfPopulated)
		{
			using (var transactionScope = Transaction.Open()) {
				using (InconsistentRegion.Open()) {
					if (checkIfPopulated) {
						// See Xtensive.Storage.Samples.AspNet for
						// example of implementing such a logic.
					}
					// Populate your data here
				}
				transactionScope.Complete();
			}
		}
	}
}