=====================
Xtensive.Orm.Security
=====================

Summary
-------
The extension provides full-fledged security layer (authentication services, principals, roles, secured queries)
There are 2 main parts that can also be used separately: authentication services and role-based access to domain entities

Prerequisites
-------------
DataObjects.Net 0.1 or later (http://dataobjects.net)

Implementation
--------------
1. Add reference to Xtensive.Orm.Security assembly
2. Include types from Xtensive.Orm.Security assembly into the domain:

  <Xtensive.Orm>
    <domains>
      <domain ... >
        <types>
          <add assembly="your assembly"/>
          <add assembly="Xtensive.Orm.Security"/>
        </types>
      </domain>
    </domains>
  </Xtensive.Orm>

3. If you are planning to use one of authentication services add 

  <section name="Xtensive.Orm.Security" type="Xtensive.Orm.Security.Configuration.ConfigurationSection, Xtensive.Orm.Security" />

and set up the desired hashing service:

  <Xtensive.Orm.Security>
    <hashingService name="plain"/>
    <!-- other options are: md5, sha1, sha256, sha384, sha512 -->
  </Xtensive.Orm.Security>

Demo
----

1. Define a class that inherits GenericPrincipal class that will describe your users, e.g.:

  [HierarchyRoot]
  public class User : GenericPrincipal
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    public string FirstName { get; set; }

    ...

    public User(Session session) : base(session) {}
  }

2. Having the User class defined, it can be used for user creation and authentication.

  // Creating a user
  using (var session = Domain.OpenSession()) {
    using (var transaction = session.OpenTransaction()) {
      var user = new User(session);
      user.Name = "admin";
      user.SetPassword("password");
      transaction.Complete();
    }
  }

  // Authenticating a user
  using (var session = Domain.OpenSession()) {
    using (var transaction = session.OpenTransaction()) {
      var user = session.Authenticate("admin", "password");
      transaction.Complete();
    }
  }

3. Define a hierarchy of roles. A role is a set of permissions for a job fuction within a company, e.g.:

EmployeeRole
|
|- StockManagerRole
|
|- SalesRepresentativeRole
     |
     |- SalesManagerRole
     |
     |- SalesPresidentRole

  // This is base role for all employees
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.SingleTable)]
  public abstract class EmployeeRole : Role
  {
    [Field, Key]
    public int Id { get; set; }

    protected override void RegisterPermissions()
    {
      // All employees can read products
      RegisterPermission(new Permission<Product>());
      // All employees can read other employees
      RegisterPermission(new Permission<Employee>());
    }

    protected EmployeeRole(Session session)
      : base(session) {}
  }

  public class StockManagerRole : EmployeeRole
  {
    protected override void RegisterPermissions()
    {
      // Stock manager inherits Employee permissions
      base.RegisterPermissions();

      // Stock manager can read and write products
      RegisterPermission(new Permission<Product>(canWrite:true));
    }

    public StockManagerRole(Session session)
      : base(session) {}
  }

  // Create instances of roles on first domain initialization
  using (var session = Domain.OpenSession()) {
    using (var transaction = session.OpenTransaction()) {
      new SalesRepresentativeRole(session);
      new SalesManagerRole(session);
      new SalesPresidentRole(session);
      new StockManagerRole(session);
      transaction.Complete();
    }
  }

4. Members of staff are assigned particular roles, e.g.:

  using (var session = Domain.OpenSession()) {
    using (var transaction = session.OpenTransaction()) {
      var stockManagerRole = session.Query.All<StockManagerRole>().Single();
      var user = new User(session);
      user.Name = "peter";
      user.SetPassword("password");
      user.Roles.Add(stockManagerRole);
      transaction.Complete();
    }
  }

5. Checking whether a user has the required role

  user.IsInRole("StockManagerRole");
  // or
  user.Roles.Contains(stockManagerRole);

6. Session impersonation

  using (var imContext = session.Impersonate(user)) {
    // inside the region the session is impersonated with the specified 
    // principal and set of their roles and permissions

    // Checking whether the user has a permission for reading Customer entities
    imContext.Permissions.Contains<Permission<Customer>>(p => p.CanRead);

    // Checking whether the user has a permission for writing to Customer entities
    imContext.Permissions.Contains<Permission<Customer>>(p => p.CanWrite);

    // another way
    var p = imContext.Permissions.Get<Permission<Customer>>();
    if (p != null && p.CanRead)
      // allow doing some stuff
  }

To end the impersonation call ImpersonationContext.Undo() or Dispose() method.
Impersonation contexts can be nested, e.g.:

  using (var userContext = session.Impersonate(user)) {
    // do some user-related stuff

    using (var adminContext = session.Impersonate(admin)) {
      // do some admin stuff
    }

    // we are still in user impersonation context
  }
  // no context here

7. Secure (restrictive) queries
A role may set up a condition that will be automatically added to any query and limits the query results, e.g.:

  public class AutomobileManagerRole : EmployeeRole
  {
    private static IQueryable<Customer> GetCustomers(ImpersonationContext context, QueryEndpoint query)
    {
      return query.All<Customer>()
        .Where(customer => customer.IsAutomobileIndustry);
    }

    protected override void RegisterPermissions()
    {
      base.RegisterPermissions();
      // This permission tells that a principal can read/write customers 
      // but only those that are returned by the specified condition
      RegisterPermission(new CustomerPermission(true, GetCustomers));
    }

    public AutomobileManagerRole(Session session)
      : base(session) {}
  }

Now all employees that have AutomobileManagerRole will read 
customers that have IsAutomobileIndustry property set to true, e.g.:

  using (var session = Domain.OpenSession()) {
    using (var transaction = session.OpenTransaction()) {
      var automobileManagerRole = session.Query.All<AutomobileManagerRole>().Single();
      var user = new User(session);
      user.Name = "peter";
      user.SetPassword("password");
      user.Roles.Add(automobileManagerRole);

      using (var context = session.Impersonate(user)) {
        var customers = Query.All<Customer>();
        // Inside the impersonation context the above-mentioned query condition
        // will be added automatically so user will get only automobile customers
      }
      transaction.Complete();
    }
  }

References
----------
http://doextensions.codeplex.com