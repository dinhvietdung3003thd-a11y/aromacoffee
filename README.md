# Aroma Coffee Backend API

## 1. Giới thiệu

**Aroma Coffee Backend API** là hệ thống backend cho ứng dụng quản lý và đặt đồ uống của quán cà phê Aroma Coffee. Project được xây dựng bằng **ASP.NET Core Web API**, sử dụng **MySQL** làm cơ sở dữ liệu chính, **Dapper** để truy vấn dữ liệu, **JWT** để xác thực người dùng, **Elasticsearch** để tìm kiếm sản phẩm và **SignalR** để hỗ trợ realtime cập nhật đơn hàng.

Backend này có thể kết nối với nhiều loại frontend khác nhau:

* Admin CMS Dashboard
* Customer/User Web Frontend
* Mobile App React Native / Expo
* Công cụ test API như Swagger hoặc Postman

---

## 2. Công nghệ sử dụng

### Backend

* ASP.NET Core Web API
* .NET 8
* Dapper
* MySqlConnector
* JWT Bearer Authentication
* BCrypt.Net-Next
* SignalR
* Swagger / Swashbuckle

### Database & Search

* MySQL 8.0
* Elasticsearch 8.13.4
* NEST / Elasticsearch.Net

### DevOps

* Docker
* Docker Compose

---

## 3. Cấu trúc thư mục

```text
WebApplication1/
│
├── Common/
│   └── StatusConstants.cs
│
├── Controllers/
│   ├── AuthController.cs
│   ├── CategoryController.cs
│   ├── ClientOrdersController.cs
│   ├── InventoryController.cs
│   ├── InventoryTransactionController.cs
│   ├── OrdersController.cs
│   ├── ProductController.cs
│   ├── RecipeController.cs
│   ├── SupplierController.cs
│   └── TablesController.cs
│
├── DTOs/
│   ├── account/
│   ├── categorys/
│   ├── inventorys/
│   ├── order/
│   ├── product/
│   ├── recipes/
│   ├── supplier/
│   └── tablefood/
│
├── Hubs/
│   └── OrderHub.cs
│
├── models/
│   ├── Account.cs
│   ├── Category.cs
│   ├── Customer.cs
│   ├── Inventory.cs
│   ├── InventoryTransaction.cs
│   ├── Order.cs
│   ├── OrderDetail.cs
│   ├── Product.cs
│   ├── Recipes.cs
│   ├── Supplier.cs
│   └── TableFood.cs
│
├── services/
│   ├── interfaces/
│   ├── AuthService.cs
│   ├── CategoryService.cs
│   ├── InventoryService.cs
│   ├── InventoryTransactionService.cs
│   ├── OrderService.cs
│   ├── ProductService.cs
│   ├── RecipeService.cs
│   ├── SupplierService.cs
│   ├── tableService.cs
│   └── TokenVersionValidator.cs
│
├── appsettings.json
├── appsettings.Development.json
├── docker-compose.yml
├── Dockerfile
├── Dump20260327.sql
├── Program.cs
├── WebApplication1.csproj
└── WebApplication1.sln
```

---

## 4. Chức năng chính

### 4.1. Xác thực người dùng

Hệ thống hỗ trợ xác thực bằng JWT.

Các nhóm người dùng chính:

* Admin
* Staff
* Customer

Chức năng liên quan:

* Thiết lập admin đầu tiên
* Đăng nhập admin/staff
* Đăng ký admin/staff
* Đăng ký khách hàng
* Đăng nhập khách hàng
* Đổi mật khẩu
* Lấy thông tin profile
* Cập nhật profile
* Kiểm tra hệ thống đã có admin hay chưa
* Kiểm tra token version để vô hiệu hóa token cũ

### 4.2. Quản lý sản phẩm

Hệ thống hỗ trợ:

