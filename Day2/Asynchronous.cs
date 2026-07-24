// WeatherService.cs — simulates a slow operation, like a web/API call
    class WeatherService
    {
        public async Task<string> GetWeatherAsync(string city)
        {
            Console.WriteLine($"Fetching weather for {city}...");
            await Task.Delay(2000); // simulates a 2-second network call, WITHOUT freezing the app
            return $"{city}: 25°C, Sunny";
        }
    }