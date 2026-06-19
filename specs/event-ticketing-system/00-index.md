# Đặc tả hệ thống Event Ticketing

## Mục Đích

Bộ tài liệu này mô tả một nền tảng event ticketing bám sát nhu cầu thị trường: tạo và vận hành event, quản lý vé/seat inventory, đăng ký người tham dự, thanh toán, check-in và báo cáo. Hệ thống hiện tại có định hướng theo CQRS Journey Guide, nhưng yêu cầu nghiệp vụ trong bộ spec này được ưu tiên trước; CQRS và event-driven là cách hiện thực hóa business một cách rõ ràng và mở rộng được.

## Tài Liệu

1. [Product Vision And Scope](./01-product-vision-and-scope.md)
2. [Actors And Use Cases](./02-actors-and-use-cases.md)
3. [Roles And Permissions](./03-roles-and-permissions.md)
4. [Functional Requirements](./04-functional-requirements.md)
5. [Business Workflows](./05-business-workflows.md)
6. [Non Functional Requirements](./08-non-functional-requirements.md)
7. [MVP, Roadmap, And Backlog](./09-mvp-roadmap-and-backlog.md)
8. [Architecture Overview](./10-architecture-overview.md)
9. [Domain Model And DDD](./11-domain-model-and-ddd.md)
10. [Event-Driven Design](./12-event-driven-design.md)
11. [AWS Deployment And Operations](./13-aws-deployment-and-operations.md)
12. [Quality And Testing Strategy](./14-quality-and-testing-strategy.md)
13. [Job Application Execution Plan](./15-job-application-execution-plan.md)
14. [Implemented API Contract](./16-api-contract.md)
15. [Screen-Level Product Specs](./screen-specs/00-screen-spec-index.md)

## Nguyên Tắc Sản Phẩm

- Tính đúng đắn nghiệp vụ quan trọng hơn việc áp dụng pattern cho đẹp.
- Seat inventory và thanh toán là vùng rủi ro cao, cần consistency rõ ràng.
- Organizer cần thao tác nhanh, ít friction, có audit và báo cáo tin cậy.
- Attendee journey phải ngắn gọn: chọn vé, nhập thông tin, thanh toán, nhận confirmation, check-in.
- Hệ thống nên thiết kế để sau này thêm multi-tenant, coupon, waitlist, invoice, sponsor, session scheduling mà không phải đập lại core.
