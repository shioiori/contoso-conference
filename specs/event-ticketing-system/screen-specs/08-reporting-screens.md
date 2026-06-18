# Reporting Screens

## Screen: Event Reports Overview

### Purpose

Cho organizer hiểu nhanh hiệu quả bán vé, doanh thu, attendance và các vấn đề cần xử lý.

### Wireframe

```text
┌──────────────────────────────────────────────────────────────┐
│ Reports                                      [Export report] │
│ [Date range v] [Ticket type v] [Refresh]                     │
├──────────────────────────────────────────────────────────────┤
│ Gross revenue   Recognized rev Tickets sold   Check-in rate  │
│ 198,000,000     190,000,000    240 / 500      52%            │
├──────────────────────────────────────────────────────────────┤
│ Sales over time                                              │
│ [line chart]                                                  │
├──────────────────────────────────────────────────────────────┤
│ Ticket breakdown                                             │
│ Early Bird   Sold 80   Reserved 5   Available 15             │
│ Standard     Sold 160  Reserved 7   Available 293            │
└──────────────────────────────────────────────────────────────┘
```

### Controls

| Control | Behavior |
| --- | --- |
| Date range | Filters metrics by order created/confirmed date depending selected report mode. |
| Ticket type | Filters all cards/charts by selected ticket type. |
| Refresh | Re-fetches report. Shows last updated timestamp. |
| Export report | Opens export modal. MVP supports CSV; XLSX/PDF are disabled options with “Coming later” tooltip. |

### Metric Definitions

- Gross revenue: sum of confirmed paid order amount.
- Recognized revenue: gross revenue minus provider fees and payment rollback/reconciliation adjustments if tracked.
- Tickets sold: active issued tickets.
- Check-in rate: checked-in active tickets / active issued tickets.
- Reserved: active reservations not yet confirmed/expired.

### Permission Behavior

- Users without financial permission see ticket and attendance metrics but not revenue.
- Export financial report requires financial export permission.

## Screen: Sales Report

### Purpose

Analyze ticket sales and payment performance.

### Wireframe

```text
┌──────────────────── Sales report ────────────────────────────┐
│ [Confirmed date v] [Date range] [Ticket type v] [Export]      │
├──────────────────────────────────────────────────────────────┤
│ Revenue chart                                                 │
│ [bar/line chart]                                              │
├──────────────────────────────────────────────────────────────┤
│ Payment status                                                │
│ Succeeded 240 · Failed 12 · Pending 8 · Rollback 3            │
├──────────────────────────────────────────────────────────────┤
│ Orders table                                                  │
│ Date      Order   Buyer          Status      Amount           │
└──────────────────────────────────────────────────────────────┘
```

### Controls

- Date mode select:
  - `Created date`
  - `Confirmed date`
  - `Payment date`
- Chart granularity:
  - Auto, Hourly, Daily, Weekly.
- Export:
  - `Summary CSV`
  - `Order detail CSV`
  - `Payment rollback detail CSV`

### Empty State

If no sales:

`No confirmed sales yet. Once attendees register, revenue and ticket sales will appear here.`

## Screen: Attendance Report

### Purpose

Track attendance and no-show.

### Wireframe

```text
┌────────────────── Attendance report ─────────────────────────┐
│ [Ticket type v] [Check-in gate v] [Export check-ins]          │
├──────────────────────────────────────────────────────────────┤
│ Checked in      Not checked in       Duplicate attempts       │
│ 124             116                  3                        │
├──────────────────────────────────────────────────────────────┤
│ Check-ins over time                                           │
│ [line chart by minute/hour]                                   │
├──────────────────────────────────────────────────────────────┤
│ Check-in log                                                  │
│ Time   Attendee       Ticket      Operator       Result        │
└──────────────────────────────────────────────────────────────┘
```

### Controls

| Control | Behavior |
| --- | --- |
| Ticket type filter | Filters attendance cards and logs. |
| Check-in gate filter | Filters by gate/device/operator group if supported. |
| Export check-ins | Downloads check-in log with current filters. |
| Check-in log row | Opens attendee detail. |

### Log Result Values

- CheckedIn
- DuplicateRejected
- OverrideCheckedIn
- InvalidTicket
- WrongEvent
- CancelledTicket

## Screen: Export Modal

### Wireframe

```text
┌──────────────── Export report ────────────────┐
│ Report type *                                  │
│ (• Sales summary) ( ) Orders ( ) Attendees     │
│                                                │
│ Format *                                       │
│ (• CSV) ( ) XLSX ( ) PDF                       │
│                                                │
│ Date range                                     │
│ [Current filters]                              │
│                                                │
│ [Cancel] [Export]                              │
└────────────────────────────────────────────────┘
```

Behavior:

- Uses current report filters by default.
- Large export runs asynchronously and appears in Exports tab/toast.
- Small export downloads immediately.
- Audit log records actor, report type, filters and timestamp.

## Acceptance Criteria

- Reports show `Last updated` timestamp.
- Revenue metrics are hidden for users without financial permission.
- Exported data must match visible filters.
- Attendance report must distinguish normal check-in from duplicate/override.
