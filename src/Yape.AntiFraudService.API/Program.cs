using Microsoft.EntityFrameworkCore;
using Yape.AntiFraudService.Application.EventHandlers;
using Yape.AntiFraudService.Application.Interfaces;
using Yape.AntiFraudService.Domain.Interfaces;
using Yape.AntiFraudService.Domain.Services;
using Yape.AntiFraudService.Infrastructure.Messaging;
using Yape.AntiFraudService.Infrastructure.Persistence;
using Yape.AntiFraudService.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<KafkaConsumerService>();

builder.Services.AddScoped<TransactionCreatedEventHandler>();

builder.Services.AddScoped<FraudDetectionService>(); 

builder.Services.AddSingleton<ITransactionStatusProducer, KafkaTransactionStatusProducer>();
builder.Services.AddScoped<IAccumulatedValueRepository, AccumulatedValueRepository>();


builder.Services.AddDbContext<AntiFraudDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("YapeAntiFraudService"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AntiFraudDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
