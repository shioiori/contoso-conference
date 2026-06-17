# Roles And Permissions

## Quyết Định Product

Hệ thống sử dụng **role-based access control theo scope**.

Nói đơn giản: user được gán một role trong một phạm vi cụ thể, ví dụ:

- Platform Admin trên toàn platform.
- Organization Owner trong một organization.
- Event Manager trong một event.
- Check-in Operator trong một event.

Không dùng permission tự do cho từng user trong MVP, vì sẽ làm vận hành và QA phức tạp. Tuy nhiên, bên trong implementation nên xem role như một tập permission để sau này có thể mở rộng custom role cho enterprise plan.

## Scope Model

| Scope | Ý nghĩa | Ví dụ |
| --- | --- | --- |
| Platform | Toàn hệ thống | Platform Admin xử lý incident, cấu hình platform. |
| Organization | Toàn bộ events của một organizer/company | Organization Owner quản lý team và billing. |
| Event | Một event cụ thể | Event Manager quản lý một event được giao. |
| Self | Dữ liệu của chính attendee | Attendee xem/cancel order của mình nếu policy cho phép. |

Quy tắc:

- Quyền ở Platform áp dụng cho mọi organization và event.
- Quyền ở Organization áp dụng cho mọi event thuộc organization đó.
- Quyền ở Event chỉ áp dụng cho event được gán.
- Attendee không vào organizer workspace; attendee chỉ có self-service permission trên order/ticket của chính họ.

## System Roles

### Platform Admin

Nhân sự vận hành platform. Dùng cho support cấp cao, incident, cấu hình hệ thống và audit.

Quyền chính:

- Xem tất cả organizations và events.
- Impersonate hoặc hỗ trợ organizer theo quy trình nội bộ.
- Xem audit log.
- Khóa/mở khóa organization hoặc event khi có vi phạm.
- Cấu hình platform-level settings.

### Organization Owner

Chủ sở hữu organization. Đây là role cao nhất phía organizer.

Quyền chính:

- Tạo, sửa, publish, unpublish, archive events.
- Quản lý team và gán role.
- Quản lý ticket, orders, attendees, check-in.
- Xem revenue và financial reports.
- Export dữ liệu.
- Thực hiện cancellation theo final-sale policy.

### Event Manager

Người quản lý một event cụ thể. Phù hợp với event producer hoặc project manager.

Quyền chính:

- Sửa event setup.
- Publish/unpublish event nếu được Organization Owner gán.
- Quản lý ticket inventory.
- Xem orders, attendees, check-in, non-financial reports.
- Export attendee list.

Giới hạn:

- Không quản lý team organization.
- Không xem financial reports mặc định.
- Không xử lý payment rollback/reconciliation nếu không có Finance Manager role.

### Registration Manager

Người phụ trách registration/support.

Quyền chính:

- Xem orders và attendees.
- Sửa attendee info.
- Resend confirmation/ticket.
- Cancel unpaid/reserved order.
- Add complimentary/manual attendee nếu event cho phép.
- Export attendee list.

Giới hạn:

- Không publish event.
- Không sửa ticket price/quota.
- Không xem full financial report.
- Không xử lý captured payment rollback/reconciliation.

### Finance Manager

Người phụ trách doanh thu, payment rollback và reconciliation.

Quyền chính:

- Xem revenue, payment status, rollback/reconciliation report.
- Export financial report.
- Request/approve payment rollback khi tiền đã capture nhưng tickets chưa được issue.
- Review confirmed paid order cancellation activity theo final-sale policy.

Giới hạn:

- Không sửa public event content nếu không có role khác.
- Không quản lý check-in onsite.

### Check-in Lead

Người phụ trách onsite check-in tại một event.

Quyền chính:

- Check-in attendee.
- Manual attendee lookup.
- Override duplicate check-in với reason.
- Xem attendance report.
- Quản lý gate/device/operator assignment nếu có.

Giới hạn:

- Không xem revenue.
- Không sửa ticket/order/payment.

### Check-in Operator

Nhân sự scan vé tại cổng.

Quyền chính:

- Scan QR.
- Manual search attendee.
- Check-in attendee.
- Xem trạng thái ticket tối thiểu để quyết định admit/reject.

Giới hạn:

- Không override duplicate check-in.
- Không xem buyer payment detail.
- Không export dữ liệu.
- Không sửa attendee info.

### Viewer

Người chỉ xem dữ liệu event.

Quyền chính:

- Xem event dashboard không có revenue.
- Xem attendee/order summary đã được mask thông tin nhạy cảm nếu cần.
- Xem reports không chứa financial data.

Giới hạn:

- Không tạo/sửa/xóa dữ liệu.
- Không export.
- Không check-in.

### Attendee

Người tham dự event.

Quyền chính:

