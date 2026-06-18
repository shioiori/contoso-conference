# Information Architecture And Navigation

## Top-Level Areas

```text
Public
├─ Event listing
├─ Event detail
├─ Registration checkout
└─ Order confirmation / ticket

Organizer App
├─ Events
│  ├─ Event dashboard
│  ├─ Event setup
│  ├─ Tickets
│  ├─ Orders
│  ├─ Attendees
│  ├─ Check-in
│  ├─ Reports
│  ├─ Notifications
│  └─ Payment settings
├─ Organization settings
├─ Team and permissions
├─ Exports
├─ Audit log
└─ Account
```

## Organizer Global Navigation

Desktop layout:

```text
┌──────────────────────────────────────────────────────────────┐
│ Eventbox Events        [Search events...]      [Org] [Avatar] │
├───────────────┬──────────────────────────────────────────────┤
│ Events        │ Page content                                  │
│ Reports       │                                              │
│ Team          │                                              │
│ Settings      │                                              │
└───────────────┴──────────────────────────────────────────────┘
```

Behavior:

- `Eventbox Events` click về Events list.
- `Search events` tìm theo event name/slug, enter mở result đầu tiên nếu chỉ có một kết quả.
- `Org` mở organization switcher nếu user thuộc nhiều org.
- `Avatar` mở menu: Profile, Notification settings, Sign out.

## Event Workspace Navigation

Khi user chọn một event, sidebar chuyển sang event workspace:

```text
┌──────────────────────────────────────────────────────────────┐
│ ← Events / Eventbox Tech Summit       [Preview] [Publish]     │
├───────────────┬──────────────────────────────────────────────┤
│ Dashboard     │ Event-specific content                        │
│ Setup         │                                              │
│ Tickets       │                                              │
│ Orders        │                                              │
│ Attendees     │                                              │
│ Check-in      │                                              │
│ Reports       │                                              │
│ Notifications │                                              │
│ Payment       │                                              │
└───────────────┴──────────────────────────────────────────────┘
```

Header buttons:

- `Preview`: mở public event detail ở tab mới hoặc modal preview. Nếu event chưa publish, preview dùng draft data.
- `Publish`: chỉ hiện khi event chưa published. Click mở Publish readiness modal.
- `Unpublish`: thay thế `Publish` khi event đã published. Click mở confirmation modal.

## Permission-Based Navigation

- Organization Owner thấy tất cả tabs.
- Event Manager thấy Dashboard, Setup, Tickets, Orders, Attendees, Check-in, Reports và Notifications.
- Registration Manager thấy Dashboard, Orders và Attendees.
- Finance Manager thấy Dashboard, Orders, Reports và Payment, bao gồm financial data.
- Check-in Lead thấy Dashboard, Attendees, Check-in và Attendance report.
- Check-in Operator chỉ thấy Check-in.
- Viewer thấy Dashboard và Reports ở chế độ read-only, không thấy financial data.
- User không có financial permission không thấy revenue cards và payout reports.

## Responsive Behavior

- Desktop: sidebar cố định, content rộng.
- Tablet: sidebar collapsible, event header sticky.
- Mobile organizer: ưu tiên Check-in, Attendees, Dashboard; các màn setup phức tạp vẫn dùng được nhưng không tối ưu.
- Public checkout mobile-first.

## Cross-Screen Toasts

Success toast:

- Hiển thị trong 4 giây.
- Có thể có action phụ như `View order`, `Undo` nếu nghiệp vụ cho phép.

Error toast:

- Không auto-hide nếu là lỗi destructive/critical.
- Có `Retry` nếu action idempotent.

Examples:

- `Ticket type saved.`
- `Could not reserve seats. Only 2 Early Bird tickets are available.`
- `Attendee already checked in at 09:14 by Linh Tran.`
