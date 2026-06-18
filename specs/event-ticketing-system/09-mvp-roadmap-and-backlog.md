# MVP, Roadmap, And Backlog

## MVP Goal

Cho phép organizer tạo event, cấu hình ticket, publish, attendee đăng ký/thanh toán giả lập hoặc qua payment adapter đơn giản, nhận ticket và check-in tại event. MVP bắt buộc không được oversell seat.

## MVP Scope

### Must Have

- Create/update/publish event.
- Create ticket type và add seats.
- Public event detail.
- Register order với seat reservation.
- Reservation expiration.
- Free order confirmation.
- Paid order confirmation qua internal/payment callback endpoint.
- Ticket issuance với ticket code.
- Organizer attendee list.
- QR/manual check-in.
- Sales/attendance summary.
- Team roles and permissions for organizer workspace.
- Transactional confirmation/resend/cancellation emails.

### Should Have

- Email confirmation integration.
- CSV attendee export.
- Payment rollback/reconciliation handling when money is captured but tickets cannot be issued.
- Basic audit log.
- Attendee self-service order lookup.

### Could Have

- Coupon/access code.
- Waitlist.
- Badge printing export.
- Multi-language attendee page.
- Organizer dashboard charts.

### Won't Have In MVP

- Full payment gateway production integration.
- Advanced seating map.
- Session agenda builder.
- Sponsor management.
- Native mobile app.

## Suggested Delivery Phases

### Phase 1: Build Foundation

- Chốt role/permission model.
- Chốt navigation và screen-level acceptance criteria.
- Chốt business policies cho reservation, cancellation và check-in.
- Chuẩn bị test cases cho critical journeys.

### Phase 2: Event Management

- Complete event và ticket management.
- Publish/unpublish rules.
- Team/role management.
- Organization settings.

### Phase 3: Registration Core

- Registration checkout.
- Seat reservation and expiration.
- Order lookup and self-service ticket page.
- Reservation concurrency tests.

### Phase 4: Payment Boundary

- Paid/free order confirmation.
- Simulated payment callback for MVP.
- Payment mismatch handling.
- Payment rollback/reconciliation handling.
- Final-sale cancellation policy for confirmed paid orders.

### Phase 5: Attendee Operations

- Issue tickets.
- Add attendee list và detail.
- Add confirmation resend.
- Add notification settings.
- Add check-in.

### Phase 6: Reporting And Hardening

- Sales và attendance reports.
- Audit log.
- Observability và alerting.

## Initial Product Backlog

### Epic 1: Event Setup

- As an organizer, I can create an event so that I can start configuring it.
- As an organizer, I can update event details so that attendees see correct information.
- As an organizer, I can publish/unpublish an event so that I control public visibility.

### Epic 2: Ticket Inventory

- As an organizer, I can create ticket types so that I can sell different packages.
- As an organizer, I can increase quota so that I can sell more seats.
- As the system, I prevent quota changes that would make inventory invalid.

### Epic 3: Registration And Reservation

- As an attendee, I can select tickets and create an order.
- As the system, I reserve seats atomically to prevent oversell.
- As the system, I expire unpaid reservations and release seats.

### Epic 4: Payment Confirmation

- As the system, I create payment intent for paid orders.
- As the system, I confirm order when payment succeeds.
- As the system, I reject mismatched or late payment events safely and route captured-but-unfulfilled payments to rollback/reconciliation.

### Epic 5: Ticketing And Notification

- As an attendee, I receive ticket code after confirmed registration.
- As an organizer, I can resend confirmation.
- As the system, I send transactional notifications for confirmation, resend and cancellation.

### Epic 6: Check-in

- As a check-in operator, I can scan QR code to admit attendee.
- As a check-in operator, I can manually search attendee.
- As the system, I prevent duplicate check-in unless authorized override.

### Epic 7: Reporting

- As an organizer, I can see sales summary.
- As an organizer, I can see attendance summary.
- As an organizer, I can export attendee list.

## Product Decisions

- Slug uniqueness là global trong toàn platform.
- MVP có organization scope, nhưng chưa cần tenant billing phức tạp.
- Payment gateway trong MVP dùng simulated callback hoặc adapter đơn giản; UI vẫn thiết kế như payment thật.
- Payment rollback chỉ áp dụng khi tiền đã bị capture nhưng order chưa thể confirm hoặc tickets chưa thể issue.
- Confirmed paid orders là final-sale: không hoàn tiền vì bất kỳ lý do gì.
- Confirmed paid order cancellation, nếu cần vì vận hành, chỉ invalidates tickets và không tự động bán lại seats trong MVP.
- Default reservation timeout là 15 phút.
- MVP hỗ trợ group registration theo nhiều attendee trong một order.
- Attendee mandatory fields trong MVP: full name và email.
- Check-in MVP yêu cầu online connection; offline mode là roadmap sau.
- Pricing dùng một currency mặc định cho mỗi event.
- Organizer có thể tạo nhiều ticket types với sales windows khác nhau để chạy nhiều giai đoạn bán vé như Early Bird, Regular và Last Minute.
- Tax invoice không thuộc MVP.
