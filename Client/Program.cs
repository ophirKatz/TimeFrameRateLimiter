// See https://aka.ms/new-console-template for more information
Console.WriteLine("Welcome to DOS-R-US!");

Console.WriteLine("Please enter the amount of http clients you would like to simulate: ");

var input = Console.ReadLine();
if (string.IsNullOrEmpty(input)) throw new InvalidOperationException("Invalid input! Exiting");

var numberOfClients = int.Parse(input);

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

var tasks = Enumerable.Range(0, numberOfClients)
    .Select(_ => Task.Run(() => SimulateClient(GetRandomClientId(numberOfClients), cancellationToken)))
    .ToArray();

input = Console.ReadLine();
if (string.IsNullOrEmpty(input))
{
    cancellationTokenSource.Cancel();

    await Task.WhenAll(tasks);
    Console.WriteLine("Gracefully closed all tasks. Goodbye!");
}

string GetRandomClientId(int clientsCount) => new Random().Next(1, clientsCount + 1).ToString();

async Task SimulateClient(string clientId, CancellationToken token)
{
    using var httpClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5086")
    };
    
    var requestUri = $"?clientId={clientId}";

    while (!token.IsCancellationRequested)
    {
        try
        {
            Console.WriteLine($"Sending a request: {httpClient.BaseAddress}/?clientId={clientId}");
            var response = await httpClient.GetAsync(requestUri, token);
            Console.WriteLine($"Request by clientId {clientId} received status {response.StatusCode}");
            // Wait a random time before trying again
            await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(100, 600)));
        }
        catch (TaskCanceledException)
        {
            break;
        }
    }
}