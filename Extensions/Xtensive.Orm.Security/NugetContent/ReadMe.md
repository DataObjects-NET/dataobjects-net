Xtensive.Orm.Security
=====================

Summary
-------
The extension provides full-fledged security layer (authentication services, principals, roles, secured queries)
There are 2 main parts that can also be used separately: authentication services and role-based access to domain entities

Prerequisites
-------------
DataObjects.Net 7.1.x or later (http://dataobjects.net)

How to use
----------

Include types from Xtensive.Orm.Security assembly into the domain:

```xml
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
```

If you are planning to use one of authentication services add 

```xml
  <section name="Xtensive.Orm.Security" type="Xtensive.Orm.Security.Configuration.ConfigurationSection, Xtensive.Orm.Security" />
```

and set up the desired hashing service:

```xml
  <Xtensive.Orm.Security>
    <hashingService name="plain"/>
    <!-- other options are: md5, sha1, sha256, sha384, sha512 -->
  </Xtensive.Orm.Security>
```

Other examples of how to configure the extension are in section below

Examples
--------

**Example #1**. Definition of a class that inherits GenericPrincipal class that will describe your users, e.g.:

```csharp
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

    public User(Session session)
      : base(session)
    {
    }
  }
```

**Example #2**. Having the User class defined above, it can be used for user creation and authentication.

```csharp
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
```

**Example #3**. Definition of a hierarchy of roles for users. A role is a set of permissions for a job fuction within a company, e.g.:

```
EmployeeRole
|
|- StockManagerRole
|
|- SalesRepresentativeRole
     |
     |- SalesManagerRole
     |
     |- SalesPresidentRole
```

The role tree above can be represented like following:

```csharp
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

  public class SalesRepresentativeRole : EmployeeRole
  {
    protected override void RegisterPermissions()
    {
      // Sales manager inherits Employee permissions
      base.RegisterPermissions();

      // All sales representative can read customer
      RegisterPermission(new Permission<Customer>());
      // All sales representative can read orders
      RegisterPermission(new Permission<Order>());
    }

    protected EmployeeRole(Session session)
      : base(session) {}
  }

  public class SalesManagerRole : SalesRepresentativeRole
  {
    protected override void RegisterPermissions()
    {
      // Sales manager inherits SalesRepresentativeRole permissions
      base.RegisterPermissions();

      // Sales managers can read and write orders
      RegisterPermission(new Permission<Order>(canWrite:true));
    }

    protected SalesManagerRole(Session session)
      : base(session) {}
  }

  public class SalesPresidentRole : SalesRepresentativeRole
  {
    protected override void RegisterPermissions()
    {
      // Sales manager inherits SalesRepresentativeRole permissions
      base.RegisterPermissions();

      // Sales president can read and write customers
      RegisterPermission(new Permission<Customer>(canWrite:true));
      // Sales president can read and write orders
      RegisterPermission(new Permission<Order>(canWrite:true));
    }

    protected SalesManagerRole(Session session)
      : base(session) {}
  }
```

The roles should be intitalized on first domain build for being able to use them further, e.g:

```csharp
  using (var session = Domain.OpenSession()) {
    using (var transaction = session.OpenTransaction()) {
      new SalesRepresentativeRole(session);
      new SalesManagerRole(session);
      new SalesPresidentRole(session);
      new StockManagerRole(session);
      transaction.Complete();
    }
  }
```

**Example #4**. Assigning one of roles to a user.

```csharp
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
```

**Example #5**. Checking whether a user has the required role

```csharp
  user.IsInRole("StockManagerRole");
  // or
  user.Roles.Contains(stockManagerRole);
```

**Example #6**. Session impersonation

```csharp
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
```

To end the impersonation call ImpersonationContext.Undo() or Dispose() method.
Impersonation contexts can be nested, e.g.:

```csharp
  using (var userContext = session.Impersonate(user)) {
    // do some user-related stuff

    using (var adminContext = session.Impersonate(admin)) {
      // do some admin stuff
    }

    // we are still in user impersonation context
  }
  // no context here
```

**Example #7**. Secure (restrictive) queries.
A role may set up a condition that will be automatically added to any query and limits the query results, e.g.:

```csharp
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
```

Now all employees that have AutomobileManagerRole will read 
customers that have IsAutomobileIndustry property set to true, e.g.:

```csharp
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
```



Examples of how to configure extension
--------------------------------------

Additionally to "How to use" section it provides extra examples of how to configure and/or read extension configuration.

The example in "How to use" section uses old fasioned API of configuration files, yet usable in many applications. But
there are some cases which may require usage of different API or work-around certain cases with existing one.

**Example #1** Reading old-style configuration of an assembly in NET 5 and newer.

Due to new architecture without AppDomain (which among the other things was in charge of gathering configuration files of loaded assemblies
as it would be one configuration file) ```System.Configuration.ConfigurationManager``` now reads only configuration file of actual executable, loaded 
assemblies' configuration files stay unreachable by default, though there is need to read some data from them.
A great example is test projects which are usually get loaded by test runner executable, and the only configuration accessible in this case
is test runner one.

Extra step is required to read configuration files in such cases. Thankfully, ```ConfigurationManager``` has methods to get access to assemblies' configurations.

To get access to an assembly configuration file it should be opened explicitly by

```csharp
  var configuration = ConfigurationManager.OpenExeConfiguration(typeof(SomeTypeInConfigOwnerAssembly).Assembly.Location);
```

The instance returned from ```OpenExeConfiguration``` provides access to sections of the assembly configuration. DataObjects.Net configurations
(```DomainConfiguration```, ```SecurityConfiguration```, etc.) have ```Load()``` methods that can recieve this instance.
```SecurityConfiguration``` can be read like so

```csharp
  var configuration = ConfigurationManager.OpenExeConfiguration(typeof(SomeTypeInConfigOwnerAssembly).Assembly.Location);
  var securityConfig = SecurityConfiguration.Load(configuration);

  // loaded configuration should be manually placed to
  domainConfiguration.ExtensionConfigurations.Set(securityConfig);
```

The ```domainConfiguration.ExtensionConfigurations``` is a new unified place from which the extension will try to get its configuration
instead of calling default parameterless ```Load()``` method, which has not a lot of sense now, though the method is kept as a second source
for backwards compatibility.

For more convenience, ```DomainConfiguration``` extensions are provided, which make code more neater.
For instance,

```csharp
  var configuration = ConfigurationManager.OpenExeConfiguration(typeof(SomeTypeInConfigOwnerAssembly).Assembly.Location);

  var domainConfiguration = DomainConfiguration.Load(configuration);

  // the extension hides getting configuration with SecurityConfiguration.Load(configuration)
  // and also putting it to ExtensionConfigurations collection.
  domainConfiguration.ConfigureSecurityExtension(configuration);
```

Remember the requirement to register ```Xtensive.Orm.Security``` to domain? The extension tries to register this assembly to ```DomainConfiguration.Types``` collection
so even if you miss registration but called extension method required types of Security extension will be registered in Domain types.

Custom section names are also supported if for some reason default section name is not used.


**Example #2** Reading old-style configuration of an assembly in a project that uses appsettings.json file.

If for some reason there is need to keep the old-style configuration then there is a work-around as well.
Static configuration manager provides method ```OpenMappedExeConfiguration()``` which allows to get access to
any *.config file as ```System.Configuration.Configuration``` instance. For example,

```csharp
  ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
  configFileMap.ExeConfigFilename = "Orm.config"; //or other file name, the file should exist bin folder
  var configuration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
```

After that, as in previous example, the instance can be passed to ```Load``` method of ```SecurityConfiguration``` to read extension configuration
and later put it to ```DomainConfiguration.ExtensionConfigurations```

```csharp
  var securityConfiguration = SecurityConfiguration.Load(configuration);

  domainConfiguration.ExtensionConfigurations.Set(securityConfiguration);
```

Extension usage will look like

```csharp
  domainConfiguration.ConfigureSecurityExtension(configuration);
```


**Example #3** Configure using Microsoft.Extensions.Configuration API.

This API allows to have configurations in various forms including JSON and XML formats.
Loading of such files may differ depending on .NET version, check Microsoft manuals for instructions.

Allowed JSON and XML configuration definition look like below

```xml
<configuration>
  <Xtensive.Orm.Security>
    <HashingService>sha512</HashingService>
    <AuthenticationService>CustomAuthenticationService</AuthenticationService>
  </Xtensive.Orm.Security>
</configuration>
```

```json
{
  "Xtensive.Orm.Security": {
    "HashingService" : "sha512",
    "AuthenticationService" : "CustomAuthenticationService"
  }
}
```

The API has certain issues with XML elements with attributes so it is recommended to use
more up-to-date attributeless nodes.
For JSON it is pretty clear, almost averyone knows its format.

```SecurityConfiguration.Load``` method can accept different types of abstractions from the API, including
- ```Microsoft.Extensions.Configuration.IConfiguration```;
- ```Microsoft.Extensions.Configuration.IConfigurationRoot```;
- ```Microsoft.Extensions.Configuration.IConfigurationSection```.

Loading of configuration may look like

```csharp
  
  var app = builder.Build();

  //...

  // tries to load from default section "Xtensive.Orm.Security"
  var securityConfig = SecurityConfiguration.Load(app.Configuration);

  domainConfiguration.ExtensionConfigurations.Set(securityConfig);
```

or, with use of extension, like


```csharp
  
  var app = builder.Build();

  //...

  // Tries to load from default section "Xtensive.Orm.Security"
  // and put it into domainConfiguration.ExtensionConfigurations.
  // Additionally, registers types of "Xtensive.Orm.Security" assembly.

  domainConfiguration.ConfigureSecurityExtension(app.Configuration);
```



**Example #4** Configure using Microsoft.Extensions.Configuration API from section with non-default name.

For configurations like

```xml
<configuration>
  <Orm.Security>
    <HashingService>sha512</HashingService>
    <AuthenticationService>CustomAuthenticationService</AuthenticationService>
  </Orm.Security>
</configuration>
```

```json
{
  "Orm.Security": {
    "HashingService" : "sha512",
    "AuthenticationService" : "CustomAuthenticationService"
  }
}

Loading of configuration may look like

```csharp
  var app = builder.Build();

  //...

  var securityConfig = SecurityConfiguration.Load(app.Configuration, "Orm.Security");

  domainConfiguration.ExtensionConfigurations.Set(securityConfig);
```

or, with use of extension, like

```csharp
  var app = builder.Build();

  //...

  domainConfiguration.ConfigureSecurityExtension(app.Configuration, "Orm.Security");
```


**Example #5** Configure using Microsoft.Extensions.Configuration API from sub-section deeper in section tree.

If for some reason extension configuration should be moved deeper in section tree like something below

```xml
<configuration>
  <Orm.Extensions>
    <Xtensive.Orm.Security>
      <HashingService>sha512</HashingService>
      <AuthenticationService>CustomAuthenticationService</AuthenticationService>
    </Xtensive.Orm.Security>
  </Orm.Extensions>
</configuration>
```

or in JSON

```json
{
  "Orm.Extensions": {
    "Xtensive.Orm.Security": {
      "HashingService" : "sha512",
      "AuthenticationService" : "CustomAuthenticationService"
    }
  }
}
```

Then section must be provided manually, code may look like

```csharp
  
  var app = builder.Build();

  //...

  var configurationRoot = app.Configuration;
  var extensionsGroupSection = configurationRoot.GetSection("Orm.Extensions");
  var securitySection = extensionsGroupSection.GetSection("Xtensive.Orm.Security");

  var securityConfig = SecurityConfiguration.Load(securitySection);

  domainConfiguration.ExtensionConfigurations.Set(securityConfig);
```

or, with use of extension method, like

```csharp
  
  var app = builder.Build();

  //...

  var configurationRoot = app.Configuration;
  var extensionsGroupSection = configurationRoot.GetSection("Orm.Extensions");
  var securitySection = extensionsGroupSection.GetSection("Xtensive.Orm.Security");

  domainConfiguration.ConfigureSecurityExtension(securitySection);
```
