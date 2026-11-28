# TanQuy Cinema – Danh mục 5 phim theo poster người dùng cung cấp

## Năm phim được giữ lại (đúng thứ tự 5 ảnh đã gửi)
1. **Love Destiny: The Movie** — Romance — `wwwroot/images/catalog/love-destiny-the-movie.jpg`
2. **Addicted** — Drama — `wwwroot/images/catalog/addicted.jpg`
3. **La La Land** — Romance — `wwwroot/images/catalog/la-la-land.jpg`
4. **The Dark Knight** — Action — `wwwroot/images/catalog/the-dark-knight.jpg`
5. **Interstellar** — Science Fiction — `wwwroot/images/catalog/interstellar.jpg`

Các file JPG trong `wwwroot/images/catalog` là chính 5 ảnh poster được gửi kèm, không phải poster SVG tự tạo hoặc phim mẫu do hệ thống tự chọn.

## Thể loại
Giữ đủ 11 thể loại của project: Action, Adventure, Animation, Comedy, Crime, Drama, Fantasy, Horror, Romance, Science Fiction và Thriller. Cột `Movies.Genre` có giới hạn 10 ký tự nên Interstellar dùng giá trị tương thích `Sci-Fi`, còn khóa ngoại `GenreID` trỏ đến thể loại đầy đủ `Science Fiction`.

## Hành vi dữ liệu
`Data/CinemaCatalogSeeder.cs` được gọi khi ứng dụng khởi động. Khi database kết nối được lần đầu, seeder sẽ thêm thể loại còn thiếu, xóa mọi phim ngoài 5 tên trên (bao gồm bản ghi trùng), sau đó bổ sung/cập nhật 5 phim và đường dẫn poster. Seeder ghi nhận lần cleanup trong bảng phụ `CinemaCatalogSeedState` (được tạo tự động, không cần migration). Những phim tạo mới qua CRUD sau lần cleanup đầu tiên sẽ không bị xóa ở các lần khởi động tiếp theo. Trang danh sách hiển thị đúng thứ tự poster.

**Cảnh báo:** khóa ngoại `ChiTietHoaDons.MaMovie` đang dùng `ON DELETE CASCADE`. Khi seeder xóa phim khác 5 phim được giữ, các dòng chi tiết hóa đơn tham chiếu đến phim bị xóa cũng bị xóa; hóa đơn chính và tài khoản không bị xóa. Sao lưu database trước khi deploy. Cleanup chỉ chạy một lần khi seeder chạy thành công lần đầu; các phim được tạo thêm qua CRUD sau đó được giữ lại.

Nếu cần chạy thủ công, dùng `Data/SeedCinemaCatalog.sql` sau khi backup database. Script thực hiện cleanup, thêm/cập nhật 5 phim và hiển thị danh sách còn lại trong một transaction. Không cần chạy cả SQL script lẫn seeder.

## Kiểm tra
ZIP bao gồm 5 poster JPG thật. Không có các poster SVG của bộ phim mẫu trước. Cần chạy `dotnet build` trên máy có .NET SDK tương ứng với `TargetFramework` hiện tại của project để xác nhận build; chưa thay đổi framework hoặc connection string trong lần cập nhật catalog này.
