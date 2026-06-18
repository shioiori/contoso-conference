# Actors And Use Cases

## Actors

### Platform Admin

Quản trị toàn hệ thống, cấu hình platform, xem audit, xử lý incident và tenant.

### Organizer Owner

Chủ sở hữu event/organization. Có quyền tạo event, publish, cấu hình ticket và xem báo cáo tài chính.

### Organizer Staff

Thành viên vận hành trong organization. Trong hệ thống thật, staff không phải một role duy nhất mà được gán role cụ thể như Event Manager, Registration Manager, Finance Manager hoặc Check-in Operator. Chi tiết quyền nằm trong [Roles And Permissions](./03-roles-and-permissions.md).

### Attendee

Người tham dự. Có thể xem event đã publish, chọn vé, đăng ký, thanh toán, nhận confirmation và check-in.

### Payment Provider

Hệ thống bên ngoài gửi kết quả payment: succeeded, failed, expired, voided/reversed.

### Notification Provider

Hệ thống email/SMS/push dùng để gửi confirmation, cancellation, reminder.

### Check-in Operator

Nhân sự tại địa điểm sự kiện, dùng QR hoặc manual lookup để check-in attendee.

## Primary Use Cases

### UC-001 Create Event

Organizer tạo event với name, slug, description, start/end date, timezone, venue/online info, visibility mặc định là private.

### UC-002 Configure Ticket Types

Organizer tạo ticket type/seat type với name, quota, price, currency, sales window, visibility, purchase limit.

### UC-003 Publish Event

Organizer publish event khi thông tin hợp lệ và có ít nhất một ticket type bán được.

### UC-004 Browse Published Event

Attendee xem thông tin event và ticket availability.

### UC-005 Register For Event

Attendee chọn ticket quantity, nhập attendee details, tạo order và seat reservation có expiration.

### UC-006 Complete Payment

Attendee thanh toán qua provider. Hệ thống nhận callback/webhook và confirm order.

### UC-007 Cancel Order

Attendee hoặc organizer cancel order tùy theo policy. Seat được release nếu order chưa confirmed. Confirmed paid orders là final-sale; cancellation nếu được phép chỉ invalidates tickets và không hoàn tiền.

### UC-008 Manage Attendees

Organizer xem, lọc, sửa thông tin attendee, resend confirmation, export danh sách.

### UC-009 Check In Attendee

Operator scan QR hoặc manual search. Hệ thống validate ticket và đánh dấu checked-in.

### UC-010 View Reports

Organizer xem sold/reserved/available seats, gross revenue, recognized revenue, payment rollback/reconciliation adjustments, attendance, cancellations.

## Permission Model

Hệ thống dùng role-based access control theo scope. Role là gói quyền mặc định cho business user; khi kiểm tra quyền, system luôn xét cả role và scope của người dùng. Chi tiết role, scope và permission matrix nằm trong [Roles And Permissions](./03-roles-and-permissions.md).
