using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using WebBaseApi.Data;
using AutoMapper;
using WebBaseApi.AutoMapper;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.Extensions.PlatformAbstractions;
using System.IO;
using WebBaseApi.Common;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using System.Security;

namespace WebBaseApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private JWTTokenOptions tokenOptions;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            //添加DbContext的注入
            services.AddDbContext<ApiContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ApiConnection")));

            //添加AutoMapper
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutoMapperProfileConfiguraion());
            });
            services.AddSingleton<IMapper>(mapper => mapperConfig.CreateMapper());

            //配置Swagger生成帮助文档
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "WebBaseApi接口文档",
                    Description = "RESTful API for WebBaseApi",
                    TermsOfService = "None",
                    Contact = new Contact { Name = "Anubis", Email = "dusky_anubis@hotmail.com", Url = "" }
                });

                options.IncludeXmlComments(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath,
                    "WebBaseApi.XML")); // 注意：此处替换成所生成的XML documentation的文件名。
                options.DescribeAllEnumsAsStrings();

                options.OperationFilter<HttpHeaderOperation>(); // 添加httpHeader参数
            });

            //配置跨域

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                  builder => builder.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials());
            });



            //JWT相关
            tokenOptions = new JWTTokenOptions()
            {
                Issuer = "WebBaseApiIssuer", // 签发者名称
                Audience = "WebBaseApiAudience",//使用者名称
                Expiration = TimeSpan.FromDays(7),//指定Token过期时间
                SecretKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("Jwt")["SecretKey"])),
            };
            services.AddSingleton<JWTTokenOptions>(tokenOptions);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = tokenOptions.SecretKey,
                ValidateAudience = true,
                ValidAudience = tokenOptions.Audience, // 设置接收者必须是 TestAudience
                ValidateIssuer = true,
                ValidIssuer = tokenOptions.Issuer, // 设置签发者必须是 TestIssuer
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            //添加JWT身份验证
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.IncludeErrorDetails = true;
                o.TokenValidationParameters = tokenValidationParameters;
                o.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = c =>
                    {
                        c.NoResult();
                        c.Response.StatusCode = 401;
                        c.Response.ContentType = "text/plain";
                        return c.Response.WriteAsync(c.Exception.ToString());
                    }
                };
            });

            //鉴权规则设置

        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, ApiContext context)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            //注入Swagger生成API文档,此方法需要卸载app.UseMvc()方法前
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "WebBaseApi v1");
            });
            //使用跨域
            app.UseCors("CorsPolicy");

            app.UseAuthentication();
            app.UseMvc();

            //注入ApiContext
            DbInitializer.Initialize(context);
        }
    }
}
