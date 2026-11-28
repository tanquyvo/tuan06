# Advance Backend - ASP.NET Core MVC

## 1. Giới thiệu dự án

**TanQuy Cinema - ASP.NET Core MVC** là website quản lý và đặt mua vé phim được xây dựng bằng ASP.NET Core MVC. Dự án được phát triển nhằm áp dụng các kiến thức về xây dựng ứng dụng web, quản lý dữ liệu với Entity Framework Core, xác thực người dùng và triển khai các chức năng quản lý phim cũng như quy trình mua vé.

Website cung cấp giao diện để người dùng xem danh sách phim, tìm hiểu thông tin phim, quản lý giỏ hàng và thực hiện các thao tác liên quan đến đặt vé. Đồng thời, hệ thống hỗ trợ các chức năng CRUD để quản lý dữ liệu phim và thể loại.

## 2. Công nghệ sử dụng

| Công nghệ | Mục đích |
|---|---|
| ASP.NET Core MVC (.NET 10) | Xây dựng ứng dụng web theo mô hình MVC |
| C# | Ngôn ngữ lập trình backend |
| Entity Framework Core | ORM và quản lý dữ liệu |
| Microsoft SQL Server | Hệ quản trị cơ sở dữ liệu |
| ASP.NET Core Identity | Quản lý tài khoản và xác thực người dùng |
| Razor Views | Xây dựng giao diện web |
| HTML, CSS, JavaScript | Xây dựng giao diện và tương tác phía client |
| Bootstrap | Hỗ trợ thiết kế giao diện responsive |
| Session | Lưu trữ thông tin giỏ hàng trong phiên người dùng |

## 3. Các chức năng chính

### 3.1. Quản lý phim
- Hiển thị danh sách phim.
- Xem chi tiết thông tin phim.
- Thêm phim mới.
- Chỉnh sửa thông tin phim.
- Xóa phim.
- Upload và lưu hình ảnh poster phim.

### 3.2. Quản lý thể loại phim
- Hiển thị danh sách thể loại.
- Thêm, chỉnh sửa và xóa thể loại.
- Liên kết phim với thể loại tương ứng.

### 3.3. Giỏ hàng
- Thêm phim vào giỏ hàng.
- Tăng số lượng sản phẩm trong giỏ.
- Xóa phim khỏi giỏ hàng.
- Tính tổng giá trị giỏ hàng.

### 3.4. Quản lý tài khoản
- Đăng ký tài khoản.
- Đăng nhập và đăng xuất.
- Quản lý xác thực người dùng thông qua ASP.NET Core Identity.

### 3.5. Quản lý dữ liệu
- Sử dụng Entity Framework Core để truy cập cơ sở dữ liệu.
- Sử dụng migrations để quản lý cấu trúc database.
- Hỗ trợ khởi tạo dữ liệu thể loại và danh mục phim thông qua seeder.

## 4. Cấu trúc thư mục

```text
31241023037_VoTanQuy/
├── Areas/
│   └── Identity/
│       └── Pages/
├── Controllers/
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── CinemaCatalogSeeder.cs
│   └── SQL scripts
├── Extension/
├── Migrations/
├── Models/
├── Properties/
├── Validation/
├── Views/
├── wwwroot/
│   ├── css/
│   ├── js/
│   ├── images/
│   └── lib/
├── Program.cs
├── appsettings.json
├── appsettings.example.json
└── 31241023037_VoTanQuy.csproj
```

## 5. Yêu cầu môi trường

Để chạy dự án, cần chuẩn bị:
- .NET SDK 10.0.
- Microsoft SQL Server hoặc SQL Server instance tương thích.
- Visual Studio Code, Visual Studio hoặc IDE hỗ trợ .NET.
- Git để clone repository.

## 6. Cài đặt và chạy dự án

### Bước 1: Clone repository

```bash
git clone https://github.com/tanquyvo/tuan06.git
```

### Bước 2: Di chuyển đến thư mục SourceCode

```bash
cd tuan06/ASP-CORE/SourceCode
```

### Bước 3: Khôi phục packages

```bash
dotnet restore
```

### Bước 4: Cấu hình cơ sở dữ liệu

Cấu hình connection string cho SQL Server trước khi chạy ứng dụng.

Có thể sử dụng file `appsettings.example.json` làm mẫu cấu hình. Không sử dụng trực tiếp các giá trị placeholder trong file này.

Đối với môi trường phát triển, nên cấu hình connection string thông qua User Secrets hoặc biến môi trường để tránh đưa thông tin kết nối nhạy cảm vào repository.

Ví dụ sử dụng User Secrets:

```bash
dotnet user-secrets init
```

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING"
```

Thay `YOUR_CONNECTION_STRING` bằng connection string phù hợp với SQL Server của môi trường chạy.

### Bước 5: Cập nhật database

Nếu database chưa có cấu trúc tương ứng với các migrations, sử dụng Entity Framework Core để cập nhật:

```bash
dotnet ef database update
```

Nếu chưa cài công cụ EF Core CLI:

```bash
dotnet tool install --global dotnet-ef
```

Đối với database đã tồn tại, cần kiểm tra trạng thái migrations và cấu trúc database trước khi thực hiện cập nhật.

### Bước 6: Chạy ứng dụng

```bash
dotnet run
```

Sau khi ứng dụng khởi động thành công, truy cập URL được hiển thị trong Terminal để sử dụng website.

## 7. Database và dữ liệu khởi tạo

Ứng dụng sử dụng Entity Framework Core để quản lý dữ liệu và SQL Server làm hệ quản trị cơ sở dữ liệu.

Các thành phần liên quan đến dữ liệu nằm trong thư mục `Data/`, bao gồm:
- `ApplicationDbContext.cs`: định nghĩa DbContext và cấu hình truy cập dữ liệu.
- `CinemaCatalogSeeder.cs`: hỗ trợ khởi tạo và đồng bộ dữ liệu danh mục phim.
- SQL scripts: chứa các script phục vụ thao tác hoặc kiểm tra dữ liệu.

**Lưu ý:** Một số script và chức năng seeding có thể thực hiện thao tác xóa dữ liệu phim. Cần sao lưu database và kiểm tra nội dung script trước khi thực thi trên cơ sở dữ liệu có dữ liệu cần bảo toàn.

## 8. Lưu ý khi sử dụng repository

- Không commit connection strings, email AppPasswords, credentials hoặc các thông tin bí mật.
- Không đưa thư mục `bin/`, `obj/`, các thư mục publish hoặc build artifacts không cần thiết lên GitHub.
- Không đưa database cá nhân hoặc file database chứa dữ liệu thực tế lên repository nếu không cần thiết.
- Sử dụng `appsettings.example.json` để minh họa cấu hình.
- Thực hiện `dotnet restore` để khôi phục packages khi clone dự án.
- Kiểm tra cấu hình database trước khi chạy ứng dụng.

## 9. Deployment

Website đã được triển khai trên Somee.

Website: https://tanquycinema.somee.com

Repository source code: https://github.com/tanquyvo/tuan06

---

**Tác giả:** Võ Tấn Quý

**Project:** Advance Backend - ASP.NET Core MVC
