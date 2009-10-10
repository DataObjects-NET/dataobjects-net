using System;
using System.Linq;
using Xtensive.Storage;
using Xtensive.Storage.Configuration;
using $safeprojectname$.Model;

namespace $safeprojectname$
{
  class Program
  {
    static void Main(string[] args)
    {
      // Loading configuration section for in-memory database. 
      // See other cases in App.config file.
      var config = DomainConfiguration.Load("Default");
//      config.Types.Register(typeof(MyEntity).Assembly);

      var domain = Domain.Build(config);

      using (Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {

          // Creating new persistent object
          var helloWorld = new MyEntity {
            Text = "Hello World!"
          };

          // Committing transaction
          transactionScope.Complete();
        }
      }

      // Reading all persisted objects from another Session
      using (Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {
          foreach (var myEntity in Query<MyEntity>.All)
            Console.WriteLine(myEntity.Text);
          transactionScope.Complete();
        }
      }
      Console.ReadKey();
    }
  }
}
