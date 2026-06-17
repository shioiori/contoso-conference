# Supporting Operations Screens

## Purpose

Tài liệu này gom các màn/modal phụ được nhắc trong các screen specs chính nhưng chưa có mô tả riêng. Đây là các operation không phải journey chính, nhưng nếu thiếu sẽ làm UI/action bị mồ côi.

## Modal: Duplicate Event

### Trigger

Từ Events List row overflow `...` > `Duplicate`.

### Wireframe

```text
┌──────────────── Duplicate event ────────────────┐
│ Duplicate Eventbox Tech Summit?                  │
│                                                  │
│ New event name * [Eventbox Tech Summit Copy____] │
│ New slug *       [eventbox-tech-summit-copy____] │
│                                                  │
│ Copy options                                     │
│ [x] Event setup                                  │
│ [x] Ticket types                                 │
│ [ ] Notification settings                        │
│                                                  │
│ [Cancel] [Duplicate event]                       │
└──────────────────────────────────────────────────┘
```

### Behavior

- Duplicated event is always created as Draft.
- Orders, attendees, check-ins, reports and payments are never copied.
- Ticket sold/reserved counters are reset to 0.
- New slug must be globally unique.

## Modal: Archive Event

### Trigger

From Events List row overflow `...` > `Archive`.

### Wireframe

```text
┌──────────────── Archive event ────────────────┐
│ Archive AI Workshop?                          │
│ Archived events are hidden from the default    │
│ event list. Historical reports remain visible. │
│                                                │
│ Type ARCHIVE to confirm                        │
│ [________________]                             │
│                                                │
│ [Cancel] [Archive event]                       │
└────────────────────────────────────────────────┘
```

### Behavior

- Enabled only for Draft or Unpublished events.
- Published events must be unpublished before archive.
- Archived event does not appear in public listing.
- Archive action is audit logged.

## Modal: Unpublish Event

### Trigger

Event header `Unpublish`.

### Behavior

- Requires confirmation.
- Shows impact message:
  - Public page will no longer accept registrations.
  - Existing confirmed tickets remain valid unless organizer cancels them.
  - Existing active checkout reservations may continue until expiration unless policy says otherwise.
- Requires reason if event has confirmed attendees.

## Modal: Payment Rollback

### Trigger

Order Detail drawer > `Payment rollback`.

### Wireframe

```text
┌────────────── Payment rollback ──────────────┐
│ Order B4EF5F · Captured 1,980,000 VND        │
│ Status: ReconciliationRequired               │
│                                               │
│ Fulfillment issue                             │
│ [Reservation expired v]                       │
│                                               │
│ Reason *                                      │
│ [________________________________________]    │
│                                               │
│ [Cancel] [Start rollback]                     │
└───────────────────────────────────────────────┘
```

### Behavior

- Finance Manager, Organization Owner and Platform Admin can start payment rollback/reconciliation.
- Rollback is allowed only when payment was captured but the order was not confirmed or tickets were not issued.
- Confirmed paid orders are final-sale and cannot enter this flow.
- Rollback request appears in order activity and support/audit views.

### Validation

- Reason required, 5-500 characters.
- Fulfillment issue is required.
- Rollback cannot be requested twice while rollback/reconciliation is pending.

## Modal: Add Complimentary Attendee

### Trigger

Attendees List > `Add attendee`.

### Wireframe

```text
┌──────────── Add complimentary attendee ────────────┐
│ Ticket type *       [Early Bird v]                 │
│ Full name *         [____________________]         │
│ Email *             [____________________]         │
│ Company             [____________________]         │
│ Job title           [____________________]         │
│ Reason *            [Speaker / Sponsor / Staff__] │
│                                                     │
│ [x] Send ticket email                              │
│                                                     │
│ [Cancel] [Add attendee]                            │
└─────────────────────────────────────────────────────┘
```

### Behavior

- Creates a zero-amount confirmed registration for one attendee.
- Consumes one seat from selected ticket type unless ticket type is configured as unlimited/internal.
- Sends ticket email if checkbox is selected.
- Action is audit logged with reason.

### Validation

- Ticket type must have availability.
- Full name and email required.
- Email must be valid.
- Reason required.

