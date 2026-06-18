# Orders And Attendees Screens

## Screen: Orders List

### Purpose

Cho organizer/support xem orders, xử lý payment/cancellation và điều tra sự cố.

### Wireframe

```text
┌──────────────────────────────────────────────────────────────┐
│ Orders                                      [Export]         │
│ [Search name/email/order...] [Status v] [Ticket v] [Date v]  │
├──────────────────────────────────────────────────────────────┤
│ Order      Buyer              Status        Total      Created│
│ B4EF5F     Nguyen Van A       Confirmed     1,980,000  09:12 │
│ C81AA0     Tran Thi B         PaymentPending 990,000   09:14 │
└──────────────────────────────────────────────────────────────┘
```

### Controls

| Control | Behavior |
| --- | --- |
| Search | Searches buyer name, buyer email, order code. Debounce 300 ms. |
| Status filter | All, Reserved, PaymentPending, Confirmed, Cancelled, Expired, RollbackPending, ReconciliationRequired. |
| Ticket filter | Filters orders containing selected ticket type. |
| Date filter | Created date range. |
| Export | Exports filtered orders if user has export permission. |
| Row click | Opens Order Detail drawer. |

### Row Indicators

- PaymentPending with < 2 minutes left shows warning icon.
- PaymentFailed shows failure reason tooltip.
- RollbackPending/ReconciliationRequired shows payment attention badge.

## Drawer: Order Detail

### Wireframe

```text
┌──────────────── Order B4EF5F ────────────────┐
│ Status: Confirmed                             │
│ Buyer: Nguyen Van A · a@example.com           │
│ Total: 1,980,000 VND                          │
│ Payment: Succeeded · stripe_txn_456           │
│                                               │
│ Tickets                                       │
│ Nguyen Van A · Early Bird · Active            │
│ Tran Thi B · Early Bird · Active              │
│                                               │
│ Activity                                      │
│ 09:12 Order confirmed                         │
│ 09:10 Payment succeeded                       │
│ 09:05 Seats reserved                          │
│                                               │
│ [Resend confirmation] [Cancel order] [...]    │
└───────────────────────────────────────────────┘
```

### Buttons

| Button | Enabled when | Behavior |
| --- | --- | --- |
| `Resend confirmation` | Order Confirmed and has active tickets | Opens confirmation modal, then sends confirmation email and records the attempt in order activity. |
| `Cancel order` | Order cancellable by policy | Opens Cancel Order modal. |
| `Payment rollback` | Payment captured but order is not confirmed or tickets were not issued | Opens Payment Rollback modal defined in Supporting Operations. |
| `View payment` | Payment exists | Opens payment details section/drawer. |
| `Copy order link` | Always | Copies support/order lookup URL. |

### Cancel Order Modal

```text
┌──────────────── Cancel order ────────────────┐
│ Cancel order B4EF5F?                          │
│ This will invalidate 2 active tickets.        │
│                                               │
│ Payment policy                                │
│ Confirmed paid orders are final-sale.         │
│ No refund will be issued.                     │
│                                               │
│ Reason *                                      │
│ [________________________________________]    │
│                                               │
│ [Back] [Cancel order]                         │
└───────────────────────────────────────────────┘
```

Validation:

- Reason required, 5-500 chars.
- Confirmed paid cancellation requires elevated permission and records audit/activity.

## Screen: Attendees List

### Purpose

Cho organizer quản lý từng attendee/ticket, phục vụ support và onsite operations.

### Wireframe

```text
┌──────────────────────────────────────────────────────────────┐
│ Attendees                                  [Add attendee] [Export] │
│ [Search name/email/ticket...] [Ticket v] [Status v] [Check-in v] │
├──────────────────────────────────────────────────────────────┤
│ Name           Email          Ticket      Status     Check-in │
│ Nguyen Van A   a@example.com  Early Bird  Active     Not yet  │
│ Tran Thi B     b@example.com  Early Bird  Active     09:42    │
└──────────────────────────────────────────────────────────────┘
```

### Controls

| Control | Behavior |
| --- | --- |
| Search | Searches attendee name, email, ticket code. |
| Ticket filter | Filters by ticket type. |
| Status filter | Active, Cancelled, CheckedIn. |
| Check-in filter | All, Checked in, Not checked in. |
| `Add attendee` | Creates complimentary/manual attendee if organizer has permission. |
| `Export` | CSV export of current filter. |
| Row click | Opens Attendee Detail drawer. |

## Drawer: Attendee Detail

### Wireframe

```text
┌────────────── Nguyen Van A ───────────────┐
│ Active · Early Bird                       │
│ Ticket code: TCK-9F3A                     │
│ Email: a@example.com                      │
│ Company: Eventbox                          │
│ Job title: Engineer                       │
│ Check-in: Not checked in                  │
│                                           │
│ [Edit info] [Resend ticket] [Check in]    │
└───────────────────────────────────────────┘
```

### Buttons

- `Edit info`: opens edit form. Ticket type cannot be changed in MVP unless transfer flow exists.
- `Resend ticket`: sends ticket email to attendee email.
- `Check in`: available if active and not checked in; calls check-in command with organizer as operator.
- `Cancel ticket`: overflow menu; requires confirmation and policy check.

### Edit Behavior

- Editable fields: full name, email, company, job title, dietary notes.
- If email changes, show checkbox `Send updated ticket to new email`, default checked.
- Save writes audit log.

## Acceptance Criteria

- Orders and attendees lists must support pagination.
- Export respects active filters and permission.
- Cancelling a confirmed paid order invalidates all tickets but does not issue a refund.
- Cancelling a single ticket is separate from cancelling whole order and must follow policy.
