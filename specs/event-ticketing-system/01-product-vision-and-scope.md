# Product Vision And Scope

## Product Vision

Event Ticketing Platform là nền tảng giúp organizers tạo, bán vé, vận hành và đo lường hiệu quả của các event như workshop, summit, meetup lớn, training program, concert, webinar và hybrid event.

## Naming Convention

- **Event** là tên canonical cho domain entity chính, API/resource, permission scope và UI copy.
- Workshop, summit, meetup, concert, webinar hoặc training program chỉ là event type; không dùng event type làm tên entity chính.
- Khi nói về messaging, dùng rõ thuật ngữ **domain event**, **integration event**, **event bus** hoặc **provider event id** để không nhầm với business entity **Event**.

Giá trị cốt lõi:

- Organizer có thể tạo event và ticket inventory nhanh chóng.
- Attendee có thể đăng ký và thanh toán một cách tin cậy.
- Hệ thống tránh oversell vé/seat trong các thời điểm có nhiều người mua cùng lúc.
- Operation team có công cụ check-in, theo dõi doanh thu, attendance và seat utilization.
- Platform có kiến trúc đủ tốt để mở rộng thành marketplace hoặc event SaaS trong tương lai.

## Market-Aligned Capabilities

Các hệ thống trên thị trường thường có các nhóm tính năng sau:

- Event setup: thông tin event, lịch diễn ra, địa điểm, visibility, landing page.
- Ticketing: ticket type, quota, price, sales window, access code, complimentary ticket.
- Registration: individual/group registration, attendee profile, order, reservation timeout.
- Payment: payment intent, capture, rollback/reconciliation for unfulfilled paid orders, failed payment handling.
- Attendee management: list, edit attendee, resend confirmation, cancel registration.
- Check-in: QR code, badge, onsite check-in, duplicate prevention.
- Reporting: revenue, sold/reserved/available seats, conversion, attendance.
- Integration: email, payment gateway, CRM/export, webhook/event bus.

## In Scope

- Quản lý lifecycle của event.
- Quản lý seat/ticket type.
- Public event catalog cho các event đã publish.
- Registration và order lifecycle.
- Seat reservation có expiration.
- Payment confirmation integration boundary.
- Email/notification contract.
- Attendee profile và ticket issuance.
- Check-in operation.
- Reporting cho organizers.
- Event-driven integration giữa các bounded context.

## Out Of Scope For Initial Version

- Full website/page builder.
- Complex agenda/session planning.
- Speaker management workflow.
- Sponsor/exhibitor portal.
- Multi-currency tax engine.
- Marketplace discovery/search SEO.
- Native mobile app.
- Advanced marketing automation.

Các phần này nên được chừa đường mở trong domain và integration events.

## Success Metrics

- Registration completion rate.
- Payment success rate.
- Oversell incident count = 0.
- Average checkout duration.
- Check-in throughput per minute.
- Organizer time to publish first event.
- Revenue reconciliation accuracy.
- Event processing delay giữa Event Management và Registration contexts.
