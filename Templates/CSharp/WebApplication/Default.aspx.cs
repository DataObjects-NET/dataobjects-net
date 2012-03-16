using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Xtensive.Orm;
using $safeprojectname$.Model;

namespace $safeprojectname$
{
	public partial class _Default : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			var session = Xtensive.Orm.Session.Current;

			var myEntity = session.Query.All<MyEntity>().SingleOrDefault();
			if (myEntity == null) {
				myEntity = new MyEntity(session) {
					Text = "Hello World!"
					};
			}
			Label1.Text = myEntity.Text;
		}
	}
}
