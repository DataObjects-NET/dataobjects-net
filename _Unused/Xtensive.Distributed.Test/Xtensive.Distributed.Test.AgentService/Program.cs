using System.ServiceProcess;

namespace Xtensive.Distributed.Test.AgentService
{
  internal static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    private static void Main()
    {
      var serviceBases = new ServiceBase[] {new Service()};
      ServiceBase.Run(serviceBases);
    }
  }
}