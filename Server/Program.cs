using Server;

var host = Host.CreateDefaultBuilder()
    .ConfigureWebHostDefaults(webHost =>
    {
        webHost.UseStartup<Startup>();
    })
    .Build();

await host.StartAsync();

while (true)
{
    var key = Console.ReadKey();
    if (key.Key == ConsoleKey.Enter)
    {
        break;
    }
}

Console.WriteLine("Shutting down...");
await Task.Delay(TimeSpan.FromSeconds(1));
await host.StopAsync();