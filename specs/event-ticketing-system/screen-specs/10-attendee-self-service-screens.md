# Attendee Self-Service Screens

## Screen: Find My Order

### Purpose

Cho attendee tự tìm lại order/ticket khi mất email confirmation.

### Wireframe

```text
┌──────────────── Find your order ────────────────┐
│ Enter the email used during registration.        │
│ Email * [____________________________]           │
│                                                  │
│ [Send secure link]                               │
└──────────────────────────────────────────────────┘
```

### Behavior

- User nhập email đã dùng khi đăng ký.
- System luôn hiển thị cùng một success message để tránh lộ email có tồn tại hay không:
  - `If an order exists for this email, we will send a secure link.`
- Secure link hết hạn sau 30 phút.
- Link chỉ mở orders/tickets gắn với email đó.

### Validation

- Email required.
- Email format valid.
- Rate limit theo email và IP.

## Screen: My Orders

### Purpose

Cho attendee xem các orders/tickets của chính mình từ secure link.

### Wireframe

```text
┌──────────────── My orders ──────────────────────┐
│ a@example.com                                    │
├─────────────────────────────────────────────────┤
│ Eventbox Tech Summit 2026                         │
│ Order B4EF5F · Confirmed · 2 tickets             │
│ [View tickets] [Request cancellation]            │
│                                                  │
│ AI Workshop                                      │
│ Order C81AA0 · Expired                           │
│ [View details]                                   │
└─────────────────────────────────────────────────┘
```

### Controls

| Control | Behavior |
| --- | --- |
| `View tickets` | Opens Order Ticket page. |
| `Request cancellation` | Opens cancellation request flow if event policy allows. |
| `View details` | Shows read-only order detail. |

### Visibility Rules

- Confirmed orders show tickets.
- PaymentPending orders show payment retry if reservation is still active.
- Expired orders show no payment action.
- Cancelled orders show status and no active QR.

## Screen: Attendee Ticket Page

### Purpose

Cho attendee mở ticket trên mobile để vào cổng.

### Wireframe

```text
┌──────────────── Eventbox Tech Summit ────────────────┐
│ Sep 10-12, 2026 · Saigon Convention Center          │
├─────────────────────────────────────────────────────┤
│ Nguyen Van A                                        │
│ Early Bird                                          │
│ [QR CODE]                                           │
│ Ticket code: TCK-9F3A                               │
│ Status: Active                                      │
│                                                     │
│ [Download PDF] [Add to calendar]                    │
└─────────────────────────────────────────────────────┘
```

### Behavior

- QR code is large enough for mobile scanning.
- Ticket page does not expose payment details beyond order status.
- Cancelled tickets show QR disabled with `Ticket no longer valid`.
- Checked-in tickets show `Checked in at HH:mm`.

## Screen: Request Cancellation

### Purpose

Cho attendee gửi cancellation request hoặc tự cancel nếu policy cho phép.

### Wireframe

```text
┌──────────── Request cancellation ────────────┐
│ Order B4EF5F · 2 tickets                     │
│ Cancellation policy                          │
│ You can cancel until Sep 8, 2026 23:59.      │
│ Paid tickets are final-sale. No refunds.     │
│                                              │
│ Tickets to cancel                            │
│ [x] Nguyen Van A · Early Bird                │
│ [x] Tran Thi B · Early Bird                  │
│ Reason                                       │
│ [________________________________________]   │
│                                              │
│ [Back] [Submit cancellation]                 │
└──────────────────────────────────────────────┘
```

### Behavior

- If policy allows automatic cancellation, button label is `Cancel tickets`.
- If organizer approval is required, button label is `Submit request`.
- If cutoff passed, page shows policy message and hides submit action.
- Partial cancellation is allowed only if event policy allows; MVP defaults to whole-order cancellation.

### Acceptance Criteria

- Secure link does not require account login but expires.
- Attendee can only see orders associated with verified email link.
- Cancellation request invalidates tickets only after cancellation is approved/processed.
- Self-service pages are mobile-first.
