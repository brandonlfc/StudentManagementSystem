using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authn
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        static public string connString = @"Data source=tcp:student-management-system.database.windows.net,1433;Initial Catalog=StudentManagementSystem;Persist Security Info=False;User ID=SMSAdmin;Password=bfs9bRYxMezZ5rc;Integrated Security = False;MultipleActiveResultSets=False;Encrypt=True;Connection Timeout=30;";

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
