# Non Functional Requirements

## Availability

- Public browsing và registration nên đạt 99.9% monthly availability cho paid production tier.
- Organizer admin có thể thấp hơn trong MVP, nhưng không được làm gián đoạn attendee checkout trong lúc mở bán.
- Khi hạ tầng phụ trợ gặp sự cố, hệ thống phải hiển thị trạng thái lỗi rõ ràng và không tạo dữ liệu trùng/lệch.

## Performance

- Public event detail p95 response time <= 300 ms under normal load.
- Registration command p95 <= 800 ms, không tính thời gian redirect/ xử lý bên payment provider.
- Check-in command p95 <= 300 ms.
- Attendee search cho check-in p95 <= 500 ms với event tới 100,000 attendees.

## Scalability

- Luồng registration phải bảo vệ seat inventory dưới concurrent load.
- Reporting queries không được làm quá tải transactional order database.

## Consistency

- Seat reservation phải strongly consistent theo từng ticket type.
- Public availability có thể chậm hơn trạng thái thực tế trong thời gian ngắn, nhưng thao tác reserve seat phải kiểm tra lại availability chính xác.
- Payment confirmation phải idempotent.

## Security

- Organizer APIs yêu cầu authentication.
- Authorization được scope theo organization/event.
- Payment callbacks yêu cầu signature verification.
- Secrets của payment provider không được lưu trong source code.
- Truy cập attendee PII phải được audit.
- CSV export cần permission riêng.

## Privacy And Compliance

- Chỉ lưu attendee PII thật sự cần thiết.
- Hỗ trợ attendee data correction.
- Hỗ trợ retention policy theo organization.
- Mask sensitive fields trong logs.
- Không log payment card data.

## Observability

- Structured logs với correlation ID.
- Trace các user action quan trọng như checkout, payment callback, cancellation và check-in.
- Metrics:
  - Orders created/confirmed/cancelled/expired.
  - Reservation failures.
  - Payment callback failures.
  - Check-in success/duplicate/rejected count.
- Alerts:
  - Payment mismatch detected.
  - Seat oversell invariant violation.
  - Payment provider unavailable.

## Reliability

- Use retry with backoff cho transient provider/broker failures.
- Background jobs phải resumable.
- Critical transactional actions must be safe to retry without duplicating orders, payments, tickets or check-ins.

## Audit

Audit required for:

- Publish/unpublish event.
- Ticket quota change.
- Price change.
- Order cancellation.
- Payment rollback/reconciliation request.
- Attendee info change.
- Duplicate check-in override.
- Data export.

Audit fields:

- ActorId
- ActorType
- Action
- EntityType
- EntityId
- Before
- After
- OccurredAt
- CorrelationId

## Backup And Recovery

- Database backups ít nhất hằng ngày cho MVP, thường xuyên hơn cho production.
- Point-in-time recovery target cần được xác định trước khi mở payment thật.
- Recovery runbook phải bao gồm orders, payments, tickets và check-in records.
