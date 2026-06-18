# Public Discovery And Payment Settings Screens

## Screen: Public Event Listing

### Purpose

Cho attendee duyệt các Events đã publish và đi vào Event detail.

### Wireframe

```text
┌──────────────────────────────────────────────────────────────┐
│ Find events                                                   │
│ [Search Events...] [Date range v] [Format v] [Clear]    │
├──────────────────────────────────────────────────────────────┤
│ Eventbox Tech Summit 2026                                      │
│ Sep 10-12, 2026 · Saigon Convention Center · Available        │
│ Annual technology event for builders and leaders.        │
│                                            [View details]     │
│                                                              │
│ AI Workshop                                                   │
│ Oct 03, 2026 · Online · Sold out                              │
│                                            [View details]     │
└──────────────────────────────────────────────────────────────┘
```

### Controls

| Control | Behavior |
| --- | --- |
| Search Events | Searches by event name, organizer name, venue/city and summary. Debounce 300 ms. |
| Date range | Filters events whose schedule overlaps selected range. |
| Format | Values: All, In-person, Online, Hybrid. |
| Clear | Clears all filters. Disabled when no filter is active. |
| Event card | Click opens Public Event Detail. |
| View details | Opens Public Event Detail. |

### Event Card Content

- Event name.
- Date/time display using event timezone.
- Venue name or Online/Hybrid label.
- Availability status: Available, Few left, Sold out, Sales ended.
- Short summary.
- Starting price if paid tickets exist.

### States

- Loading: card skeleton.
- Empty no published events: `No public events are available right now.`
- Empty filtered: `No events match your filters.` + `Clear filters`.
- Error: inline error + `Retry`.

### Acceptance Criteria

- Draft, unpublished and archived events never appear.
- Hidden/access-code-only tickets do not affect starting price unless unlocked on detail page.
- Sold out events can still appear, but primary action remains `View details`, not `Register`.

## Screen: Event Payment Settings

### Purpose

Cho Organization Owner hoặc Finance Manager cấu hình khả năng nhận thanh toán cho một event. Đây là màn mà dashboard action `Complete payment setup` trỏ tới.

### Wireframe

```text
┌──────────────── Payment settings ────────────────────────────┐
│ Payment mode                                                  │
│ (• Simulated payment for MVP)                                 │
│ ( ) External payment provider                                 │
│                                                               │
│ Currency                                                      │
│ [VND v]                                                       │
│                                                               │
│ Final-sale policy                                             │
│ Cancellation cutoff [Sep 08, 2026] [23:59]                    │
│ Cancellation note                                             │
│ [Paid tickets are final-sale and non-refundable.]             │
│                                                               │
│ Payment status                                                │
│ ✓ Ready for paid tickets                                      │
│                                                               │
│ [Send test payment] [Save changes]                            │
└──────────────────────────────────────────────────────────────┘
```

### Controls

| Control | Behavior |
| --- | --- |
| Payment mode | MVP defaults to Simulated payment. External provider is disabled until production integration is enabled. |
| Currency | Defaults from organization settings. Changing currency is blocked once paid tickets or orders exist. |
| Cancellation cutoff | Controls attendee self-service cancellation eligibility. Must be before event end. |
| Cancellation note | Text shown on attendee cancellation screen and cancellation emails. Must state that confirmed paid tickets are final-sale and non-refundable. |
| Send test payment | Creates a non-business test run to verify payment success/failure UI. Does not create real order. |
| Save changes | Saves payment settings and updates publish readiness. |

### Readiness Rules

Payment setup is considered ready when:

- Event currency is selected.
- Payment mode is selected.
- Final-sale cancellation policy text is present if attendee cancellation is enabled.

If event only has free tickets:

- Payment setup is not blocking publish.
- Dashboard readiness shows `Payment not required`.

### Error And Validation

- Currency change blocked: `Currency cannot be changed after paid tickets or orders exist.`
- Missing cancellation note: `Cancellation note is required when attendee cancellation is enabled.`
- Invalid cutoff: `Cancellation cutoff must be before event end time.`

### Permission Behavior

- Organization Owner can edit all fields.
- Finance Manager can edit payment and final-sale cancellation settings.
- Event Manager can view payment readiness but cannot edit.
- Other roles cannot access this screen.

### Acceptance Criteria

- `Complete payment setup` from Event Dashboard opens this screen.
- Publish is not blocked by payment setup if all active tickets are free.
- Paid tickets require payment setup before publish.
- Test payment must not create real orders, tickets or attendee records.
