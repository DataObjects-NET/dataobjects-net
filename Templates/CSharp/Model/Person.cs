using System;
using Xtensive.Orm;

namespace $safeprojectname$
{
	[HierarchyRoot]
	public class Person : Entity
	{
		[Field, Key]
		public int Id { get; private set; }

		[Field]
		public string FirstName { get; set; }

		[Field]
		public string LastName { get; set; }

		[Field]
		public int Age { get; set; }


		public Person()
			: base()
		{
		}

		public Person(Session session)
			: base(session)
		{
		}
	}
}
