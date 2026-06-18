# Ticket Inventory And Pricing Screens

## Screen: Tickets Overview

### Purpose

Cho organizer quản lý ticket types, quota, price, sales window và availability.

Sales window là khoảng thời gian một ticket type được phép bán. Organizer có thể tạo nhiều ticket types với sales windows khác nhau để vận hành nhiều giai đoạn bán vé, ví dụ `Early Bird`, `Regular` và `Last Minute`.

### Wireframe

```text
┌──────────────────────────────────────────────────────────────┐
│ Tickets                                      [+ Add ticket]  │
│ [All statuses v] [Sales channel v] [Search tickets...]       │
├──────────────────────────────────────────────────────────────┤
│ Ticket       Status   Price       Sold  Reserved  Available │
│ Early Bird   On sale  990,000 VND 80    5         15        │
│ Standard     Hidden   1,500,000   0     0         300       │
└──────────────────────────────────────────────────────────────┘
```

### Controls

| Control | Type | Behavior |
| --- | --- | --- |
| `+ Add ticket` | Primary button | Opens Add Ticket drawer. |
| Status filter | Select | All, Draft, Scheduled, On sale, Sold out, Sales ended, Hidden. |
| Search tickets | Text input | Filters by ticket name. |
| Ticket row | Clickable | Opens Ticket Detail drawer. |
| Row `...` | Menu | Edit, Duplicate, Hide/Show, Add seats, End sales, Delete. |

### Status Rules

- `Draft`: not sellable, not visible.
- `Scheduled`: sales window starts in future.
- `On sale`: visible and within sales window with availability.
- `Sold out`: available = 0.
- `Sales ended`: current time after sales end.
- `Hidden`: not public but can be unlocked by code/internal.

### Empty State

Message: `No tickets yet. Add at least one ticket before publishing this event.`

Action: `Add ticket`.

## Drawer: Add/Edit Ticket

### Wireframe

```text
┌──────────────── Add ticket ────────────────┐
│ Ticket name *       [Early Bird________]   │
│ Description         [__________________]   │
│ Type                (• Paid) ( ) Free      │
│ Price *             [990000] [VND v]       │
│ Quantity *          [100____]              │
│ Sales start         [Date] [Time]          │
│ Sales end           [Date] [Time]          │
│ Min per order       [-] 1 [+]              │
│ Max per order       [-] 4 [+]              │
│ Visibility          [x] Show publicly      │
│ Access code         [__________]           │
│                                           │
│ [Cancel] [Save ticket]                    │
└───────────────────────────────────────────┘
```

### Inputs

| Input | Behavior |
| --- | --- |
| Ticket name | Required, 2-80 chars. Must be unique within event. |
| Description | Optional, shown on public registration page. |
| Type | Paid shows price/currency. Free sets price to 0 and disables price field. |
| Price | Required for paid ticket, numeric, >= 0. Supports decimal if currency allows. |
| Currency | Defaults to event currency. Changing currency warns if other tickets use different currency. |
| Quantity | Required, integer >= 0. For edit, cannot be lower than sold + active reserved. |
| Sales start | Optional. Empty means available after publish. |
| Sales end | Optional. Must be after sales start and before or equal registration close time if configured. |
| Min per order | Integer >= 1. |
| Max per order | Integer >= min per order. |
| Show publicly | If off, ticket hidden unless access code/internal link reveals it. |
| Access code | Optional. If filled, public registration requires code to reveal ticket. |

### Buttons

- `Cancel`: closes drawer; if dirty, confirm discard.
- `Save ticket`: validates and calls `CreateSeatType` or `UpdateTicketType`.
- `Delete ticket`: edit mode only. Enabled only if sold + reserved = 0.

### Validation Messages

- `Ticket name is required.`
- `Quantity cannot be lower than 85 because 80 are sold and 5 are reserved.`
- `Sales end must be after sales start.`
- `Maximum per order must be greater than or equal to minimum per order.`

## Modal: Add Seats

### Purpose

Tăng quota cho ticket type mà không chỉnh trực tiếp số tổng nếu hệ thống muốn audit rõ hành động add capacity.

```text
┌──────────────── Add seats ────────────────┐
│ Early Bird currently has:                 │
│ Sold: 80 · Reserved: 5 · Available: 15    │
│                                           │
│ Seats to add * [____]                     │
│ New total quota: 100 + seats to add       │
│                                           │
│ [Cancel] [Add seats]                      │
└───────────────────────────────────────────┘
```

Behavior:

- Input accepts positive integer only.
- `Add seats` disabled until valid.
- On submit, calls `AddSeats`.
- Success toast: `Seats added to Early Bird.`
- The tickets table updates `Available` immediately after success.

## Modal: End Sales

Purpose: Stop selling a ticket without deleting it.

Behavior:

- Requires confirmation text if ticket has active availability.
- Updates sales end to now or marks status as ended.
- Existing orders/tickets are unaffected.

## Acceptance Criteria

- Organizer cannot publish event without at least one ticket with valid quota and sellable settings.
- Hidden tickets do not appear on public page unless unlocked.
- Add seats must not reset sold/reserved counters.
- All ticket changes are audit logged with actor and timestamp.