- Xem public event.
- Register.
- Xem order/ticket của chính mình qua confirmation link/order lookup.
- Download ticket.
- Request cancellation nếu event policy cho phép.

Giới hạn:

- Không truy cập organizer workspace.
- Không xem attendee khác.

### External Provider

Payment Provider và Notification Provider không phải user role. Đây là integration actor. Chúng chỉ được gọi các endpoint/integration contract được bảo vệ bằng secret/signature.

## Permission Matrix

| Capability | Platform Admin | Org Owner | Event Manager | Registration Manager | Finance Manager | Check-in Lead | Check-in Operator | Viewer | Attendee |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Create event | Yes | Yes | No | No | No | No | No | No | No |
| Edit event setup | Yes | Yes | Yes | No | No | No | No | View | No |
| Publish/unpublish event | Yes | Yes | Yes | No | No | No | No | No | No |
| Archive event | Yes | Yes | No | No | No | No | No | No | No |
| Create/edit ticket type | Yes | Yes | Yes | No | No | No | No | View | No |
| Add seats | Yes | Yes | Yes | No | No | No | No | View | No |
| Change ticket price | Yes | Yes | Yes | No | No | No | No | View | No |
| View orders | Yes | Yes | Yes | Yes | Yes | No | No | View | Self only |
| Cancel unpaid order | Yes | Yes | Yes | Yes | No | No | No | No | Self if policy allows |
| Cancel confirmed paid order | Yes | Yes | No | No | Review only | No | No | No | Self request only |
| Request/approve payment rollback | Yes | Yes | No | No | Yes | No | No | No | No |
| View attendees | Yes | Yes | Yes | Yes | No | Yes | Limited | View | Self only |
| Edit attendee info | Yes | Yes | Yes | Yes | No | No | No | No | Self if policy allows |
| Add complimentary attendee | Yes | Yes | Yes | Yes | No | No | No | No | No |
| Resend confirmation/ticket | Yes | Yes | Yes | Yes | No | No | No | No | Self only |
| Check-in attendee | Yes | Yes | Yes | Yes | No | Yes | Yes | No | No |
| Override duplicate check-in | Yes | Yes | No | No | No | Yes | No | No | No |
| View sales/revenue report | Yes | Yes | No | No | Yes | No | No | No | No |
| View attendance report | Yes | Yes | Yes | Yes | No | Yes | No | View | No |
| Export attendee list | Yes | Yes | Yes | Yes | No | No | No | No | No |
| Export financial report | Yes | Yes | No | No | Yes | No | No | No | No |
| Manage team/roles | Yes | Yes | No | No | No | No | No | No | No |
| View audit log | Yes | Yes | Event scope | No | Financial actions only | Check-in actions only | Own actions only | No | No |

## Navigation By Role

| Role | Visible Organizer Tabs |
| --- | --- |
| Organization Owner | Dashboard, Setup, Tickets, Orders, Attendees, Check-in, Reports, Team, Settings |
| Event Manager | Dashboard, Setup, Tickets, Orders, Attendees, Check-in, Reports, Notifications |
| Registration Manager | Dashboard, Orders, Attendees |
| Finance Manager | Dashboard, Orders, Reports, Payment |
| Check-in Lead | Dashboard, Attendees, Check-in, Reports |
| Check-in Operator | Check-in |
| Viewer | Dashboard, Reports |

## Business Decisions

- Slug uniqueness: global trong toàn platform để public URL rõ ràng và tránh nhầm lẫn SEO.
- MVP có organization scope, nhưng không cần tenant billing phức tạp.
- Payment gateway trong MVP dùng simulated callback hoặc adapter đơn giản; UI vẫn thiết kế theo payment thật.
- Payment rollback chỉ áp dụng khi tiền đã bị capture nhưng order chưa thể confirm hoặc tickets chưa thể issue.
- Confirmed paid orders là final-sale: không hoàn tiền vì bất kỳ lý do gì.
- Confirmed paid order cancellation, nếu cần vì vận hành, chỉ invalidates tickets và không tự động bán lại seats trong MVP.
- Default reservation timeout là 15 phút.
- MVP hỗ trợ group registration theo nhiều attendee trong một order.
- Attendee mandatory fields trong MVP: full name và email.
- Check-in MVP yêu cầu online connection; offline mode là roadmap sau.
- Pricing dùng một currency mặc định cho mỗi event; mỗi ticket type dùng currency của event.
- Tax invoice không thuộc MVP.

## Audit Requirements By Role

Các hành động sau luôn phải ghi audit:

- Role assignment hoặc role removal.
- Publish/unpublish/archive event.
- Ticket quota/price/sales window change.
- Order cancellation.
- Payment rollback/reconciliation request/approval.
- Attendee info update.
- Duplicate check-in override.
- Data export.

Audit entry cần hiển thị được trong admin/support view với actor, role tại thời điểm thực hiện, scope, action, timestamp và reason nếu có.
