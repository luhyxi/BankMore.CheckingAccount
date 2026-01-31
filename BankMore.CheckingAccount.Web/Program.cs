using BankMore.CheckingAccount.Web;
using BankMore.CheckingAccount.Web.Configs;
using BankMore.CheckingAccount.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

using var loggerFactory = LoggerFactory.Create(config => config.AddConsole());
var startupLogger = loggerFactory.CreateLogger<Program>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth(startupLogger);

builder.Services.AddServiceConfigs(startupLogger, builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapContaCorrenteEndpoints();

app.Run();
