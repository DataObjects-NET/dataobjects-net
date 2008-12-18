using System;
using Xtensive.Storage;
using Xtensive.Storage.Attributes;

namespace $safeprojectname$
{
  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Person : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    public int Age { get; set; }
  }
}
