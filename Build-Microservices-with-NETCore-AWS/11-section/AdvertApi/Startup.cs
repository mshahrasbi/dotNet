using AdvertApi.HealthChecks;
using AdvertApi.Services;
using Amazon.ServiceDiscovery;
using Amazon.ServiceDiscovery.Model;
using Amazon.Util;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdvertApi
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
            services.AddAutoMapper(typeof(Startup));
            services.AddTransient<IAdvertStorageService, DynamoDBAdvertStorage>();
            services.AddControllers();
            services.AddHealthChecks( checks =>
            {
                checks.AddCheck<StorageHealthCheck>("Storage", new System.TimeSpan(0, 1, 0));
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "web Advertisment Apis",
                    Version = "Version 1",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "Mo Sha",
                        Email = "xxx@yyy.com"
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async Task Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            //app.UseHealthChecks("/health");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web Advert Api");
            });

            await RegisterToCloudMap();
        }

        private async Task RegisterToCloudMap()
        {
            const string serviceId = "xxxxxxxxxx";
            var instanceId = EC2InstanceMetadata.InstanceId;
            if (!string.IsNullOrEmpty(instanceId))
            {
                /*
                 * if you want to get a public IP address before you can do that, you need just
                 * to query for the network adapters that are attached to your EC2 and then get 
                 * the first one which ever that has a public IP
                 */
                var ipv4 = EC2InstanceMetadata.PrivateIpAddress;
                var client = new AmazonServiceDiscoveryClient();
                await client.RegisterInstanceAsync(new RegisterInstanceRequest { 
                    InstanceId = instanceId,
                    ServiceId = serviceId,
                    Attributes = new Dictionary<string, string>() { { "AWS_INSTANCE_IPV4", ipv4}, { "AWS_INSTANCE_PORT", "80"} }
                });
            }
        }
    }
}
