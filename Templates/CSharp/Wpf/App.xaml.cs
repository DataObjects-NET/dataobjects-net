using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;

namespace $safeprojectname$
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static Domain Domain { get; private set; }

		private void App_Startup(object sender, StartupEventArgs e)
		{
			Domain = Domain.Build(DomainConfiguration.Load("Default"));
		}
	}
}
