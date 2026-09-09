using Summarix.Application;
using Summarix.Infrastructure;
using Summarix.Presentation;
using Summarix.Presentation.Exceptions;

var builder = WebApplication.CreateBuilder(args);

const string frontendCorsPolicy = "FrontendCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:80", "http://localhost")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("Content-Disposition");
    });
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation(builder.Configuration, builder.WebHost);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Summarix API v1");
        options.RoutePrefix = string.Empty;
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(frontendCorsPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();
