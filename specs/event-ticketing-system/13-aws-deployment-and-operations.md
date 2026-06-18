# AWS Deployment And Operations

## AWS Goal

This document maps the system to AWS services at a practical level. The goal is to show familiarity with cloud architecture, deployment, reliability and observability without making AWS the center of the project.

## Target AWS Architecture

### Edge And API

- CloudFront serves public static assets and can cache public Event pages.
- WAF protects public APIs from common attacks and abusive traffic.
- API Gateway or Application Load Balancer routes traffic to backend services.
- Route53 manages DNS records.

### Compute

- ECS Fargate runs .NET services:
  - Event API.
  - Registration API.
  - Payment Adapter.
  - Notification Worker.
  - Reporting Worker.
  - Check-in API.
- Lambda can be used for small event handlers, scheduled cleanup or export finalization.

### Data

- RDS PostgreSQL stores transactional service data for the application services.
- Separate databases or schemas are used per service boundary.
- ElastiCache stores frequently read public availability snapshots.
- OpenSearch can support attendee search, audit search or operational support queries.
- S3 stores CSV exports, generated badges, report files and archived artifacts.

### Messaging

- EventBridge routes integration events across bounded contexts.
- SQS queues isolate each consumer and support retries/dead-letter queues.
- SNS can fan out notification request events.
- For local development, RabbitMQ remains the broker behind the event bus abstraction.

### Security

- Cognito or IdentityServer handles user authentication.
- Organizer authorization is scoped by organization and Event.
- KMS encrypts secrets and sensitive storage.
- Secrets Manager or Systems Manager Parameter Store stores connection strings and provider secrets.
- CloudTrail tracks administrative actions.
- Inspector scans container images and runtime risks.

### Observability

- CloudWatch Logs collects structured application logs.
- CloudWatch Metrics tracks service health and business KPIs.
- CloudWatch Alarms notify on failure thresholds.
- Distributed tracing should include correlation id across REST calls and event handlers.

## Environment Strategy

### Local

- Docker compose.
- PostgreSQL.
- RabbitMQ.
- Local secrets through user secrets or environment variables.

### Staging

- ECS Fargate.
- RDS.
- SQS/EventBridge.
- Minimal production-like observability.
- Seed data and simulated payment provider.

### Production-Like Portfolio Demo

For CV/demo purposes, full production is not required. The project should still document:

- How the services would be deployed.
- How secrets are configured.
- How logs and metrics are collected.
- How failed events are retried.
- How database backups and restore would work.

## Deployment Pipeline

Minimum pipeline:

- Restore and build.
- Run unit tests.
- Run integration tests with test containers or docker compose.
- Build container images.
- Run basic vulnerability scan.
- Push images to registry.
- Deploy to ECS staging.

Optional:

- Database migration approval step.
- Smoke tests after deployment.
- Performance smoke test for registration endpoint.

## Operational Runbooks

### Reservation Failure Spike

Signals:

- Reservation failure metric increases.
- Registration p95 latency increases.
- SeatAvailability concurrency conflicts increase.

Actions:

- Check database CPU/locks.
- Check hot ticket type and current quota.
- Review recent deployment.
- Temporarily reduce public cache duration if availability is stale.

### Payment Callback Failure

Signals:

- Payment callback signature failures.
- Payment mismatch alerts.
- Callback retry queue grows.

Actions:

- Verify provider secret.
- Check provider event id idempotency table.
- Inspect mismatched amount/currency.
- Route unsafe confirmations to support review.

### Event Consumer Backlog

Signals:

- SQS queue age increases.
- Dead-letter queue receives messages.
- Reporting data delay increases.

Actions:

- Scale ECS worker.
- Inspect failed message reason.
- Replay messages after fixing handler issue.
- Confirm idempotency before replay.

## AWS Services To Learn For This Project

Priority 1:

- ECS Fargate.
- RDS PostgreSQL.
- SQS.
- EventBridge.
- CloudWatch.
- Secrets Manager.

Priority 2:

- API Gateway or ALB.
- S3.
- ElastiCache.
- Cognito.
- WAF.

Priority 3:

- OpenSearch.
- Lambda.
- CloudTrail.
- Inspector.
- KMS.

## Interview Talking Points

- Local RabbitMQ and AWS EventBridge/SQS can share the same event bus abstraction.
- ECS Fargate is a good fit for .NET APIs and workers when Kubernetes is unnecessary.
- SQS per consumer improves isolation and retry control.
- CloudWatch business metrics are as important as CPU/memory metrics for checkout systems.
