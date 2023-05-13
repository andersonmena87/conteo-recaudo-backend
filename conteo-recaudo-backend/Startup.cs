using conteo_recaudo_backend.DAL;
using ConteoRecaudo.BLL;
using ConteoRecaudo.BLL.Interfaces;
using ConteoRecaudo.Infraestructure;
using ConteoRecaudo.Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace ConteoRecaudo
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services) {
            string? nombreAPI = "API Conteo y Recaudo";
            string connectionString = Configuration.GetConnectionString("conexionSqlServer");

            services.AddSwaggerGen();
            services.AddCors();

            services.AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = nombreAPI, Version = "v1" });
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string xmlFile = $"APIConteoRecaudo.xml";
                string xmlPath = Path.Combine(baseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            #region Scopeds
            services.AddScoped<IRecaudoRepository, RecaudoRepository>();
            services.AddScoped<IRecaudoBL, RecaudoBL>();
            #endregion
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors(options =>
            {
                options.AllowAnyMethod();
                options.AllowAnyHeader();
                options.AllowAnyOrigin();
            });

            app.UseDeveloperExceptionPage();
            app.UseHttpsRedirection();
            app.UseFileServer(enableDirectoryBrowsing: true);
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
