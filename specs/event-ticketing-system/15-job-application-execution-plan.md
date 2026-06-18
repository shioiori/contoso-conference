# Job Application Execution Plan

## Goal

Prepare this project and the missing JD knowledge so the candidate can apply for the Backend Developer (.NET) role with a strong project story around .NET, REST APIs, SQL/EF, DDD, EDA, microservices, AWS familiarity, testing, and ownership.

Total timeline:

- 2 to 2.5 weeks: complete the project to portfolio-ready MVP.
- 1.5 to 2 weeks: review and practice the missing JD topics.

## Part 1: Project Completion Plan

### Day 1: Stabilize Scope And Architecture

Deliverables:

- Confirm MVP slice:
  - Event Management.
  - Ticket inventory.
  - Registration and reservation.
  - Payment callback simulation.
  - Ticket issuance.
  - Basic check-in.
  - Sales/attendance summary.
- Fix solution/project references if needed.
- Add architecture docs to README links.
- Create issue checklist or local task board.

Definition of done:

- You can explain service boundaries in 3 minutes.
- The solution restore/build problem list is known.
- MVP scope is frozen for this job application cycle.

### Days 2-3: Event And Ticket Management

Deliverables:

- Complete Event API CRUD and publish/unpublish rules.
- Complete TicketType create/update/add seats rules.
- Publish integration events for Event and ticket inventory changes.
- Add unit tests for publish and quota rules.

Definition of done:

- Organizer can create Event, create ticket type and publish.
- Invalid publish and invalid quota changes are rejected.
- Event contracts exist for downstream Registration.

### Days 4-6: Registration Core And Seat Reservation

Deliverables:

- Implement Order and SeatAvailability aggregates.
- Implement start registration command.
- Reserve seats atomically.
- Add reservation expiration.
- Add order lookup endpoint.
- Add concurrency tests for oversell prevention.

Definition of done:

- Attendee can start registration and receive reservation expiration time.
- Concurrent reservation test proves no oversell.
- Order state transitions are tested.

### Days 7-8: Payment Boundary And Ticket Issuance

Deliverables:

- Implement simulated payment callback endpoint.
- Verify amount/currency against order.
- Add idempotency by payment provider event id.
- Confirm free orders without payment.
- Issue ticket codes after confirmation.
- Publish OrderConfirmed and TicketIssued events.

Definition of done:

- Free and paid confirmation paths work.
- Duplicate payment callback does not duplicate tickets.
- Payment mismatch is rejected and logged.

### Days 9-10: Check-in, Attendee List And Reporting

Deliverables:

- Add attendee list endpoint with pagination/filter basics.
- Add QR/ticket-code check-in endpoint.
- Prevent duplicate check-in.
- Add sales and attendance summary.
- Add audit records for critical operations if time allows.

Definition of done:

- Organizer can list attendees.
- Operator can check in a valid ticket.
- Duplicate check-in test passes.
- Sales and attendance summary returns meaningful data.

### Days 11-12: Reliability, Observability And API Quality

Deliverables:

- Add structured logging and correlation id middleware.
- Add consistent error responses.
- Add event outbox/inbox if feasible; otherwise document the planned pattern and implement idempotency on the most critical consumers.
- Add basic OpenAPI examples.
- Add health checks.

Definition of done:

- Critical flows can be traced by correlation id.
- API failures are readable and support-friendly.
- Health endpoint reports app/database/broker status.

### Days 13-14: Docker, README And Portfolio Polish

Deliverables:

- Ensure Docker compose starts APIs, database and broker.
- Update README with:
  - Project summary.
  - Architecture diagram links.
  - How to run locally.
  - Main API workflows.
  - Testing command.
  - DDD/EDA/AWS highlights.
- Add sample requests.
- Run full build/test.

Definition of done:

- Fresh local run is documented.
- README tells the same story you want to tell in interview.
- Project is ready to mention in CV if core tests pass.

### Optional Days 15-17: Extra Buffer For 2.5-Week Version

Use only if core MVP is already working.

Priority options:

