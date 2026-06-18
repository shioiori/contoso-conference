# Onsite Check-in Screens

## Screen: Check-in Home

### Purpose

Cho onsite staff chọn event, xem trạng thái check-in và bắt đầu scan/search.

### Wireframe

```text
┌──────────────────────────────────────────────────────────────┐
│ Check-in · Eventbox Tech Summit                 [Switch event]│
│ Sep 10, 2026 · Saigon Convention Center                      │
├──────────────────────────────────────────────────────────────┤
│ Checked in        Remaining         Duplicate scans           │
│ 124 / 500         376               3                         │
├──────────────────────────────────────────────────────────────┤
│ [Scan QR code] [Manual search] [Kiosk mode]                   │
├──────────────────────────────────────────────────────────────┤
│ Recent check-ins                                             │
│ 09:42 Tran Thi B · Early Bird · Gate A                        │
│ 09:41 Nguyen Van A · Standard · Gate A                        │
└──────────────────────────────────────────────────────────────┘
```

### Controls

| Control | Behavior |
| --- | --- |
| `Switch event` | Opens event selector scoped to events user can check in. |
| `Scan QR code` | Opens scanner screen and requests camera permission. |
| `Manual search` | Opens attendee search screen. |
| `Kiosk mode` | Opens self-service check-in mode if enabled. Requires supervisor permission to exit. |
| Recent row | Opens attendee check-in detail. |

### States

- No event selected: event picker is primary content.
- Camera unsupported: hide scan primary, show manual search and troubleshooting.
- Offline/degraded: show banner `Connection unstable. Check-ins may be delayed.` only if offline mode exists; otherwise block check-in.

## Screen: QR Scanner

### Purpose

Scan ticket QR quickly and give immediate admit/reject feedback.

### Wireframe

```text
┌──────────────────── Scan tickets ────────────────────────────┐
│ [Camera preview]                                              │
│ ┌──────────────────────────────┐                              │
│ │                              │                              │
│ │        QR scan area          │                              │
│ │                              │                              │
│ └──────────────────────────────┘                              │
│                                                               │
│ [Manual search] [Torch] [Pause scanner]                       │
└──────────────────────────────────────────────────────────────┘
```

### Controls

| Control | Behavior |
| --- | --- |
| Camera preview | Continuously scans QR payload. Debounce same payload for 3 seconds. |
| `Manual search` | Opens manual search without losing selected event/gate. |
| `Torch` | Toggles device torch if supported. Hidden if unsupported. |
| `Pause scanner` | Stops scanning until user resumes. Useful after an error. |

### Scan Success Result

```text
┌────────────── CHECKED IN ──────────────┐
│ Nguyen Van A                           │
│ Early Bird                             │
│ Ticket TCK-9F3A                        │
│ 09:42 · Gate A                         │
│                                        │
│ [Scan next] [View attendee]            │
└────────────────────────────────────────┘
```

Behavior:

- Success panel uses strong green visual state.
- Plays optional success sound/vibration if enabled.
- Auto returns to scanner after 2 seconds unless user taps `View attendee`.

### Rejection Result

Examples:

- `Already checked in at 09:14 by Linh Tran.`
- `Ticket cancelled. Do not admit.`
- `Ticket belongs to another event.`
- `Ticket not found.`

Wireframe:

```text
┌────────────── DO NOT ADMIT ─────────────┐
│ Already checked in                      │
│ Nguyen Van A · Early Bird               │
│ Checked in at 09:14 by Linh Tran        │
│                                        │
│ [Scan next] [Override] [View attendee]  │
└────────────────────────────────────────┘
```

Override:

- Visible only with permission.
- Requires reason.
- Creates audit log.

## Screen: Manual Search

### Purpose

Find attendee when QR is unavailable.

### Wireframe

```text
┌────────────────── Manual search ─────────────────────────────┐
│ [Search name, email, company, ticket code...]                 │
│ [Ticket type v] [Status v] [Clear]                            │
├──────────────────────────────────────────────────────────────┤
│ Nguyen Van A                                                  │
│ a@example.com · Early Bird · Not checked in                   │
│                                             [Check in]        │
│ Tran Thi B                                                    │
│ b@example.com · Early Bird · Checked in 09:42                 │
│                                             [View]            │
└──────────────────────────────────────────────────────────────┘
```

### Search Behavior

- Search input focuses automatically.
- Minimum 2 characters before query.
- Supports exact ticket code search with one-character debounce.
- Results are scoped to selected event.
- Keyboard Enter:
  - If one exact ticket code match, opens/checks in depending confirmation setting.
  - If multiple matches, focuses first result.

### Check In Button

- Enabled only for active, not checked-in attendee.
- Click opens compact confirmation if manual check-in confirmation is enabled:

```text
Check in Nguyen Van A?
[Cancel] [Check in]
```

## Screen: Kiosk Mode

### Purpose

Self-service attendee check-in for low-risk events or controlled badge pickup.

### Wireframe

```text
┌──────────────────────────────────────────────────────────────┐
│ Welcome to Eventbox Tech Summit                               │
│ Scan your QR code                                             │
│ [Large scan area]                                             │
│ Need help? Ask staff.                                         │
└──────────────────────────────────────────────────────────────┘
```

Behavior:

- Hides organizer navigation.
- Shows minimal attendee data after success.
- Auto resets after success/error.
- Exit requires staff PIN or authenticated staff action.

## Acceptance Criteria

- QR scan must be idempotent; repeated scan of same ticket cannot create duplicate check-in.
- Duplicate check-in result must clearly show original check-in time and operator.
- Manual search must work for name, email and ticket code.
- Check-in screen must remain usable on tablet width.
- Every override is audited with reason and operator.

