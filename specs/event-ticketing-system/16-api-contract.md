# Implemented API Contract

This document records the API surface implemented by the current codebase. It intentionally excludes planned product routes that do not have controllers/endpoints yet.

## Base URLs

Local `dotnet run` defaults:

| Service | URL |
| --- | --- |
| Auth API | `http://localhost:5104` |
| Event API | `http://localhost:5145` |
| Ticketing API | `http://localhost:5151` |
| Payment API | `http://localhost:5193` |

Docker compose defaults:

| Service | URL |
| --- | --- |
| Auth API | `http://localhost:8082` |
| Ticketing API | `http://localhost:5151` |
| Payment API | `http://localhost:5193` |

`event.api` publishes its container ports dynamically in the current compose file. Use `docker compose ps` and set `eventApiUrl` in Postman to the assigned host URL.

## Auth API

| Method | Route | Auth | Body |
| --- | --- | --- | --- |
| `POST` | `/api/auth/customers/register` | Public | `email`, `password`, `name` |
| `POST` | `/api/auth/organizers/register` | Public | `email`, `password`, `name` |
| `POST` | `/api/auth/login` | Public | `email`, `password`, `keepLoggedIn` |
| `GET` | `/api/auth/me` | Bearer | none |

Successful auth responses return:

```json
{
  "accessToken": "jwt",
  "expiresAt": "2026-09-15T09:00:00+00:00"
}
```

JWTs include `account_type` as `Customer` or `Organizer`.

## Event API

Public routes:

| Method | Route | Query |
| --- | --- | --- |
| `GET` | `/api/public/events` | `status`, `q`, `dateFrom`, `dateTo`, `startDate`, `endDate`, `page`, `pageSize` |
| `GET` | `/api/public/events/{slug}` | `accessCode` |

Organizer routes require `account_type = Organizer`.

| Method | Route | Body |
| --- | --- | --- |
| `POST` | `/api/organizations` | `id` optional, `name` |
| `GET` | `/api/organizations/{id}` | none |
| `PUT` | `/api/organizations/{id}` | `id` optional, `name` |
| `DELETE` | `/api/organizations/{id}` | none |
| `GET` | `/api/organizations/{organizationId}/events` | query: `status`, `q`, `dateFrom`, `dateTo`, `page`, `pageSize` |
| `POST` | `/api/organizations/{organizationId}/events` | `name`, `slug`, `from`, `to`, `summary` optional, `description` optional |
| `GET` | `/api/organizations/{organizationId}/events/{id}` | none |
| `PUT` | `/api/organizations/{organizationId}/events/{id}` | `name`, `from`, `to`, `summary` optional, `description` optional |
| `GET` | `/api/organizations/{organizationId}/events/{id}/publish-readiness` | none |
| `POST` | `/api/organizations/{organizationId}/events/{id}/publish` | `isPublished` is accepted but ignored by the current action |
| `POST` | `/api/organizations/{organizationId}/events/{id}/unpublish` | `isPublished` is accepted but ignored by the current action |
| `GET` | `/api/organizations/{organizationId}/events/{eventId}/ticket-types` | none |
| `POST` | `/api/organizations/{organizationId}/events/{eventId}/ticket-types` | `name`, `description`, `quota`, `currency`, `minPerOrder`, `maxPerOrder`, `visibility`, `accessCodeHash`, `pricingPhases` |
| `PATCH` | `/api/organizations/{organizationId}/events/{eventId}/ticket-types/{ticketTypeId}` | none in current code; returns the ticket type |
| `POST` | `/api/organizations/{organizationId}/events/{eventId}/ticket-types/{ticketTypeId}/capacity` | `quantity` |
| `GET` | `/api/organizations/{organizationId}/events/{eventId}/ticket-types/{ticketTypeId}/availability` | none |

`visibility` is currently a numeric enum in JSON request bodies: `0 = Public`, `1 = Hidden`, `2 = AccessCode`.

## Ticketing API

Public routes:

| Method | Route | Auth | Body or query |
| --- | --- | --- | --- |
| `GET` | `/api/public/events/{eventId}/ticket-availability` | Public | none |
| `POST` | `/api/public/events/{eventId}/orders` | Public or customer bearer | `name`, `email` for guest checkout, `ticketTypeId`, `quantity`, `accessCode` optional |
| `POST` | `/api/public/order-lookup-requests` | Public | none; placeholder returns `202 Accepted` |
| `GET` | `/api/public/self-service/orders` | Public | query `token` |

Customer routes require `account_type = Customer`.

| Method | Route | Body or query |
| --- | --- | --- |
| `GET` | `/api/customer/orders` | none |
| `GET` | `/api/customer/orders/by-email` | query `email` |
| `GET` | `/api/customer/orders/{orderId}` | none |
| `POST` | `/api/customer/orders/{orderId}/confirm-free` | none |
| `POST` | `/api/customer/orders/{orderId}/cancel` | none |

Organizer check-in requires `account_type = Organizer`.

| Method | Route | Body |
| --- | --- | --- |
| `POST` | `/api/events/{eventId}/check-ins/` | `qrToken` |

## Payment API

| Method | Route | Auth | Body and headers |
| --- | --- | --- | --- |
| `POST` | `/api/payments/intents` | Bearer forwarded to Ticketing API, or `orderAccessCode` in body | Header `Idempotency-Key` optional. Body: `orderId`, `returnUrl`, `cancelUrl`, `amount`, `currency`, `orderAccessCode`. |
| `POST` | `/api/payments/simulated-callbacks` | Header secret | Header `X-Provider-Signature`. Body: `providerEventId`, `paymentIntentId`, `orderId`, `status`, `amount`, `currency`, `paidAt`, `failedAt`, `failureReason`. |

Callback `status` accepts `Succeeded` or `Failed`.

## Notes

- The current Postman collection at `postman/Event.API.postman_collection.json` mirrors this implemented contract.
- OpenAPI is mapped for Auth and Event APIs in development; Ticketing and Payment currently rely on this spec plus Postman examples.
- Planned routes for attendee management, reporting exports, notification settings, team roles, and platform support are still product/backlog scope unless code is added.