* Lấy danh sách sản phẩm
* Lấy chi tiết sản phẩm
* Thêm sản phẩm
* Sửa sản phẩm
* Xóa sản phẩm
* Tìm kiếm sản phẩm bằng MySQL
* Tìm kiếm sản phẩm bằng Elasticsearch
* Đồng bộ dữ liệu sản phẩm sang Elasticsearch
* Kiểm tra khả năng pha chế theo nguyên liệu trong kho

### 4.3. Quản lý danh mục

Hệ thống hỗ trợ:

* Lấy danh sách danh mục
* Lấy chi tiết danh mục
* Tìm kiếm danh mục
* Thêm danh mục
* Sửa danh mục
* Xóa danh mục

### 4.4. Quản lý đơn hàng

Hệ thống hỗ trợ hai luồng đặt hàng:

**Luồng dành cho Admin/Staff**

* Lấy toàn bộ đơn hàng
* Lấy chi tiết đơn hàng
* Tìm kiếm đơn hàng
* Tạo đơn hàng tại quán
* Cập nhật đơn hàng
* Cập nhật trạng thái đơn hàng
* Xóa đơn hàng
* Lấy đơn theo số bàn

**Luồng dành cho Customer**

* Khách hàng tạo đơn hàng
* Khách hàng xem danh sách đơn của mình
* Khách hàng xem chi tiết đơn hàng của mình

### 4.5. Quản lý bàn

Hệ thống hỗ trợ:

* Lấy danh sách bàn
* Lấy chi tiết bàn
* Thêm bàn
* Sửa bàn
* Cập nhật trạng thái bàn
* Xóa bàn

### 4.6. Quản lý kho

Hệ thống hỗ trợ:

* Lấy danh sách nguyên liệu
* Thêm nguyên liệu
* Ghi nhận giao dịch nhập/xuất kho
* Xem báo cáo tồn kho
* Xem báo cáo chi tiêu theo nhà cung cấp

### 4.7. Quản lý công thức

Hệ thống hỗ trợ:

* Lấy danh sách công thức hiển thị
* Lấy công thức theo sản phẩm
* Thêm công thức
* Sửa công thức
* Xóa công thức

Công thức dùng để liên kết sản phẩm với nguyên liệu trong kho.

### 4.8. Quản lý nhà cung cấp

Hệ thống hỗ trợ:

* Lấy danh sách nhà cung cấp
* Lấy chi tiết nhà cung cấp
* Thêm nhà cung cấp
* Sửa nhà cung cấp
* Xóa nhà cung cấp

### 4.9. Realtime với SignalR

Project có `OrderHub` tại endpoint:

```text
/orderHub
```

SignalR được dùng để hỗ trợ cập nhật realtime liên quan đến đơn hàng.

---

## 5. Yêu cầu trước khi chạy

Trước khi chạy project, cần cài:

* Git
* Docker Desktop
* .NET 8 SDK nếu muốn chạy trực tiếp bằng `dotnet run`
* Visual Studio 2022 hoặc VS Code nếu muốn mở source code để chỉnh sửa
* Postman nếu muốn test API ngoài Swagger

Kiểm tra đã cài Docker chưa:

```bash
docker --version
```

Kiểm tra Docker Compose:

```bash
docker compose version
```

Kiểm tra .NET SDK:

```bash
dotnet --version
```

---

## 6. Cách clone project từ GitHub

Mở Terminal, CMD, PowerShell hoặc Git Bash, sau đó chạy:

```bash
git clone https://github.com/dinhvietdung3003thd-a11y/aromacoffee.git
```

Sau khi clone xong, di chuyển vào thư mục backend:

```bash
cd aromacoffee/WebApplication1
```

Kiểm tra trong thư mục có các file quan trọng sau:

```text
docker-compose.yml
Dockerfile
Program.cs
WebApplication1.csproj
Dump20260327.sql
```

Nếu có các file trên thì bạn đang đứng đúng thư mục để chạy backend.

---

## 7. Cách chạy project bằng Docker Compose

