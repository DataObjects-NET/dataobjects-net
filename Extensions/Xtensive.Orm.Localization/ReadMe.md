Xtensive.Orm.Localization
=========================

Summary
-------
The extension transparently solves a task of application or service localization.
This implies that localizable resources are a part of domain model so they are stored in database.

Prerequisites
-------------
DataObjects.Net 6.0.x or later (http://dataobjects.net)

Implementation
--------------

Implement ILocalizable<TLocalization> on your localizable entities, e.g.:

```csharp
  [HierarchyRoot]
  public class Page : Entity, ILocalizable<PageLocalization>
  {
    [Field, Key]
    public int Id { get; private set; }

    // Localizable field. Note that it is non-persistent
    public string Title
    {
      get { return Localizations.Current.Title; }
      set { Localizations.Current.Title = value; }
    }

    [Field] // This is a storage of all localizations for Page class
    public LocalizationSet<PageLocalization> Localizations { get; private set; }

    public Page(Session session) : base(session) {}
  }
```

Define corresponding localizations, e.g.:

```csharp
  [HierarchyRoot]
  public class PageLocalization : Localization<Page>
  {
    [Field(Length = 100)]
    public string Title { get; set; }

    public PageLocalization(Session session, CultureInfo culture, Page target)
      : base(session, culture, target) {}
  }
```

Examples of usage
-----------------

**Example #1**. Access localizable properties as regular ones, e.g.:

```csharp
  page.Title = "Welcome";
  string title = page.Title;
```

**Example #2**. Mass editing of localizable properties:

```csharp
  var en = new CultureInfo("en-US");
  var sp = new CultureInfo("es-ES");
  var page = new Page(session);
  page.Localizations[en].Title = "Welcome";
  page.Localizations[sp].Title = "Bienvenido";
```

**Example #3**. Value of localizable properties reflects culture of the current Thread, e.g.:

```csharp
  Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
  string title = page.Title; // title is "Welcome"

  Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
  string title = page.Title; // title is "Bienvenido"
```

**Example #4**. Instead of altering CurrentThread, instance of LocalizationScope can be used, e.g.:

```csharp
  using (new LocalizationScope(new CultureInfo("en-US"))) {
    string title = page.Title; // title is "Welcome"
  }

  using (new LocalizationScope(new CultureInfo("es-ES"))) {
    string title = page.Title; // title is "Bienvenido"
  }
```

**Example #5**. LINQ queries that include localizable properties are transparently translated

```csharp
  Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
  var query = from p in session.Query.All<Page>()
    where p.Title=="Welcome"
    select p;
  Assert.AreEqual(1, query.Count());

  Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
  var query = from p in session.Query.All<Page>()
    where p.Title=="Bienvenido"
    select p;
  Assert.AreEqual(1, query.Count());
```