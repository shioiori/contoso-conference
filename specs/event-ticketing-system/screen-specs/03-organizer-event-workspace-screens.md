# Organizer Event Workspace Screens

## Screen: Events List

### Purpose

Cho organizer xem toàn bộ Events/events thuộc organization và tạo event mới.

### Wireframe

```text
┌──────────────────────────────────────────────────────────────┐
│ Events                                      [+ Create event] │
│ [Search events...] [Status v] [Date range v] [Clear]         │
├──────────────────────────────────────────────────────────────┤
│ Event name        Status      Date             Sold  Check-in│
│ Eventbox Summit    Published   Sep 10-12, 2026  240   0       │
│ AI Workshop       Draft       Oct 03, 2026     0     0       │
└──────────────────────────────────────────────────────────────┘
```

### Controls

| Control | Type | Behavior |
| --- | --- | --- |
| `+ Create event` | Primary button | Mở Create Event modal hoặc `/events/new`. |
| `Search events` | Text input | Search theo name, slug. Debounce 300 ms. Enter giữ nguyên filter, không submit form. |
| `Status` | Select | Values: All, Draft, Published, Unpublished, Archived. |
| `Date range` | Date range picker | Filter events có date overlap với range. |
| `Clear` | Button | Xóa toàn bộ filters. Disabled nếu không có filter. |
| Event row | Clickable row | Mở Event Dashboard. |
| Row overflow `...` | Menu | Preview, Duplicate, Archive. Archive chỉ enabled với draft/unpublished. |

### States

- Loading: table skeleton 5 rows.
- Empty no data: “No events yet” + `Create event`.
- Empty filtered: “No events match your filters” + `Clear filters`.
- Error: inline error block + `Retry`.

### Acceptance Criteria

- User không có quyền create không thấy `+ Create event`.
- Archived events không hiện mặc định.
- Search/filter state được giữ khi user quay lại từ event detail.

## Screen: Create Event

### Purpose

Tạo event nhanh với thông tin tối thiểu, sau đó chuyển sang Event Setup để hoàn thiện.

### Wireframe

```text
┌──────────────────────── Create event ────────────────────────┐
│ Event name *       [____________________________]             │
│ URL slug *         [eventbox-tech-summit-2026____]             │
│ Start date/time *  [Date] [Time]                              │
│ End date/time *    [Date] [Time]                              │
│ Timezone *         [Asia/Ho_Chi_Minh v]                       │
│ Format             (• In-person) ( ) Online ( ) Hybrid        │
│                                                              │
│                         [Cancel] [Create event]              │
└──────────────────────────────────────────────────────────────┘
```

### Inputs

| Input | Allowed values | Validation | Behavior |
| --- | --- | --- | --- |
| Event name | 3-120 chars | Required | Auto-generates slug until user edits slug manually. |
| URL slug | lowercase letters, numbers, hyphen | Required, unique | On blur checks uniqueness. Show “Available” or error. |
| Start date/time | Future or current date | Required | Uses organization timezone default. |
| End date/time | After start | Required | If earlier than start, inline error. |
| Timezone | IANA timezone | Required | Defaults to organization timezone. |
| Format | In-person, Online, Hybrid | Required | Controls fields shown later in Setup. |

### Buttons

- `Cancel`: closes modal. If dirty, show discard confirmation.
- `Create event`: validates form, disables while submitting, calls `CreateEvent`.

### Success Behavior

- Redirect to `/organizer/events/{eventId}/setup`.
- Toast: `Event created. Add details and tickets before publishing.`

### Error Behavior

- Slug conflict: inline under slug.
- Network/server error: modal-level alert, keep entered values.

## Screen: Event Dashboard

### Purpose

Cho organizer nhìn nhanh tình trạng event và các next actions.

### Wireframe

