# Eventbox API

Eventbox API is an event ticketing backend built with .NET 10, PostgreSQL, RabbitMQ, and an event-driven microservice layout.

## Overview

Eventbox covers the core flow for organizers publishing events and customers registering for tickets:

- Organizer accounts create organizations, events, ticket types, and ticket capacity.
- Public users discover published events and check ticket availability.
- Customers or guests create orders for an event.
- Orders reserve ticket availability, can be paid, cancelled, confirmed for free, or expired automatically.
- Payment simulation confirms or fails paid orders through RabbitMQ integration events.
- Notification consumes order confirmation events and sends email through SMTP.
- Organizers check attendees in by QR token at the door.

## Branches

| Branch | Purpose |
| --- | --- |
| `v2` | Active development branch. |
| `deploy/v2` | Staging — automatic deploy to ECS Fargate. |

## Deployment

Pushing to `deploy/v2` triggers the GitHub Actions workflow at `.github/workflows/deploy.yml`. It detects which services have changed and redeploys only those.

### Target

| | |
| --- | --- |
| Provider | AWS |
| Region | `ap-southeast-2` |
| Cluster | `eventbox-staging` (ECS Fargate) |
| Registry | ECR — one repository per service |

### Services

| Service | ECR repository | ECS service |
| --- | --- | --- |
| Auth API | `eventbox/auth-api` | `auth-api` |
| Event API | `eventbox/event-api` | `event-api` |
| Ticketing API | `eventbox/ticketing-api` | `ticketing-api` |
| Payment API | `eventbox/payment-api` | `payment-api` |
| Notification API | `eventbox/notification-api` | `notification-api` |

### Change detection

Each service is redeployed only when its source paths change:

| Service | Watched paths |
| --- | --- |
| Auth API | `Auth/**`, `Shared/**` |
| Event API | `EventManagement/**`, `Shared/**`, `EventBus/**` |
| Ticketing API | `Ticketing/**`, `Shared/**`, `EventBus/**` |
| Payment API | `Payment/**`, `Shared/**`, `EventBus/**` |
| Notification API | `Notification/**`, `Shared/**`, `EventBus/**` |

### Required secrets

| Secret | Used for |
| --- | --- |
| `AWS_ACCOUNT_ID` | ECR registry URL |
| `AWS_ACCESS_KEY_ID` | AWS credentials |
| `AWS_SECRET_ACCESS_KEY` | AWS credentials |

### Deploy flow

1. `detect-changes` job runs `dorny/paths-filter` to determine which services have changed.
2. Each service job runs only if its filter output is `true`.
3. The job builds the Docker image from the repo root context, pushes it to ECR with the `latest` tag, then calls `aws ecs update-service --force-new-deployment`.

## Services

| Service | Project | Responsibility |
| --- | --- | --- |
| YarpApiGateway | `ApiGateway/YarpApiGateway` | YARP reverse proxy for the public API surface. |
| Auth API | `Auth/Auth.API` | Customer/organizer registration, login, logout, JWT issuance, and current-user endpoint. |
| Event API | `EventManagement/Event.API` | Organizations, organizer event management, public event discovery, ticket type management, and event outbox publishing. |
| Ticketing API | `Ticketing/Ticketing.API` | Public checkout, customer orders, internal payment start endpoint, ticket availability, order expiration jobs, and QR check-in. |
| Payment API | `Payment/Payment.API` | Payment intent creation, order access verification against Ticketing, and simulated payment provider callbacks. |
| Notification API | `Notification/Notification.API` | RabbitMQ subscriber that sends transactional emails through SMTP/MailHog. |
| EventBus | `EventBus/EventBus.Core`, `EventBus/EventBus.RabbitMQ`, `EventBus/EventBus.Aws` | Shared abstractions with RabbitMQ and AWS SQS/SNS implementations for integration events. |
| Contracts | `Shared/Eventbox.Contracts` | Shared integration event contracts across services. |
| Shared | `Shared/Eventbox.Shared`, `Shared/Eventbox.Shared.Infrastructure` | Cross-cutting concerns: seedwork, exceptions, auditing, logging, repositories, and outbox support. |
