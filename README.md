## Description

Web API built with .NET Core 3.1, using SQL Server **(M1 Mac please change to use Azure SQL Edge)**

Swagger is added

http://localhost:10297/swagger

## Requirements

Docker

## Build Containers

`docker compose build`

## Run Containers

`docker compose up -d`

Then run `setup.sql` to create database and tables

## To be Enhanced

- Logging
- Unit test
- SoapUI
- Deploy Kubenetes ReplicaSet
- Move database to more dedicated services, either managed DB, or separated VM; not just inside container

## Answers to Questions

### Could you please describe your ideal strategy to handle interservice communications in a microservice environment, especially when hosted in the cloud?

Microservices should be decoupled, no need to wait for others' response. Therefore use asynchronous way for communications is preferred, this can be message queue, pub/sub, or any other event-based mechanism.

Failure is expected to occur, business logic needs to take care of it, how to retry failed request, or when to revert partially done transaction. Make it be eventually consistent.

### What could be the consequences of not adequately handling interservice communications in a microservice environment?

The biggest issue could be affecting consistency, making the state incorrect, having direct impact to business. Without proper design of interservice communications may also degrade performance; reduce maintainability because of difficulty to upgrade or scaling.