- Add Identity/Auth skeleton with organization-scoped authorization.
- Add CSV export to S3-compatible local storage or file abstraction.
- Add outbox worker fully.
- Add more performance/concurrency tests.
- Add AWS deployment diagram and Terraform/CDK skeleton only if it will not distract from backend quality.

Definition of done:

- The extra work strengthens JD alignment without breaking the core demo.

## Part 2: JD Review Plan

### Days 15-16 Or 18-19: DDD Review

Topics:

- Entity vs Value Object.
- Aggregate root and invariants.
- Repository role.
- Domain event vs integration event.
- Bounded context and ubiquitous language.

Practice:

- Explain why Order, SeatAvailability and Ticket are separate aggregates.
- Explain where business validation should live.
- Review your project code and remove anemic domain behavior where practical.

Output:

- 1-page DDD notes using your project examples.
- 5 interview answers in English.

### Days 17-18 Or 20-21: EDA And Microservices Review

Topics:

- Event-driven architecture.
- Outbox/inbox.
- Idempotency.
- Retry, backoff and dead-letter queues.
- Eventual consistency.
- Service boundary and data ownership.

Practice:

- Draw the flow: EventPublished -> TicketTypeCreated -> SeatsReserved -> PaymentConfirmed -> OrderConfirmed -> TicketIssued.
- Explain why notification/reporting are async.
- Explain how to recover from failed consumers.

Output:

- Event catalog can be explained without reading notes.
- 3 failure scenarios and recovery answers.

### Days 19-20 Or 22-23: AWS Review

Topics:

- ECS Fargate.
- RDS PostgreSQL.
- SQS.
- EventBridge.
- S3.
- CloudWatch.
- Secrets Manager.
- API Gateway or ALB.
- Cognito basics.

Practice:

- Map each local component to AWS.
- Explain how logs, metrics, secrets and retries work.
- Explain why SQS/EventBridge can replace RabbitMQ behind an abstraction.

Output:

- AWS architecture explanation in 5 minutes.
- One diagram or text flow in the README/spec.

### Days 21-22 Or 24-25: REST API, Security And EF Review

Topics:

- REST resource design.
- Status codes and error response.
- Authentication vs authorization.
- Organization/Event scoped permissions.
- EF Core transactions, indexes and concurrency tokens.
- SQL performance basics.

Practice:

- Review API routes and naming.
- Explain how to prevent oversell at database/application level.
- Explain how to protect payment callback and attendee PII.

Output:

- 10 likely interview questions answered.
- API improvements logged as TODOs or implemented.

### Days 23-24 Or 26-27: Interview And CV Prep

Tasks:

- Write project CV bullet points.
- Prepare 60-second and 5-minute project explanation.
- Prepare English answers for:
  - Tell me about yourself.
  - Tell me about this project.
  - A difficult bug/design problem.
  - How you use AI at work.
  - Why Creative Force.
- Do one mock interview pass.

Output:

- CV-ready project section.
- Project story sounds natural in English.
- You can connect your Next.js/.NET web experience to their SaaS product environment.

## Daily Rhythm

Recommended daily blocks:

- 30 minutes: read/review notes.
- 2 to 4 hours: implement project deliverables.
- 30 minutes: write what changed and what you learned.
- 15 minutes: prepare one interview explanation from the day's work.

## Minimum Apply-Ready Bar

Apply when these are true:

- You can run the project locally.
- One end-to-end registration flow works.
- Tests cover no-oversell and payment idempotency.
- README and specs explain DDD, EDA and AWS mapping.
- You can explain the architecture in English without memorizing.

## CV Positioning

Suggested project title:

- Event Ticketing SaaS Backend.

Suggested CV bullet:

- Built a .NET backend system for event registration using DDD, CQRS-style application commands, REST APIs and event-driven integration between Event, registration, payment, notification and reporting contexts.
- Designed concurrency-safe seat reservation, idempotent payment confirmation, ticket issuance and check-in workflows with EF Core persistence, Docker-based local infrastructure and RabbitMQ messaging.
- Documented AWS deployment mapping using ECS Fargate, RDS PostgreSQL, SQS/EventBridge, S3, CloudWatch and Secrets Manager.
