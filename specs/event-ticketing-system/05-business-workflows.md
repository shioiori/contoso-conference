# Business Workflows

## Workflow 1: Organizer Publish Một Event

1. Organizer tạo event ở trạng thái draft.
2. Organizer cấu hình thông tin cơ bản: name, slug, dates, timezone, venue/online info.
3. Organizer tạo ticket types với quotas và prices.
4. System validate publish readiness.
5. Organizer publish event.
6. Public catalog bắt đầu hiển thị event.
7. Ticket availability được hiển thị cho attendee registration.

Business exceptions:

- Không thể publish nếu dates không hợp lệ.
- Không thể publish nếu không có ticket type có thể bán.
- Không thể publish nếu slug bị trùng.

## Workflow 2: Attendee Đăng Ký Vé Trả Phí

1. Attendee mở event detail.
2. System hiển thị current availability.
3. Attendee chọn ticket quantity.
4. Attendee nhập buyer và attendee information.
5. System validate event/ticket availability.
6. System tạo order ở trạng thái `Reserved` hoặc `PaymentPending`.
7. System reserve seats và đặt expiration.
8. System tạo payment intent.
9. Attendee thanh toán trên provider.
10. Provider gửi payment success callback.
11. System confirm order.
12. System chuyển reserved seats thành sold.
13. System issue ticket codes và QR payloads.
14. System gửi confirmation email.

Business exceptions:

- Nếu payment fails, order chuyển sang `PaymentFailed` hoặc vẫn retry được cho tới expiration.
- Nếu reservation expires hoặc seat không còn hợp lệ trước khi payment success được xử lý, payment success phải đi vào rollback/reconciliation workflow; system không được confirm order hoặc issue ticket sai.
- Nếu requested quantity vượt availability, system reject registration.

## Workflow 3: Free Ticket Registration

1. Attendee chọn free ticket.
2. System validate availability.
3. System tạo order.
4. System reserve và confirm order ngay lập tức.
5. System issue ticket.
6. System gửi confirmation.

Business exceptions:

- Nếu ticket có access code, attendee phải cung cấp valid code.
- Free ticket vẫn có thể có per-order limit.

## Workflow 4: Reservation Expiration

1. Order được tạo với expiration.
2. Background job scan các reserved/payment-pending orders đã hết hạn.
3. System đánh dấu order là `Expired`.
4. System release reserved seats.
5. Attendee không thể tiếp tục thanh toán order đã hết hạn.

Business exceptions:

- Nếu payment success đến sau expiration, system không được silently confirm nếu chưa validate lại seat.
- Late payment cần rollback/reconciliation và support visibility.

## Workflow 5: Organizer Thêm Seats

1. Organizer mở ticket type.
2. Organizer tăng quota.
3. Event Management validate quantity.
4. Event Management lưu new quota.
5. System cập nhật seat availability.
6. Public catalog phản ánh availability mới.

Business exceptions:

- Giảm quota là một controlled operation riêng và không được làm sold + active reserved vượt quota.

## Workflow 6: Attendee Check-in

1. Operator chọn active event.
2. Operator scan QR code hoặc search attendee.
3. System validate ticket.
4. System check duplicate check-in.
5. System ghi nhận check-in.
6. System trả success result cho badge/admission.

Business exceptions:

- Cancelled tickets bị reject.
- Ticket thuộc event khác bị reject.
- Duplicate scan trả trạng thái already checked-in.

## Workflow 7: Payment Succeeded But Ticket Fulfillment Failed

1. Provider gửi payment success callback.
2. System load order và validate reservation, amount, currency, expiration và seat availability.
3. Nếu order không thể confirm hoặc ticket không thể issue, system không chuyển order sang `Confirmed`.
4. System mark payment/order vào trạng thái `RollbackPending` hoặc `ReconciliationRequired`.
5. System gọi payment rollback/void/reversal nếu provider còn cho phép, hoặc tạo support case để reconcile thủ công.
6. System release reserved seats nếu order chưa được confirmed.
7. System ghi audit/activity để support thấy rõ tiền đã được xử lý nhưng vé chưa được bán thành công.

Policy decision:

- Rollback chỉ áp dụng cho trường hợp payment succeeded nhưng attendee chưa mua được vé hợp lệ.
- Sau khi order đã `Confirmed` và tickets đã issued, giao dịch là final-sale và không hoàn tiền vì bất kỳ lý do gì.

## Workflow 8: Organizer Cancel Một Confirmed Paid Order

Policy decision:

- Confirmed paid order cancellation, nếu được phép vì lý do vận hành, chỉ invalidates tickets và ghi activity/audit.
- Không có refund cho confirmed paid order.
- Seats của cancelled confirmed paid tickets không tự động được bán lại trong MVP.
