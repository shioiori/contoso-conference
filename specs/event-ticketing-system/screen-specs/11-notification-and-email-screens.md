# Notification And Email Screens

## Scope

MVP không cần full marketing automation, nhưng cần các notification transactional quan trọng để vận hành event tin cậy.

## Screen: Event Notification Settings

### Purpose

Cho organizer cấu hình các email transactional ở mức event.

### Wireframe

```text
┌──────────────── Notifications ───────────────────────────────┐
│ Sender name        [Eventbox Events________]                  │
│ Reply-to email     [events@eventbox.com____]                  │
│                                                              │
│ Transactional emails                                         │
│ [x] Order confirmation                                       │
│ [x] Ticket resend                                            │
│ [x] Order cancellation                                       │
│ [x] Payment rollback alert                                   │
│ [x] Event reminder                                           │
│                                                              │
│ Reminder timing                                              │
│ [24 hours before event v]                                    │
│                                                              │
│ [Send test email] [Save changes]                             │
└──────────────────────────────────────────────────────────────┘
```

### Controls

| Control | Behavior |
| --- | --- |
| Sender name | Required. Defaults to organization name. |
| Reply-to email | Required, valid email. Defaults to organization contact email. |
| Order confirmation | Required in MVP, cannot be disabled. |
| Ticket resend | Required in MVP, cannot be disabled. |
| Order cancellation | Required if cancellation enabled. |
| Payment rollback alert | Required for organizer/support when captured payment cannot produce tickets. |
| Event reminder | Optional toggle. |
| Reminder timing | Values: 1 hour, 6 hours, 24 hours, 3 days before event. |
| Send test email | Opens modal to enter recipient email. |
| Save changes | Saves notification settings. |

## Modal: Send Test Email

```text
┌──────────────── Send test email ────────────────┐
│ Template             [Order confirmation v]      │
│ Recipient email *    [____________________]      │
│                                                   │
│ [Cancel] [Send test]                              │
└───────────────────────────────────────────────────┘
```

Behavior:

- Sends test using sample order/ticket data.
- Does not create real order or ticket.
- Shows success toast: `Test email sent.`

## Transactional Email Requirements

### Order Confirmation Email

Sent when:

- Free order is confirmed.
- Paid order payment succeeds and order is confirmed.

Must include:

- Event name, date/time, timezone.
- Venue/online access instructions according to event setting.
- Buyer/order summary.
- Attendee ticket links.
- Support/contact email.

### Ticket Resend Email

Sent when:

- Organizer clicks `Resend confirmation` or `Resend ticket`.
- Attendee requests secure order link.

Must include:

- Secure ticket/order link.
- Expiration behavior if link is temporary.

### Cancellation Email

Sent when:

- Order/ticket is cancelled.

Must include:

- Cancelled tickets.
- Final-sale/no-refund policy text for confirmed paid orders.
- Contact email.

### Payment Rollback Alert

Sent when:

- Payment is captured but order confirmation or ticket issuance fails.

Must include:

- Order id, payment id, provider reference and fulfillment failure reason.
- Current rollback/reconciliation status.
- Link to support/order detail.

### Reminder Email

Sent when:

- Event reminder is enabled and attendee has active ticket.

Must include:

- Event date/time.
- Venue/online access.
- Ticket link.
- Check-in instructions.

## Delivery States

Organizer-facing delivery status values:

- Queued
- Sent
- Delivered
- Failed
- Suppressed

Failed delivery behavior:

- Show failure in attendee/order activity.
- Allow organizer to resend.
- Do not retry indefinitely; max retry policy is implementation detail.

## Acceptance Criteria

- Confirmation email cannot be disabled in MVP.
- Test email must not create business data.
- Resend actions are audit logged.
- Attendee order activity shows notification attempts relevant to that attendee/order.
