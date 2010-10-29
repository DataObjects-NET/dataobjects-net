using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using Xtensive.IoC;
using Xtensive.Orm;
using Xtensive.Reflection;
using Xtensive.Practices.Localization.Internals;
using Xtensive.Practices.Localization.Model;
using Xtensive.Orm.Configuration;
using ConfigurationSection=Xtensive.IoC.Configuration.ConfigurationSection;

namespace Xtensive.Practices.Localization
{
  internal class Program
  {
    public static CultureInfo English = new CultureInfo("en-US");
    public static CultureInfo Spanish = new CultureInfo("es-ES");
    private static Domain domain;

    private static void Main(string[] args)
    {
      BuildDomain();

      CreatePages();
      EnumeratePages();
      QueryPages();
      QueryMyPages();

      Console.ReadKey();
    }

    private static void CreatePages()
    {
      using (Session.Open(domain)) {
        using (var ts = Transaction.Open()) {
          var welcomePage = new Page();

          // Editing localizable properties through localization scope
          using (new LocalizationScope(English)) {
            welcomePage.Title = "Welcome!";
            welcomePage.Content = "My dear guests, welcome to my birthday party!";
          }

          // Editing localizable properties through CurrentThread.CurrentCulture
          Thread.CurrentThread.CurrentCulture = Spanish;
          welcomePage.Title = "Bienvenido!";
          welcomePage.Content = "Mis amigos mejores! Bienvenido a mi cumpleanos!";

          var goodbyePage = new Page();

          // Editing localizations directly
          goodbyePage.Localizations[English].Title = "Goodbye!";
          goodbyePage.Localizations[English].Content = "Goodbye, my dear friends.";
          goodbyePage.Localizations[Spanish].Title = "Adiós!";
          goodbyePage.Localizations[Spanish].Content = "Adiós, mis amigos.";

          // Reverting Thread.CurrentCulture to English
          Thread.CurrentThread.CurrentCulture = English;

          // Creating MyPage instance to check whether inheritance is supported
          var myPage = new MyPage();
          myPage.Title = "Some title";
          myPage.Content = "Some content";
          myPage.MyContent = "Some my content";

          ts.Complete();
        }
      }
    }

    private static void EnumeratePages()
    {
      Console.WriteLine("Enumerating Pages");
      using (Session.Open(domain)) {
        using (var ts = Transaction.Open()) {

          // Enumerating through localization scope activation
          Console.WriteLine(string.Format("Switching to {0} using LocalizationScope", English.Name));
          using (new LocalizationScope(English))
            foreach (var page in Xtensive.Orm.Query.All<Page>())
              Console.WriteLine(string.Format("{0} {1}", page.Title, page.Content));
          Console.WriteLine();

          // Enumerating using Thread.Current.CurrentCulture change
          Console.WriteLine(string.Format("Switching to {0} using Thread.Current.CurrentCulture", Spanish.Name));
          Thread.CurrentThread.CurrentCulture = Spanish;
          foreach (var page in Xtensive.Orm.Query.All<Page>())
            Console.WriteLine(string.Format("{0} {1}", page.Title, page.Content));
          Thread.CurrentThread.CurrentCulture = English;
          Console.WriteLine();

          ts.Complete();
        }
      }
    }

    private static void QueryPages()
    {
      Console.WriteLine("Querying Pages");
      using (Session.Open(domain)) {
        using (var ts = Transaction.Open()) {

          Console.WriteLine("Implicit join through preprocessor");
          var pages = from p in Xtensive.Orm.Query.All<Page>()
          where p.Title=="Welcome!"
          select p;
          Console.WriteLine(pages.ToList().Count);

          ts.Complete();
        }

        Console.WriteLine("Explicit hard-coded join");
        using (var ts = Transaction.Open()) {
          var pages = from p in Xtensive.Orm.Query.All<Page>()
          join pl in Xtensive.Orm.Query.All<PageLocalization>()
            on p equals pl.Target
          where pl.CultureName==LocalizationContext.Current.CultureName && pl.Title=="Welcome!"
          select p;
          Console.WriteLine(pages.ToList().Count);

          ts.Complete();
        }

        Console.WriteLine("using Query.All<Page, PageLocalization>()");
        using (var ts = Transaction.Open()) {
          var pairs = from pair in Query.All<Page, PageLocalization>()
          where pair.Localization.Title=="Welcome!"
          select pair.Target;
          Console.WriteLine(pairs.ToList().Count);

          ts.Complete();
        }
      }
    }

    private static void QueryMyPages()
    {
      Console.WriteLine("Querying MyPages");
      using (Session.Open(domain)) {
        using (var ts = Transaction.Open()) {

          Console.WriteLine("Implicit join through preprocessor");
          var pages = from p in Xtensive.Orm.Query.All<MyPage>()
          where p.MyContent=="Some my content"
          select p;
          Console.WriteLine(pages.ToList().Count);

          ts.Complete();
        }

        Console.WriteLine("Explicit hard-coded join");
        using (var ts = Transaction.Open()) {
          var pages = from p in Xtensive.Orm.Query.All<MyPage>()
          join pl in Xtensive.Orm.Query.All<PageLocalization>()
            on p equals pl.Target
          where pl.CultureName==LocalizationContext.Current.CultureName && pl.MyContent=="Some my content"
          select p;
          Console.WriteLine(pages.ToList().Count);

          ts.Complete();
        }

        Console.WriteLine("using Query.All<Page, PageLocalization>()");
        using (var ts = Transaction.Open()) {
          var pairs = from pair in Query.All<Page, PageLocalization>()
          where pair.Localization.MyContent=="Some my content"
          select pair.Target;
          Console.WriteLine(pairs.ToList().Count);

          ts.Complete();
        }
      }
    }

    private static void BuildDomain()
    {
      domain = Domain.Build(DomainConfiguration.Load("Default"));
      BuildLocalizationMap();
    }

    private static void BuildLocalizationMap()
    {
      var map = new TypeLocalizationMap();
      foreach (var localizableTypeInfo in domain.Model.Types.Entities) {
        var type = localizableTypeInfo.UnderlyingType;
        if (!type.IsOfGenericInterface(typeof (ILocalizable<>)))
          continue;
        var localizationType = type.GetInterface("ILocalizable`1").GetGenericArguments()[0];
        map.Register(type, localizationType.GetTypeInfo(domain));
      }
      domain.Extensions.Set(map);
    }
  }
}