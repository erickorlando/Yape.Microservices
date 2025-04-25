using MediatR;
using Microsoft.EntityFrameworkCore;
using Yape.TransactionService.Application.EventHandlers;
using Yape.TransactionService.Application.Interfaces;
using Yape.TransactionService.Application.UseCases.CreateTransaction;
using Yape.TransactionService.Domain.Interfaces;
using Yape.TransactionService.Infrastructure.Messaging;
using Yape.TransactionService.Infrastructure.Persistence;
using Yape.TransactionService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<KafkaConsumerService>();

builder.Services.AddScoped<TransactionUpdatedEventHandler>();

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddSingleton<IMessageProducer, KafkaMessageProducer>();

// Register MediatR
builder.Services.AddMediatR(typeof(CreateTransactionCommand).Assembly);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("YapeTransactionService"));
});

var app = builder.Build();

// Configure the HTTP request pipelinSe.
// if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();