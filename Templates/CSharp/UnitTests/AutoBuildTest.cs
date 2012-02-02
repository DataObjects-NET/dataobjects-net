using System.Configuration;
using NUnit.Framework;
using System;
using Xtensive.Core;
using Xtensive.Disposing;
using Xtensive.Reflection;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;

namespace $safeprojectname$
{
	[TestFixture]
	public abstract class AutoBuildTest
	{
		protected Domain Domain { get; private set; }
		
		[TestFixtureSetUp]
		public virtual void TestFixtureSetUp()
		{
			var config = BuildConfiguration();
			Domain = BuildDomain(config);
		}

		[TestFixtureTearDown]
		public virtual void TestFixtureTearDown()
		{
			Domain.DisposeSafely();
		}

		protected virtual DomainConfiguration BuildConfiguration()
		{
			return DomainConfiguration.Load("Default");
		}

		protected virtual Domain BuildDomain(DomainConfiguration configuration)
		{
			try {
				return Domain.Build(configuration);
			}
			catch (Exception e) {
				Log.Error(GetType().GetFullName());
				Log.Error(e);
				throw;
			}
		}
	}
}
