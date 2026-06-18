# Design References And Product Assumptions

## Tham Khảo Thị Trường

Các pattern dưới đây được rút ra từ cách các nền tảng event phổ biến tổ chức sản phẩm:

- Eventbrite Organizer nhấn mạnh dashboard, ticket sales, check-in bằng QR scan, quản lý attendee tại onsite và export attendance report.
- Cvent có mô hình event management toàn diện: registration, attendee management, onsite check-in, badge, reporting, session tracking.
- Whova nhấn mạnh registration, attendee list, check-in, badge generation, announcement, survey và event app.

Nguồn tham khảo:

- Eventbrite Organizer check-in app: https://www.eventbrite.com/organizer/features/organizer-check-in-app/
- Eventbrite attendance/check-ins report: https://www.eventbrite.com/help/en-us/articles/876912/view-your-event-attendance-report/
- Eventbrite organizer QR check-in help: https://www.eventbrite.co.uk/help/en-gb/articles/741083/how-to-check-in-attendees-at-the-event-with-eventbrite-organizer/
- Cvent event management features: https://www.cvent.com/en/event-management-software/features
- Cvent OnArrival check-in app: https://support.cvent.com/articles/en_US/Topic_Hub/Meet-Our-OnArrival-Check-In-App
- Whova event registration software: https://whova.com/event-registration-software/
- Whova event management software: https://whova.com/event-management-software/

## Product Assumptions

- Sản phẩm ưu tiên web app trước, responsive cho organizer desktop/tablet và attendee mobile.
- Organizer workspace là authenticated area.
- Public registration flow không yêu cầu attendee login trong MVP; order lookup dùng email + order code hoặc magic link.
- Payment trong MVP có thể là simulated provider hoặc adapter đơn giản, nhưng UI vẫn thiết kế như payment thật.
- Check-in có thể chạy trên tablet/mobile browser. Native app là roadmap sau.
- Event nhỏ có thể vận hành bằng một organizer, nhưng UI vẫn hỗ trợ staff role.

## Design Principles

- Organizer screens phải dense, scan nhanh, thao tác ít bước.
- Attendee checkout phải ngắn, rõ giá, rõ thời gian giữ chỗ.
- Check-in screen phải cực nhanh, ít chữ, status lớn, tránh nhầm lẫn.
- Reporting screen ưu tiên decision-making: sold, revenue, attendance, trend.
- Mọi destructive action phải có confirmation rõ ràng.
- Mọi async action cần trạng thái loading/success/error nhìn thấy được.

## Common UI Patterns

- Primary action nằm góc phải trên của content header.
- Secondary actions nằm trong overflow menu `...` nếu ít dùng.
- Filters nằm trên table, có `Clear filters`.
- Form dài chia section, có sticky footer chứa `Cancel` và `Save`.
- Table row click mở detail drawer hoặc detail page.
- Empty state phải có action tiếp theo, không chỉ thông báo trống.
- Error state phải cho retry nếu lỗi có thể retry.