## Screen: Export Center

### Purpose

Cho organizer tải lại các export lớn chạy bất đồng bộ.

### Wireframe

```text
┌──────────────── Exports ───────────────────────────────┐
│ [Report type v] [Created by v] [Status v]              │
├────────────────────────────────────────────────────────┤
│ Created at       Type              Status      Action  │
│ Jun 6 10:20      Attendees CSV     Ready       Download│
│ Jun 6 10:18      Sales XLSX        Processing  --      │
│ Jun 5 15:01      Check-ins CSV     Failed      Retry   │
└────────────────────────────────────────────────────────┘
```

### Behavior

- Small exports download immediately and do not need to appear here.
- Large exports show toast `Export is being prepared. View in Exports.`
- Ready exports expire after 7 days.
- Failed exports show failure reason and `Retry` if user still has permission.

## Screen: Platform Support Console

### Purpose

Cho Platform Admin xử lý incident/support cấp cao mà không cần thao tác trực tiếp trên database.

### Wireframe

```text
┌──────────────── Platform support ───────────────────────────┐
│ [Search organization, event, order, email...]                │
├──────────────────────────────────────────────────────────────┤
│ Result type   Name / Identifier             Status           │
│ Organization  Eventbox Events                Active           │
│ Event         Eventbox Tech Summit 2026      Published        │
│ Order         B4EF5F                        Confirmed        │
└──────────────────────────────────────────────────────────────┘
```

### Controls

| Control | Behavior |
| --- | --- |
| Search | Searches organization name, event name, order code, attendee email. |
| Result row | Opens read-only support detail by default. |
| `Impersonate organizer` | Requires elevated confirmation and reason. |
| `View audit log` | Opens audit log filtered to selected entity. |
| `Lock event` | Blocks public registration for incident response. Requires reason. |

### Acceptance Criteria

- Platform Admin actions always require reason.
- Impersonation banner must be visible while active.
- Support Console never exposes raw payment secrets or card data.

## Screen: Audit Log

### Purpose

Cho authorized users xem các thay đổi quan trọng để điều tra và tuân thủ.

### Wireframe

```text
┌──────────────── Audit log ───────────────────────────────┐
│ [Actor v] [Action v] [Entity type v] [Date range v]      │
├──────────────────────────────────────────────────────────┤
│ Time        Actor        Action           Entity          │
│ 10:12       Mai Nguyen   Publish event    Eventbox Summit  │
│ 10:05       Linh Tran    Override check-in TCK-9F3A       │
└──────────────────────────────────────────────────────────┘
```

### Behavior

- Organization Owner can view organization/event audit logs.
- Event Manager can view audit log scoped to assigned event.
- Finance Manager can view financial action logs.
- Check-in Lead can view check-in action logs.
- Platform Admin can view all logs.

### Acceptance Criteria

- Audit entries are read-only.
- Filters preserve URL state for sharing with other authorized users.
- Sensitive values are masked where appropriate.

## Screen: Account And Profile

### Purpose

Cho user quản lý thông tin cá nhân tối thiểu cho organizer workspace. Đây không phải core Event workflow, nhưng là đích của Avatar menu.

### Wireframe

```text
┌──────────────── Account ───────────────────────────────┐
│ Profile                                                 │
│ Full name *        [Mai Nguyen____________]             │
│ Email              [mai@eventbox.com]                    │
│ Timezone           [Asia/Ho_Chi_Minh v]                 │
│                                                        │
│ Security                                                │
│ Password           [Change password]                    │
│ Sessions           [Sign out all other sessions]        │
│                                                        │
│ [Save changes]                                         │
└────────────────────────────────────────────────────────┘
```

### Behavior

- Email is read-only in MVP; changing email is handled by support or future account flow.
- Full name is required.
- Timezone defaults dates/times in organizer workspace when event timezone is not the main context.
- `Change password` opens existing identity-provider/password flow.
- `Sign out all other sessions` requires confirmation.

### Acceptance Criteria

- Avatar menu `Profile` opens this screen.
- `Sign out` from Avatar menu signs out current session.
- Account screen does not expose organization/team permissions; those stay in Team screens.
