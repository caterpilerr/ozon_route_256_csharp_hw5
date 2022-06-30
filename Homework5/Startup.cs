using Homework5.Contracts;
using Homework5.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

namespace Homework5
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var multiplexer =
                ConnectionMultiplexer.Connect(Configuration.GetSection("Redis:ConnectionString").Value);
            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            services.AddSingleton<IProcessedKafkaMessages<string, string>, ProcessedKafkaMessages<string, string>>();
            services.AddSingleton<IBatchKafkaConsumer<string, string>, BatchKafkaConsumer<string, string>>();
            services.AddSingleton<IMessageProcessor<string, string>, SimpleStringMessageProcessor>();
            services.AddHostedService<TestBackgroundKafkaConsumer>();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Homework5", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Homework5 v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}