```text
┌──────────────────────────────────────────────────────────────┐
│ Eventbox Tech Summit                            [Preview]     │
│ Draft · Sep 10-12, 2026 · Asia/Ho_Chi_Minh     [Publish]     │
├──────────────────────────────────────────────────────────────┤
│ Readiness                                                    │
│ [✓ Basic info] [✓ Tickets] [! Payment] [! Publish settings]  │
├──────────────────────────────────────────────────────────────┤
│ Tickets sold     Revenue        Reserved      Check-ins      │
│ 240 / 500        198,000,000    12            0              │
├──────────────────────────────────────────────────────────────┤
│ Next actions                                                 │
│ [Complete payment setup] [Add ticket type] [Preview page]    │
├──────────────────────────────────────────────────────────────┤
│ Recent activity                                              │
│ 09:10 Seats added to Early Bird by Mai                       │
│ 08:43 Order confirmed: Nguyen Van A                          │
└──────────────────────────────────────────────────────────────┘
```

### Cards

| Card | Behavior |
| --- | --- |
| Readiness | Click item navigates to section that needs fixing. |
| Tickets sold | Click opens Tickets tab with inventory breakdown. |
| Revenue | Hidden if user lacks financial permission. |
| Reserved | Click opens Orders filtered by `Reserved/PaymentPending`. |
| Check-ins | Click opens Check-in or Attendance report. |
| Recent activity | Click row opens related object detail where available. |

### Publish Button

On click:

1. Calls readiness query.
2. If all checks pass, opens Publish confirmation modal.
3. If checks fail, opens Publish readiness modal with blocking items.

Publish readiness modal:

```text
┌──────────────── Publish event ────────────────┐
│ This event is almost ready.                    │
│                                                │
│ ✓ Basic information complete                   │
│ ✓ At least one ticket is available             │
│ ✕ Payment setup missing                        │
│ ✕ Public page has no venue/online access info  │
│                                                │
│ [Go to payment setup] [Cancel]                 │
└────────────────────────────────────────────────┘
```

If ready:

```text
┌──────────────── Publish event ────────────────┐
│ Publish Eventbox Tech Summit?                   │
│ Attendees will be able to view and register.   │
│                                                │
│ [Cancel] [Publish event]                       │
└────────────────────────────────────────────────┘
```

### Acceptance Criteria

- Dashboard must load even if reporting service is delayed; delayed cards show skeleton or “Updating”.
- Publish must be blocked if no sellable ticket exists.
- Revenue card must not leak financial data to users without permission.

## Screen: Event Setup

### Purpose

Cho organizer chỉnh public-facing details và operational settings.

### Layout

```text
┌──────────────────────────────────────────────────────────────┐
│ Event setup                                      [Save]      │
├───────────────┬──────────────────────────────────────────────┤
│ Basic info    │ Event name * [________________]              │
│ Date & venue  │ Slug *       [________________]              │
│ Public page   │ Description  [rich text editor]              │
│ Settings      │                                              │
└───────────────┴──────────────────────────────────────────────┘
```

### Sections And Controls

#### Basic info

- `Event name`: required, 3-120 chars.
- `Slug`: required, unique. Shows public URL preview.
- `Description`: rich text, supports paragraphs, bold, italic, links, bullet list.
- `Category`: optional select. Used for reporting/search later.

#### Date & venue

- `Start date`, `Start time`, `End date`, `End time`.
- `Timezone`.
- `Format`: In-person, Online, Hybrid.
- If In-person/Hybrid:
  - `Venue name`: required before publish.
  - `Venue address`: required before publish.
  - `Map URL`: optional.
- If Online/Hybrid:
  - `Online access URL`: optional before publish if hidden until order confirmation.
  - `Show online URL publicly`: toggle, default off.

#### Public page

- `Cover image`: upload/remove. Validate file type and size.
- `Summary`: short text shown in listing.
- `Contact email`: required before publish.

#### Settings

- `Require access code`: toggle.
- `Registration close time`: date/time. Default event start time.
- `Allow attendee cancellation`: toggle.
- `Cancellation cutoff`: shown if cancellation enabled.

### Save Behavior

- Sticky footer appears when dirty: `[Discard changes] [Save changes]`.
- `Save changes` validates current section and calls `UpdateEvent`.
- If event is published, save shows toast: `Event updated. Changes are now visible to attendees.`

### Validation Behavior

- Inline validation appears after blur or submit.
- Publish-only requirements are marked as warning while draft, blocking error during publish.

