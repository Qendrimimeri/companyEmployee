using CompanyEmployees.Extensions;
using Contracts;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NLog;
using CompanyEmployees.Presentation.ActionFilters;
using Shared.DataTransferObjects;
using AspNetCoreRateLimit;

NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter() =>
    new ServiceCollection().AddLogging().AddMvc().AddNewtonsoftJson()
    .Services.BuildServiceProvider()
    .GetRequiredService<IOptions<MvcOptions>>().Value.InputFormatters
    .OfType<NewtonsoftJsonPatchInputFormatter>().First();


var builder = WebApplication.CreateBuilder(args);

LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));


builder.Services.AddControllers(config =>
{
    config.RespectBrowserAcceptHeader = true;
    config.ReturnHttpNotAcceptable = true;
    config.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
    config.CacheProfiles.Add("120SecondsDuration", new CacheProfile
    {
        Duration = 120
    });
}).AddApplicationPart(typeof(CompanyEmployees.Presentation.AssemblyReference).Assembly);
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddScoped<ValidationFilterAttribute>();
builder.Services.AddCustomMediaTypes();
builder.Services.ConfigureVersioning();
builder.Services.ConfigureResponseCaching();
builder.Services.ConfigureHttpCacheHeaders();
builder.Services.AddMemoryCache();
builder.Services.ConfigureRateLimitingOptions();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication();
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.AddOptionPattern(builder.Configuration);
var app = builder.Build();


var logger = app.Services.GetRequiredService<ILoggerManager>();

app.ConfigureExceptionHandler(logger);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();

    app.UseSwaggerUI();
}

if (app.Environment.IsProduction())

    app.UseHsts();

app.UseIpRateLimiting();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});

app.UseCors("CorsPolicy");

app.UseResponseCaching();

app.UseHttpCacheHeaders();

app.UseAuthentication();

app.UseAuthorization();

//app.Map("/usingmapbranch", builder =>
//{
//    builder.Use(async (context, next) =>
//    {
//        Console.WriteLine("Map branch logic in the Use method before the next delegate");
//        await next.Invoke();
//        Console.WriteLine("Map branch logic in the Use method after the next delegate");
//    });
//    builder.Run(async context =>
//    {
//        Console.WriteLine($"Map branch response to the client in the Run method");
//        await context.Response.WriteAsync("Hello from the map branch.");
//    });
//});

//app.MapWhen(context => context.Request.Query.ContainsKey("testquerystring"), builder =>
//{
//    builder.Run(async context =>
//    {
//        await context.Response.WriteAsync("Hello from the MapWhen branch.");
//    });
//});


//app.Run(async context =>
//{
//    await context.Response.WriteAsync("Hello from the middleware component.");
//});

app.MapControllers();

app.Run();
