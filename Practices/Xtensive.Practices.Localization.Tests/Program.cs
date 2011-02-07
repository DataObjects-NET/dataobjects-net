using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Xtensive.Orm;
using Xtensive.Practices.Localization.Tests.Model;
using Xtensive.Reflection;
using Xtensive.Orm.Configuration;

namespace Xtensive.Practices.Localization.Tests
{
  internal class Program
  {

    private static void QueryPages()
    {
      Console.WriteLine("Querying Pages");
      Console.WriteLine(new string('-', 14));
      using (Session.Open(domain)) {
        using (var ts = Transaction.Open()) {

          Console.WriteLine("Using implicit join through LINQ preprocessor");
          var pages = from p in Xtensive.Orm.Query.All<Page>()
          where p.Title=="Welcome!"
          select p;
          Console.WriteLine(string.Format("Found pages: {0}", pages.ToList().Count));
          Console.WriteLine();

          ts.Complete();
        }

        Console.WriteLine("Using explicit hard-coded join");
        using (var ts = Transaction.Open()) {
          var pages = from p in Xtensive.Orm.Query.All<Page>()
          join pl in Xtensive.Orm.Query.All<PageLocalization>()
            on p equals pl.Target
          where pl.CultureName==LocalizationContext.Current.CultureName && pl.Title=="Welcome!"
          select p;
          Console.WriteLine(string.Format("Found pages: {0}", pages.ToList().Count));
          Console.WriteLine();

          ts.Complete();
        }

        Console.WriteLine("Using Query.All<Page, PageLocalization>()");
        using (var ts = Transaction.Open()) {
          var pairs = from pair in Query.All<Page, PageLocalization>()
          where pair.Localization.Title=="Welcome!"
          select pair.Target;
          Console.WriteLine(string.Format("Found pages: {0}", pairs.ToList().Count));
          Console.WriteLine();

          ts.Complete();
        }
      }
    }

    private static void QueryMyPages()
    {
      Console.WriteLine();
      Console.WriteLine("Querying MyPages");
      Console.WriteLine(new string('-', 16));
      using (Session.Open(domain)) {
        using (var ts = Transaction.Open()) {

          Console.WriteLine("Using implicit join through LINQ preprocessor");
          var pages = from p in Xtensive.Orm.Query.All<MyPage>()
          where p.MyContent=="MyContent of MyPage"
          select p;
          Console.WriteLine(string.Format("Found pages: {0}", pages.ToList().Count));
          Console.WriteLine();

          ts.Complete();
        }

        Console.WriteLine("Using explicit hard-coded join");
        using (var ts = Transaction.Open()) {
          var pages = from p in Xtensive.Orm.Query.All<MyPage>()
          join pl in Xtensive.Orm.Query.All<PageLocalization>()
            on p equals pl.Target
          where pl.CultureName==LocalizationContext.Current.CultureName && pl.MyContent=="MyContent of MyPage"
          select p;
          Console.WriteLine(string.Format("Found pages: {0}", pages.ToList().Count));
          Console.WriteLine();

          ts.Complete();
        }

        Console.WriteLine("Using Query.All<Page, PageLocalization>()");
        using (var ts = Transaction.Open()) {
          var pairs = from pair in Query.All<Page, PageLocalization>()
          where pair.Localization.MyContent=="MyContent of MyPage"
          select pair.Target;
          Console.WriteLine(string.Format("Found pages: {0}", pairs.ToList().Count));
          Console.WriteLine();

          ts.Complete();
        }
      }
    }
  }
}