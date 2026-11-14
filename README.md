# Crypto Intelligence System — Backend API

ASP.NET Core Web API đọc dữ liệu phân tích từ ClickHouse để frontend dashboard sử dụng.

## Phạm vi hiện tại

Backend chỉ đọc các bảng đang dùng trong pipeline:

- `pipeline.market_data`: dữ liệu market 1 phút.
- `pipeline.candles_5m`: nến đã gộp 5 phút.
- `pipeline.indicators`: chỉ báo, hiện có `ma20`.
- `pipeline.signals`: tín hiệu, hiện có `volume_anomaly`.
- `pipeline.news_data`: tin tức, sẽ có dữ liệu khi pipeline ghi news vào ClickHouse.
- `pipeline.news_impact`: biến động giá ở mốc 5/15/60 phút sau khi đăng tin; kết quả chỉ thể hiện quan hệ thời gian.

`pipeline.analysis_results` là bảng cũ và không được Backend sử dụng.

## Công nghệ và cấu trúc

- .NET 8 / ASP.NET Core Web API.
- ClickHouse HTTP API thông qua `HttpClient`, không cần cài driver riêng ở Backend.
- Luồng đọc dữ liệu: `Controller → Service → Repository → ClickHouse`.
- DTO request/response nằm trong `Application/Contracts`.
- Kiểm tra lỗi tập trung tại `Middleware/ExceptionHandlingMiddleware.cs`.

## Chạy local

Đảm bảo ClickHouse trong repo `infrastructure` đang chạy trước:

```powershell
cd "D:\Bích Đa Ta\infrastructure"
docker compose up -d
```

Sau đó chạy Backend:

```powershell
cd "D:\Bích Đa Ta\backend-api"
dotnet restore
dotnet build
dotnet run --urls http://localhost:5080
```

Mở Swagger UI để xem và thử API:

```text
http://localhost:5080/swagger
```

## Chạy Backend trong Docker

Backend được khai báo trong `infrastructure/docker-compose.yml`. Hai repo cần nằm
cùng một thư mục cha:

```text
D:\Bích Đa Ta\
├── backend-api\
├── data-pipeline\
└── infrastructure\
```

Từ repo infrastructure, chạy:

```powershell
cd "D:\Bích Đa Ta\infrastructure"
docker compose up -d --build backend
```

Trong Docker, Backend kết nối tới ClickHouse bằng tên service:

```text
http://clickhouse:8123
```

Từ máy host, truy cập Backend bằng:

```text
http://localhost:5080/swagger
```

Xem log Backend:

```powershell
docker compose logs -f backend
```

`appsettings.json` mặc định dùng tên service Docker `http://clickhouse:8123`.
Nếu chạy Backend trực tiếp trên máy, ghi đè cấu hình bằng biến môi trường:

```powershell
$env:ClickHouse__BaseUrl="http://localhost:8123"
$env:ClickHouse__Database="pipeline"
$env:ClickHouse__Username="pipeline_user"
$env:ClickHouse__Password="mat-khau-trong-file-env-cua-infrastructure"
```

Không commit mật khẩu thật vào Git.

## API chính

Khi Backend chạy ở `http://localhost:5080`:

| Method | Endpoint | Mục đích |
|---|---|---|
| GET | `/api/v1/health` | Kiểm tra Backend và ClickHouse |
| GET | `/api/v1/market/candles` | Lấy nến 5 phút |
| GET | `/api/v1/market/minute-candles` | Lấy dữ liệu market nến 1 phút từ `market_data` |
| GET | `/api/v1/indicators` | Lấy chỉ báo, mặc định `ma20` |
| GET | `/api/v1/signals` | Lấy tín hiệu, mặc định `volume_anomaly` |
| GET | `/api/v1/news` | Lấy tin tức |
| GET | `/api/v1/news/impact` | Lấy biến động giá sau tin, lọc theo `symbol`, `horizonMinutes`, thời gian và phân trang |
| GET | `/api/v1/status` | Thống kê số dòng và thời điểm cập nhật |

Ví dụ:

```text
http://localhost:5080/api/v1/health
http://localhost:5080/api/v1/market/candles?symbol=BTCUSDT&interval=5m&limit=20
http://localhost:5080/api/v1/market/minute-candles?symbol=BTCUSDT&from=2026-10-29T12:00:00Z&to=2026-10-29T14:00:00Z&limit=100&offset=0
http://localhost:5080/api/v1/indicators?symbol=BTCUSDT&interval=1m&name=ma20&onlyReady=true&limit=20
http://localhost:5080/api/v1/signals?symbol=BTCUSDT&signalType=volume_anomaly&limit=20
http://localhost:5080/api/v1/news?symbol=BTCUSDT&limit=20&offset=0
http://localhost:5080/api/v1/news/impact?symbol=BTCUSDT&horizonMinutes=15&limit=20&offset=0
http://localhost:5080/api/v1/status
```

Các API danh sách hỗ trợ `limit` và `offset`. `limit` tối đa là 5000. Response có
`data`, `count`, `limit`, `offset`, `hasMore` và `nextOffset` để frontend biết còn
trang tiếp theo hay không. Các mốc thời gian được truy vấn và trả về theo UTC.
Có thể thêm `from` và `to` dạng ISO-8601, ví dụ `2026-10-29T12:00:00Z`.

## Kiểm thử nhanh

```powershell
dotnet build
Invoke-RestMethod http://localhost:5080/api/v1/health
Invoke-RestMethod "http://localhost:5080/api/v1/status"
```

Chạy unit/API tests (integration test được skip nếu chưa bật biến môi trường):

```powershell
dotnet test tests/CryptoIntelligence.Api.Tests/CryptoIntelligence.Api.Tests.csproj
```

Chạy integration test với ClickHouse Docker thật:

```powershell
cd "D:\Bích Đa Ta\infrastructure"
docker compose up -d clickhouse

cd "D:\Bích Đa Ta\backend-api"
$env:RUN_CLICKHOUSE_INTEGRATION_TESTS="1"
$env:CLICKHOUSE_TEST_BASE_URL="http://localhost:8123"
$env:CLICKHOUSE_TEST_DATABASE="pipeline"
$env:CLICKHOUSE_TEST_USERNAME="pipeline_user"
$env:CLICKHOUSE_TEST_PASSWORD="mat-khau-trong-file-env-cua-infrastructure"
dotnet test tests/CryptoIntelligence.Api.Tests/CryptoIntelligence.Api.Tests.csproj `
  --filter "FullyQualifiedName~ClickHouseIntegrationTests"
```

Integration test chèn dữ liệu mẫu có tiền tố `backend_integration` vào ClickHouse,
sau đó gọi thật `/market/minute-candles` và `/news`. Dữ liệu mẫu này chỉ phục vụ
kiểm thử và có thể xóa thủ công nếu cần.
