# Public Registration And Checkout Screens

## Screen: Public Event Detail

### Purpose

Cho attendee xem thông tin Event, chọn vé và bắt đầu registration.

### Wireframe

```text
┌──────────────────────────────────────────────────────────────┐
│ [Cover image]                                                 │
│ Eventbox Tech Summit 2026                                      │
│ Sep 10-12, 2026 · Saigon Convention Center                    │
│ [Register]                                                    │
├──────────────────────────────────────────────────────────────┤
│ About                                                        │
│ ...description...                                             │
├──────────────────────────────────────────────────────────────┤
│ Tickets                                                      │
│ Early Bird      990,000 VND      15 left     [-] 0 [+]        │
│ Standard        1,500,000 VND    Available   [-] 0 [+]        │
│ [Access code] [Apply]                                        │
│                                            [Register]         │
└──────────────────────────────────────────────────────────────┘
```

### Controls

| Control | Behavior |
| --- | --- |
| Top `Register` | Scrolls to ticket section. If only one ticket is available, focuses quantity stepper. |
| Ticket quantity `- / +` | Increments within min/max per order and available quantity. |
| Quantity input | Allows integer only. On blur clamps to valid range or shows inline error. |
| `Access code` | Reveals hidden/code-protected tickets if valid. Case-insensitive unless configured otherwise. |
| `Apply` | Calls access-code validation. Shows unlocked ticket rows on success. |
| Bottom `Register` | Disabled until at least one ticket quantity > 0. Starts checkout. |

### Ticket Row States

- Available: quantity controls enabled.
- Few left: label `Only X left`.
- Sold out: disabled row, label `Sold out`.
- Sales start future: disabled row, label `Sales start Jun 10, 09:00`.
- Sales ended: disabled row, label `Sales ended`.
- Hidden locked: not rendered until access code succeeds.

### Behavior

- Availability shown here is informational. Final validation happens when reservation command runs.
- If availability changes between selection and checkout, checkout shows conflict and asks user to adjust quantity.
- Mobile layout keeps selected ticket summary sticky at bottom.

## Screen: Checkout Step 1 - Tickets And Buyer

### Purpose

Collect buyer information and confirm selected ticket quantities before reserving seats.

### Wireframe

```text
┌──────────────────── Register ────────────────────────────────┐
│ Step 1 of 3: Your details                                     │
│ Buyer name *      [____________________]                      │
│ Buyer email *     [____________________]                      │
│ Confirm email *   [____________________]                      │
├──────────────────────────────────────────────────────────────┤
│ Tickets                                                       │
│ Early Bird  x2                         1,980,000 VND          │
│ [Change tickets]                                              │
├──────────────────────────────────────────────────────────────┤
│ [Back]                                      [Continue]        │
└──────────────────────────────────────────────────────────────┘
```

### Inputs

| Input | Validation | Behavior |
| --- | --- | --- |
| Buyer name | Required, 2-120 chars | Used as default attendee name for first ticket. |
| Buyer email | Required, valid email | Used for confirmation and order lookup. |
| Confirm email | Must match buyer email | Paste allowed. Error shown on blur and continue. |

### Buttons

- `Back`: returns to Event detail, preserving ticket quantities.
- `Change tickets`: opens ticket selection panel, preserving buyer fields.
- `Continue`: validates buyer fields, moves to attendee details.

## Screen: Checkout Step 2 - Attendee Details

### Purpose

Collect attendee-level information for each ticket.

### Wireframe

```text
┌──────────────────── Attendees ────────────────────────────────┐
│ Ticket 1 · Early Bird                                         │
│ Full name *   [____________________]                          │
│ Email *       [____________________]                          │
│ Company       [____________________]                          │
│ Job title     [____________________]                          │
│                                                               │
│ Ticket 2 · Early Bird                                         │
│ [ ] Same as buyer                                             │
│ Full name *   [____________________]                          │
│ Email *       [____________________]                          │
│ Company       [____________________]                          │
│ Job title     [____________________]                          │
│                                                               │
│ [Back]                                      [Reserve seats]   │
└──────────────────────────────────────────────────────────────┘
```

### Controls

| Control | Behavior |
| --- | --- |
| `Same as buyer` | Copies buyer name/email/company where available. If unchecked after copy, values remain editable. |
| Attendee fields | Required fields depend on event registration form config. MVP requires full name and email. |
| `Reserve seats` | Calls registration command to create order and reserve seats. |

### Reservation Behavior

On `Reserve seats`:

1. Validate all required attendee fields.
2. Disable button and show loading: `Reserving your seats...`.
3. Call `RegisterToEvent`.
4. If success:
   - Create order.
   - Start reservation countdown.
   - Navigate to payment/review step.
5. If insufficient availability:
   - Show modal with changed availability.
   - Let user adjust quantities or return to ticket selection.

### Insufficient Availability Modal

```text
┌──────────── Tickets changed ────────────┐
│ Early Bird now has only 1 ticket left.  │
│ You selected 2.                         │
│                                         │
│ [Back to tickets] [Update to 1 ticket]  │
└─────────────────────────────────────────┘
```

## Screen: Checkout Step 3 - Review And Payment

### Purpose

Show reserved order, countdown and payment action.

### Wireframe

```text
┌──────────────────── Review & pay ────────────────────────────┐
│ Seats reserved for 14:32                                      │
│                                                               │
│ Order summary                                                 │
│ Early Bird x2                         1,980,000 VND           │
│ Fees                                  0 VND                   │
│ Total                                 1,980,000 VND           │
│                                                               │
│ Buyer: Nguyen Van A · a@example.com                           │
│ Attendees: 2                                                  │
│                                                               │
│ [Cancel order]                              [Pay now]         │
└──────────────────────────────────────────────────────────────┘
```

### Controls

| Control | Behavior |
| --- | --- |
| Countdown | Updates every second. At 2 minutes remaining, visual warning. At 0, order expires and UI moves to expired state. |
| `Cancel order` | Opens confirm modal. On confirm, cancels order and releases seats. |
| `Pay now` | Creates/uses payment intent and redirects to provider or simulated payment screen. |

### Free Order Behavior

If total = 0:

- Primary button label is `Confirm registration`.
- On click, confirm order immediately.
- Navigate to confirmation page.

### Expired State

```text
┌──────────────── Reservation expired ────────────────┐
│ Your seats were released because checkout timed out. │
│ [Start over]                                        │
└──────────────────────────────────────────────────────┘
```

## Screen: Order Confirmation / Ticket

### Purpose

Show successful registration and provide ticket access.

### Wireframe

```text
┌──────────────── Registration confirmed ──────────────────────┐
│ Order #B4EF5F                                                 │
│ Confirmation sent to a@example.com                            │
├──────────────────────────────────────────────────────────────┤
│ Ticket: Nguyen Van A                                          │
│ Early Bird                                                    │
│ [QR CODE]                                                     │
│ [Download ticket] [Add to calendar]                           │
├──────────────────────────────────────────────────────────────┤
│ Ticket: Tran Thi B                                            │
│ Early Bird                                                    │
│ [QR CODE]                                                     │
│ [Download ticket]                                             │
└──────────────────────────────────────────────────────────────┘
```

### Controls

- `Download ticket`: downloads PDF/pass for one ticket.
- `Add to calendar`: downloads `.ics` or opens calendar integration.
- `Resend confirmation`: visible if user opens order lookup page later.

### Acceptance Criteria

- Confirmation page must not show payment controls.
- QR payload must be unique per ticket.
- Refreshing page must not duplicate order confirmation.