Đây là cách chạy khuyến nghị vì Docker Compose sẽ tự chạy đủ 3 service:

* API ASP.NET Core
* MySQL
* Elasticsearch

Tại thư mục `WebApplication1`, chạy:

```bash
docker compose up --build
```

Lần đầu chạy có thể mất vài phút vì Docker cần tải image MySQL, Elasticsearch và build API.

Sau khi chạy thành công, các container chính gồm:

```text
aromacafe_api
aromacafe_mysql
aromacafe_es
```

Kiểm tra container đang chạy:

```bash
docker ps
```

---

## 8. Link truy cập sau khi chạy

| Thành phần                     | Đường dẫn                                                      |
| ------------------------------ | -------------------------------------------------------------- |
| Swagger API                    | [http://localhost:5035/swagger](http://localhost:5035/swagger) |
| API Base URL                   | [http://localhost:5035](http://localhost:5035)                 |
| Elasticsearch                  | [http://localhost:9200](http://localhost:9200)                 |
| MySQL bên ngoài Docker         | localhost:3307                                                 |
| MySQL bên trong Docker network | mysql:3306                                                     |

Mở Swagger bằng trình duyệt:

```text
http://localhost:5035/swagger
```

Nếu Swagger mở được thì backend đã chạy thành công.

---

## 9. Cấu hình Docker Compose

File `docker-compose.yml` đang cấu hình 3 service.

### 9.1. API service

```yaml
api:
  build:
    context: .
    dockerfile: Dockerfile
  container_name: aromacafe_api
  ports:
    - "5035:8080"
  depends_on:
    - mysql
    - elasticsearch
```

Ý nghĩa:

* API chạy trong container tại port `8080`
* Máy thật truy cập API qua port `5035`
* Vì vậy Swagger chạy tại `http://localhost:5035/swagger`

### 9.2. MySQL service

```yaml
mysql:
  image: mysql:8.0
  container_name: aromacafe_mysql
  environment:
    MYSQL_ROOT_PASSWORD: 123456
    MYSQL_DATABASE: aromacafedb
  ports:
    - "3307:3306"
```

Thông tin kết nối MySQL từ máy thật:

```text
Host: localhost
Port: 3307
User: root
Password: 123456
Database: aromacafedb
```

Thông tin kết nối MySQL từ API container:

```text
Host: mysql
Port: 3306
User: root
Password: 123456
Database: aromacafedb
```

### 9.3. Elasticsearch service

```yaml
elasticsearch:
  image: docker.elastic.co/elasticsearch/elasticsearch:8.13.4
  container_name: aromacafe_es
  environment:
    - discovery.type=single-node
    - xpack.security.enabled=false
  ports:
    - "9200:9200"
```

Elasticsearch URL:

```text
http://localhost:9200
```

---

## 10. Database seed

Project có file seed database:

```text
Dump20260327.sql
```

Trong `docker-compose.yml`, file này được mount vào MySQL:

```yaml
./Dump20260327.sql:/docker-entrypoint-initdb.d/Dump20260327.sql
```

Khi MySQL container được tạo lần đầu, file SQL này sẽ tự động chạy để tạo bảng và dữ liệu mẫu.

Lưu ý quan trọng:

* File SQL chỉ tự chạy ở lần đầu container MySQL tạo database.
* Nếu database volume đã tồn tại thì sửa file SQL rồi chạy lại `docker compose up` sẽ không tự import lại.
* Muốn import lại từ đầu thì phải reset volume.

Reset database:

```bash
docker compose down -v
```

Sau đó chạy lại:

```bash
docker compose up --build
```

---

## 11. Cấu hình môi trường

Trong Docker Compose, API đang được truyền biến môi trường:

```yaml
ASPNETCORE_ENVIRONMENT: Development
ConnectionStrings__DefaultConnection: "Server=mysql;Port=3306;Database=aromacafedb;User=root;Password=123456;"
Elasticsearch__Uri: "http://elasticsearch:9200"
Jwt__Key: "AromaCafe_Jwt_Secret_Key_2026_Super_Safe_123456"
```

Khi chạy bằng Docker Compose, API sẽ lấy cấu hình từ các biến môi trường này.

Nếu chạy trực tiếp bằng `dotnet run`, bạn cần tự cấu hình connection string trong `appsettings.Development.json`, user secrets hoặc biến môi trường.

---

## 12. Cách chạy trực tiếp bằng .NET CLI

Cách này chỉ nên dùng khi bạn đã có MySQL và Elasticsearch đang chạy.

### 12.1. Chạy MySQL và Elasticsearch bằng Docker

Bạn có thể vẫn dùng Docker Compose để bật MySQL và Elasticsearch. Tuy nhiên file hiện tại cũng sẽ bật API. Nếu muốn chạy API bằng `dotnet run`, cần đảm bảo không bị trùng port `5035`.

Cách đơn giản nhất vẫn là chạy toàn bộ bằng:

```bash
docker compose up --build
```

Nếu muốn chạy API ngoài Docker, có thể tự tạo MySQL và Elasticsearch riêng, sau đó cấu hình:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3307;Database=aromacafedb;User=root;Password=123456;"
  },
  "Elasticsearch": {
    "Uri": "http://localhost:9200",
    "DefaultIndex": "aroma_products"
  },
  "Jwt": {
    "Key": "AromaCafe_Jwt_Secret_Key_2026_Super_Safe_123456"
  }
}
```

Sau đó chạy:

```bash
dotnet restore
dotnet run
```

Swagger sẽ mở tại:

```text
http://localhost:5035/swagger
```

---

## 13. CORS

Project cấu hình CORS trong `appsettings.json`:

```json
"Cors": {
  "AllowedOrigins": [
    "http://127.0.0.1:5500",
    "http://localhost:5500",
    "http://localhost:3000",
    "http://localhost:5173",
    "http://localhost:8081"
  ]
}
```

Ý nghĩa:

* `5500`: thường dùng cho HTML/CSS/JS chạy bằng Live Server
* `3000`: thường dùng cho React app
* `5173`: thường dùng cho Vite React app
* `8081`: thường dùng cho Expo / React Native development

Nếu test bằng điện thoại thật qua mạng LAN, cần thêm IP máy tính vào CORS, ví dụ:

```json
"http://192.168.1.10:8081"
```

Sau khi sửa CORS cần restart lại backend.

---

## 14. Authentication và JWT

Project sử dụng JWT Bearer Authentication.

Khi đăng nhập thành công, backend trả về token. Frontend cần lưu token và gửi token ở header khi gọi API yêu cầu đăng nhập.

Header mẫu:

```text
Authorization: Bearer YOUR_TOKEN_HERE
```

Trong Swagger, bấm nút **Authorize**, sau đó dán token vào ô Bearer.

Không cần gõ thêm chữ `Bearer` nếu Swagger đã yêu cầu nhập token trực tiếp.

---

## 15. Phân quyền API

Một số API public có thể gọi không cần token, ví dụ:

* Lấy danh sách sản phẩm
* Lấy danh mục
* Tìm kiếm sản phẩm
* Đăng nhập
* Đăng ký khách hàng

Một số API cần quyền `Customer`, ví dụ:

* Tạo đơn hàng khách hàng
* Xem đơn hàng của khách hàng

Một số API cần quyền `Admin` hoặc `Staff`, ví dụ:

* Quản lý sản phẩm
* Quản lý đơn hàng
* Quản lý kho
* Quản lý công thức
* Quản lý nhà cung cấp
* Quản lý bàn

---

## 16. Một số endpoint quan trọng

### 16.1. Auth

#### Tạo admin đầu tiên

```http
POST /api/Auth/setup-first-admin
```

#### Đăng nhập Admin/Staff

```http
POST /api/Auth/login
```

Body mẫu:

```json
{
  "username": "admin",
  "password": "123456"
}
```

#### Đăng ký Admin/Staff

```http
POST /api/Auth/register
```

#### Đăng ký Customer

```http
POST /api/Auth/customer/register
```

Body mẫu:

```json
{
  "username": "customer1",
  "password": "123456",
  "fullName": "Nguyen Van A",
  "phoneNumber": "0123456789",
  "email": "customer1@gmail.com"
}
```

#### Đăng nhập Customer

```http
POST /api/Auth/customer/login
```

Body mẫu:

```json
{
  "username": "customer1",
  "password": "123456"
}
```

#### Đổi mật khẩu

```http
PUT /api/Auth/change-password
```

#### Lấy profile Admin/Staff

```http
GET /api/Auth/me
```

#### Cập nhật profile Admin/Staff

```http
PUT /api/Auth/me
```

### 16.2. Product

#### Lấy toàn bộ sản phẩm

```http
GET /api/Product
```

#### Lấy sản phẩm theo id

```http
GET /api/Product/{id}
```

#### Thêm sản phẩm

```http
POST /api/Product
```

#### Sửa sản phẩm

```http
PUT /api/Product/{id}
```

#### Xóa sản phẩm

```http
DELETE /api/Product/{id}
```

#### Tìm kiếm bằng Elasticsearch

```http
GET /api/Product/search-elastic?keyword=coffee
```

#### Đồng bộ sản phẩm sang Elasticsearch

```http
POST /api/Product/sync-elastic
```

#### Tìm kiếm bằng MySQL

```http
GET /api/Product/search?keyword=coffee
```

#### Kiểm tra khả năng pha chế theo nguyên liệu

```http
GET /api/Product/ingredient-availability
```

### 16.3. Category

#### Lấy danh sách danh mục

```http
GET /api/Category
```

#### Lấy danh mục theo id

```http
GET /api/Category/{id}
```

#### Tìm kiếm danh mục

```http
GET /api/Category/search
```

#### Thêm danh mục

```http
POST /api/Category
```

#### Sửa danh mục

```http
PUT /api/Category/{id}
```

#### Xóa danh mục

```http
DELETE /api/Category/{id}
```

### 16.4. Customer Orders

#### Lấy đơn hàng của customer đang đăng nhập

```http
GET /api/client/orders
```

#### Tạo đơn hàng customer

```http
POST /api/client/orders
```

Body mẫu:

```json
{
  "orderDate": "2026-05-16T10:00:00",
  "tableId": null,
  "note": "Ít đá",
  "details": [
    {
      "productId": 1,
      "quantity": 2
    }
  ]
}
```

#### Lấy chi tiết đơn hàng customer

```http
GET /api/client/orders/{id}
```

### 16.5. Admin/Staff Orders

#### Lấy toàn bộ đơn hàng

```http
GET /api/Orders
```

#### Lấy đơn hàng theo id

```http
GET /api/Orders/{id}
```

#### Tìm kiếm đơn hàng

```http
GET /api/Orders/search
```

#### Lấy đơn theo số bàn

```http
GET /api/Orders/table/{tableNumber}
```

#### Tạo đơn hàng

```http
POST /api/Orders
```

#### Cập nhật đơn hàng

```http
PUT /api/Orders/{id}
```

#### Cập nhật trạng thái đơn

```http
PATCH /api/Orders/{id}/status
```

#### Xóa đơn hàng

```http
DELETE /api/Orders/{id}
```

### 16.6. Inventory

#### Lấy danh sách nguyên liệu

```http
GET /api/Inventory
```

#### Tạo giao dịch kho

```http
POST /api/Inventory/transaction
```

#### Báo cáo tồn kho

```http
GET /api/Inventory/report/summary
```

#### Báo cáo chi tiêu nhà cung cấp

```http
GET /api/Inventory/report/supplier-spend
```

#### Thêm nguyên liệu

```http
POST /api/Inventory
```

### 16.7. Inventory Transaction

#### Lấy danh sách giao dịch kho

```http
GET /api/InventoryTransaction
```

#### Lấy giao dịch theo id

```http
GET /api/InventoryTransaction/{id}
```

#### Tìm kiếm giao dịch kho

```http
GET /api/InventoryTransaction/search
```

### 16.8. Recipe

#### Lấy danh sách công thức hiển thị

```http
GET /api/Recipe/display-all
```

#### Lấy công thức theo sản phẩm

```http
GET /api/Recipe/product/{productId}
```

#### Thêm công thức

```http
POST /api/Recipe
```

#### Lấy công thức theo id

```http
GET /api/Recipe/{id}
```

#### Sửa công thức

```http
PUT /api/Recipe/{id}
```

#### Xóa công thức

```http
DELETE /api/Recipe/{id}
```

### 16.9. Supplier

#### Lấy danh sách nhà cung cấp

```http
GET /api/Supplier
```

#### Lấy nhà cung cấp theo id

```http
GET /api/Supplier/{id}
```

#### Thêm nhà cung cấp

```http
POST /api/Supplier
```

#### Sửa nhà cung cấp

```http
PUT /api/Supplier/{id}
```

#### Xóa nhà cung cấp

```http
DELETE /api/Supplier/{id}
```

### 16.10. Tables

#### Lấy danh sách bàn

```http
GET /api/Tables
```

#### Lấy bàn theo id

```http
GET /api/Tables/{id}
```

#### Thêm bàn

```http
POST /api/Tables
```

#### Cập nhật trạng thái bàn

```http
PATCH /api/Tables/{id}/status
```

#### Sửa bàn

```http
PUT /api/Tables/{id}
```

#### Xóa bàn

```http
DELETE /api/Tables/{id}
```

---

## 17. Cách test nhanh sau khi chạy

### 17.1. Test Swagger

Mở:

```text
http://localhost:5035/swagger
```

Nếu Swagger hiển thị danh sách API thì backend chạy thành công.

### 17.2. Test Product API

Gọi:

```http
GET http://localhost:5035/api/Product
```

Nếu trả về danh sách sản phẩm thì API đã kết nối được MySQL.

### 17.3. Test Elasticsearch

Mở:

```text
http://localhost:9200
```

Nếu Elasticsearch chạy, trình duyệt sẽ trả về thông tin cluster dạng JSON.

Sau đó test:

```http
GET http://localhost:5035/api/Product/search-elastic?keyword=coffee
```

Nếu chưa có dữ liệu trong index, có thể cần gọi API đồng bộ:

```http
POST http://localhost:5035/api/Product/sync-elastic
```

API này cần quyền Admin/Staff.

---

## 18. Kết nối frontend với backend

Frontend nên cấu hình API base URL là:

```text
http://localhost:5035
```

Ví dụ trong React/Vite:

```ts
export const appConfig = {
  apiBaseUrl: "http://localhost:5035"
};
```

Nếu chạy trên điện thoại thật bằng Expo, không dùng `localhost` vì `localhost` trên điện thoại là chính điện thoại, không phải máy tính.

Cần lấy IPv4 của máy tính:

```bash
ipconfig
```

Ví dụ IPv4 máy tính là `192.168.1.10`, frontend mobile dùng:

```text
http://192.168.1.10:5035
```

Đồng thời cần thêm origin tương ứng vào CORS nếu bị lỗi CORS.

---

## 19. Các lỗi thường gặp

### 19.1. Không mở được Swagger

Kiểm tra container API có chạy không:

```bash
docker ps
```

Xem log API:

```bash
docker logs aromacafe_api
```

Nếu port `5035` bị chiếm, cần tắt ứng dụng đang dùng port đó hoặc đổi port trong `docker-compose.yml`.

### 19.2. API báo lỗi không kết nối được MySQL

Kiểm tra MySQL container:

```bash
docker ps
```

Xem log MySQL:

```bash
docker logs aromacafe_mysql
```

Kiểm tra connection string trong Docker Compose:

```text
Server=mysql;Port=3306;Database=aromacafedb;User=root;Password=123456;
```

Lưu ý: Khi API chạy trong Docker, host MySQL phải là `mysql`, không phải `localhost`.

### 19.3. Sửa file SQL nhưng dữ liệu không thay đổi

Nguyên nhân: MySQL volume cũ vẫn còn, nên file seed SQL không chạy lại.

Cách xử lý:

```bash
docker compose down -v
docker compose up --build
```

### 19.4. Lỗi 401 Unauthorized

Nguyên nhân thường gặp:

* Chưa đăng nhập
* Chưa gửi token
* Token sai hoặc hết hạn
* Token đã bị vô hiệu hóa do token version thay đổi
* Gọi API cần quyền Admin/Staff nhưng token là Customer

Cách gửi token:

```text
Authorization: Bearer YOUR_TOKEN_HERE
```

### 19.5. Lỗi 403 Forbidden

Nguyên nhân thường gặp:

* Đã đăng nhập nhưng không đủ quyền
* Ví dụ Customer gọi API chỉ dành cho Admin/Staff

### 19.6. Lỗi 404 Not Found

Nguyên nhân thường gặp:

* Sai URL endpoint
* Sai tên controller
* Sai id
* Frontend đang gọi nhầm base URL

Ví dụ đúng:

```text
http://localhost:5035/api/Product
```

### 19.7. Lỗi Network request failed trên mobile

Nguyên nhân thường gặp:

* Mobile đang gọi `localhost`
* Điện thoại và máy tính không cùng mạng
* Backend chưa mở port đúng
* Firewall chặn kết nối
* CORS chưa thêm origin phù hợp

Cách xử lý:

* Lấy IPv4 của máy tính bằng `ipconfig`
* Dùng URL dạng `http://IPv4:5035`
* Đảm bảo điện thoại và máy tính cùng Wi-Fi
* Test trên điện thoại bằng trình duyệt: `http://IPv4:5035/swagger`

---

## 20. Lệnh Docker hay dùng

Chạy project:

```bash
docker compose up --build
```

Chạy ẩn dưới nền:

```bash
docker compose up -d --build
```

Dừng container:

```bash
docker compose down
```

Dừng và xóa volume database:

```bash
docker compose down -v
```

Xem container đang chạy:

```bash
docker ps
```

Xem log API:

```bash
docker logs aromacafe_api
```

Xem log MySQL:

```bash
docker logs aromacafe_mysql
```

Xem log Elasticsearch:

```bash
docker logs aromacafe_es
```

---

## 21. Gợi ý quy trình chạy lần đầu

Làm lần lượt như sau:

```bash
git clone https://github.com/dinhvietdung3003thd-a11y/aromacoffee.git
cd aromacoffee/WebApplication1
docker compose up --build
```

Sau đó mở:

```text
http://localhost:5035/swagger
```

Test API public:

```http
GET /api/Product
GET /api/Category
```

Sau đó đăng nhập hoặc đăng ký tài khoản để test các API cần token.

---

## 22. Ghi chú cho người phát triển

* Không nên commit thư mục `bin/` và `obj/` lên GitHub.
* Không nên hardcode JWT secret thật trong repository public.
* Khi đổi database seed, cần reset Docker volume để dữ liệu được import lại.
* Khi thêm API mới, nên cập nhật README để frontend biết endpoint cần gọi.
* Khi thêm role mới, cần kiểm tra lại `[Authorize(Roles = "...")]` ở controller.
* Khi đổi port backend, cần cập nhật lại frontend `apiBaseUrl`.

---

## 23. Tác giả

Aroma Coffee Team

Repository:

```text
https://github.com/dinhvietdung3003thd-a11y/aromacoffee
```
