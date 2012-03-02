using System;
using Xtensive.Orm;

namespace $safeprojectname$.Model
{
	[HierarchyRoot]
	public class MyEntity : Entity
	{
		[Field, Key]
		public int Id { get; private set; }

		[Field(Length = 100)]
		public string Text { get; set; }


		public MyEntity()
			: base()
		{
		}

		public MyEntity(Session session)
			: base(session)
		{
		}
	}
}
