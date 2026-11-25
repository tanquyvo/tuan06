<div align="center">

# tq's todo list

Ứng dụng quản lý task cá nhân với checklist, tìm kiếm nhanh và trang thống kê riêng.

**Node.js · HTML · CSS · JavaScript · SQLite schema**

</div>

## Giới thiệu

`tq's todo list` giúp theo dõi công việc trong ngày bằng giao diện sáng, xanh da trời pastel. Ứng dụng có dữ liệu mẫu cho buổi học, hỗ trợ quản lý task cơ bản và có REST API để tiếp tục mở rộng.

## Tính năng

- Tạo, sửa, xóa và đánh dấu hoàn thành task.
- Xem checklist hôm nay hoặc toàn bộ task.
- Lọc theo trạng thái và mức ưu tiên.
- Tìm trong tiêu đề, mô tả và danh mục; gợi ý từ khóa khi nhập và hỗ trợ tìm không dấu.
- Mở tìm kiếm nhanh bằng nút kính lúp, `/`, `⌘ K` trên macOS hoặc `Ctrl K` trên Windows/Linux. Dùng `↑` / `↓` để chọn gợi ý, `Enter` để tìm và `Esc` để đóng.
- Xem thống kê task theo trạng thái và danh mục.
- Popup có animation; giao diện responsive và có hiệu ứng khi cuộn.
- REST API chạy trên Node.js, không cần cài package ngoài.

> Biểu đồ hoạt động theo tuần hiện dùng dữ liệu mock. Những thống kê task còn lại được tính từ dữ liệu trong ứng dụng.

## Công nghệ

| Thành phần | Công nghệ |
| --- | --- |
| Backend | Node.js built-in `http`, không dùng framework |
| Frontend | HTML, CSS, JavaScript thuần |
| API | REST, JSON |
| Lưu trữ hiện tại | File JSON |
| Thiết kế database nâng cấp | SQLite, khóa ngoại, index, trigger và FTS5 |

## Chạy local

Yêu cầu **Node.js 18 trở lên**.

Sau khi clone repo về máy, mở Terminal tại thư mục project:

```bash
npm start
```

Mở [http://127.0.0.1:8080](http://127.0.0.1:8080). Không cần chạy `npm install` vì project không có package bên ngoài. Giữ Terminal mở trong lúc sử dụng; nhấn `Ctrl + C` để dừng server.

Nếu cổng `8080` đang được sử dụng, có thể chạy cổng khác trên macOS/Linux:

```bash
PORT=8081 npm start
```

Sau đó mở [http://127.0.0.1:8081](http://127.0.0.1:8081).

App cần Node.js server để gọi API và lưu dữ liệu; không mở `public/index.html` trực tiếp bằng `file://`.

## Dữ liệu

Khi chạy lần đầu, server tạo `data/tasks.json` với bốn task mẫu cho ngày hiện tại. Server không ghi đè file này nếu file đã tồn tại. Các thay đổi task được lưu vào JSON ngay khi thao tác.

Task gồm các trường chính:

| Trường | Mô tả |
| --- | --- |
| `id` | Mã định danh task |
| `title`, `description` | Tên và nội dung mô tả |
| `category` | Danh mục |
| `status` | `todo`, `in-progress` hoặc `done` |
| `priority` | `low`, `medium` hoặc `high` |
| `dueDate`, `dueTime` | Ngày và giờ đến hạn |
| `createdAt`, `updatedAt` | Thời điểm tạo và cập nhật |

## REST API

Base URL mặc định: `http://127.0.0.1:8080`.

| Method | Endpoint | Mô tả |
| --- | --- | --- |
| `GET` | `/api/health` | Kiểm tra trạng thái server và loại lưu trữ |
| `GET` | `/api/tasks` | Lấy toàn bộ task |
| `GET` | `/api/tasks?date=YYYY-MM-DD` | Lọc task theo ngày |
| `POST` | `/api/tasks` | Tạo task |
| `PATCH` | `/api/tasks/:id` | Cập nhật task |
| `DELETE` | `/api/tasks/:id` | Xóa task |

Ví dụ tạo task:

```bash
curl -X POST http://127.0.0.1:8080/api/tasks \
  -H 'Content-Type: application/json' \
  -d '{
    "title": "Ôn bài Node.js",
    "description": "Đọc lại ghi chú buổi học",
    "category": "Học tập",
    "status": "todo",
    "priority": "medium",
    "dueDate": "2026-11-25",
    "dueTime": "15:00"
  }'
```

## Cấu trúc thư mục

```text
.
├── database/
│   ├── README.md       # Quan hệ bảng và truy vấn SQL mẫu
│   └── schema.sql      # Thiết kế SQLite nâng cao
├── data/
│   └── tasks.json      # Dữ liệu runtime, tự tạo lần đầu chạy
├── public/
│   ├── app.js          # Tìm kiếm, lọc, thống kê và thao tác task
│   ├── index.html      # Giao diện
│   └── styles.css      # Theme, responsive và animation
├── server.js           # Static server và REST API
├── HelloWorld.js       # Entry point tương thích bài thực hành cũ
└── package.json
```

## Thiết kế SQLite

App hiện lưu dữ liệu runtime trong JSON; SQLite chưa được nối vào API. `database/schema.sql` là thiết kế để chuyển sang database khi cần phát triển thêm. Schema gồm users, workspaces, thành viên, categories, tasks, labels, comments, reminders, recurrence rules và lịch sử hoạt động; kèm ràng buộc, khóa ngoại, index tối ưu truy vấn và FTS5 tìm kiếm toàn văn.

Xem [database/README.md](database/README.md) để đọc sơ đồ quan hệ, truy vấn checklist/thống kê/tìm kiếm và lưu ý khi tích hợp. FTS5 cần bản SQLite được build kèm extension này.

## Tác giả

Thực hiện bởi [@tanquyvo](https://github.com/tanquyvo).
