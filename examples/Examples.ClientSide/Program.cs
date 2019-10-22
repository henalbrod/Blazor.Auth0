using Microsoft.AspNetCore.Blazor.Hosting;

namespace Examples.ClientSide
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebAssemblyHostBuilder CreateHostBuilder(string[] _args)
        {
            return BlazorWebAssemblyHost.CreateDefaultBuilder()
                .UseBlazorStartup<Startup>();
        }
    }
}
