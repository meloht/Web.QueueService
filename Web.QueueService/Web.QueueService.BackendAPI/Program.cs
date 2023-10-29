
using Microsoft.Extensions.Configuration;
using Web.QueueService.Common;

namespace Web.QueueService.BackendAPI
{
    public class Program
    {
        public static AppSettingsQueueApi Config;
        public static void Main(string[] args)
        {
   
            var builder = WebApplication.CreateBuilder(args);

            Config = new AppSettingsQueueApi(builder.Configuration);
           
            // CosmosDBClient.Init(Config.CosmosDb);
            RedisResponseClient.InitializeConnectionString(Config.RedisResponse, Config.HttpTimeOut);
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}