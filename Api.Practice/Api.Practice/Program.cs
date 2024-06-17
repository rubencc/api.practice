using Api.Practice.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddEndpointsApiExplorer()
    .AddControllers();
builder.Services
    .AddSwaggerGen()
    .AddDependencies();


var app = builder.Build();


app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseEndpoints(x =>
{
    x.MapControllers();
});


app.Run();