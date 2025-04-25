# YAPE Bolivia AntiFraud Solution

## Overview

This solution comprises two microservices (AntiFraudService and TransactionService) designed to handle transaction processing and fraud detection. Both services are built using .NET 8.0 and follow the Clean Architecture pattern. Communication between the services is facilitated through Apache Kafka event messaging.

## Architecture

### Clean Architecture 

Both microservices implement Clean Architecture, which:

- Isolates the domain/business logic from external concerns
- Enables better testability and maintainability
- Structures the code in three main layers:
    - **Domain**: Core business logic
    - **Application**: Use cases and orchestration
    - **Infrastructure**: External dependencies (databases, messaging, etc.)

### AntiFraudService

The AntiFraudService is responsible for:

- Analyzing transactions for potential fraud
- Processing fraud detection rules
- Communicating results back to the TransactionService

### TransactionService

The TransactionService handles:

- Processing incoming transaction requests
- Storing transaction data
- Requesting fraud verification
- Updating transaction status based on fraud analysis results

## Technology Stack

- **Backend**: .NET 8.0
- **Databases**: PostgreSQL
- **Messaging**: Apache Kafka
- **Containerization**: Docker and Docker Compose
- **Orchestration**: Docker Compose for local development

## Communication Flow

1. TransactionService receives a transaction request
2. Transaction is saved to PostgreSQL
3. TransactionService publishes a "transaction-created-topic" event to Kafka
4. AntiFraudService consumes the event and processes fraud detection
5. AntiFraudService publishes a "transaction-status-updated-topic" event back to Kafka
6. TransactionService consumes the result and updates the transaction status

## Getting Started

### Prerequisites

- Docker and Docker Compose installed
- Git

### Running the Solution

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd <repository-directory>
   ```

2. Start the solution using Docker Compose:
   ```bash
   docker-compose up --build -d
   ```

3. The following services will be available:
    - TransactionService API: http://localhost:8000
    - AntiFraudService API: http://localhost:7000

### Environment Configuration

The solution uses Docker Compose to set up:
- PostgreSQL databases for each service
- Zookeeper for Kafka cluster management
- Kafka brokers for event messaging
- .NET 8.0 containers for both APIs

## Database Structure

Each microservice has its own PostgreSQL database:

- **YapeTransactionService DB**: Stores transaction data, status, and history
- **YapeAntiFraudService DB**: Stores analysis results, and detection patterns

## Kafka Topics

The main Kafka topics used for communication:

- `transaction-created-topic`: Published by TransactionService when a new transaction is created
- `transaction-status-updated-topic`: Published by AntiFraudService with the fraud analysis result

## Development

To add new features or modify the existing code:

1. Each microservice follows the Clean Architecture pattern
2. Domain models and business logic are isolated from external concerns
3. Infrastructure classes handle database and messaging interactions

## About the author
This solution was developed by Erick Orlando as part of a project to demonstrate the capabilities of .NET 8.0 and Clean Architecture in building microservices for fraud detection and transaction processing.