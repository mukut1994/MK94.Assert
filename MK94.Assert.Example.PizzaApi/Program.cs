namespace MK94.Assert.Example.PizzaApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            WebApplication app = BuildWebApp(builder);

            app.Run();
        }

        private static WebApplication BuildWebApp(WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();

            var app = builder.Build();

            app.MapControllers();
            return app;
        }
    }
}