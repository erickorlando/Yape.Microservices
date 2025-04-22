using Yape.TransactionService.Application.Services;
using Yape.TransactionService.Domain.Interfaces;
using Yape.TransactionService.Infrastructure.Kafka;
using Yape.TransactionService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ITransactionRepository, InMemoryTransactionRepository>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<ITransactionEventPublisher, KafkaTransactionProducer>();

var app = builder.Build();

// Configure the HTTP request pipelinSe.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();