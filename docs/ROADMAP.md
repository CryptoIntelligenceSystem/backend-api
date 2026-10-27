# backend-api — Lộ trình triển khai

> Tham chiếu: [infrastructure/docs/PROJECT_REVIEW.md](https://github.com/CryptoIntelligenceSystem/infrastructure/blob/main/docs/PROJECT_REVIEW.md). Repo này **triển khai** `infrastructure/contracts/v1/openapi.yaml` và **đọc** các bảng ClickHouse định nghĩa ở `infrastructure/clickhouse/init/`; không tự định nghĩa lại response hay schema bảng.
> Lịch: tuần 1 = 19–25/10/2026. Phụ trách chính: **C (Backend & Storage)**. Stack đề xuất (chờ Q7): Python 3.11 + FastAPI + `clickhouse-connect` + Pydantic v2.

## 1. Mục tiêu cuối (khi demo)

- Service `api` chạy ở port **8000** (profile `app` của infrastructure), base path `/api/v1`.
- Endpoint đúng OpenAPI v1: `/health`, `/symbols`, `/candles` (1m, 5m), `/indicators` (MA20), `/signals` (volume bất thường), `/news`, `/status` (thời điểm cập nhật gần nhất, độ trễ).
- Mọi truy vấn ClickHouse dùng tham số (không ghép chuỗi), đọc với `FINAL`, có giới hạn `limit`.
- p95 thời gian phản hồi `/candles?limit=500` < 200 ms trên máy demo.
- Test unit + test tích hợp với ClickHouse thật (container) chạy trong CI.

## 2. Hiện trạng

- Repo chỉ có commit "Initial commit" và `README.md` rỗng (0 byte). Chưa chọn ngôn ngữ/framework, chưa có code, test, Dockerfile, CI.
- Phía dữ liệu: bảng `pipeline.market_data`, `news_data` đã có schema nhưng chưa có writer (infrastructure C2) → API phải phát triển trên **fixture** tới khi INF-M4 xong.

## 3. Kiến trúc và cấu trúc thư mục đề xuất

```
app/
  main.py                # FastAPI app, CORS, error handler → envelope lỗi của hợp đồng
  config.py              # Settings từ env: CLICKHOUSE_*, API_PORT, API_CORS_ORIGINS, BINANCE_SYMBOLS
  db.py                  # client clickhouse-connect dùng chung, health ping
  schemas.py             # Pydantic model sinh từ contracts/v1/openapi.yaml (datamodel-codegen)
  repositories/          # candles.py, news.py, indicators.py, signals.py, status.py — chỉ SQL
  routes/                # 1 file / nhóm endpoint — chỉ validate tham số + gọi repository
contracts/v1/openapi.yaml  # bản vendor từ infrastructure (scripts/sync_contracts.py)
tests/
  unit/                  # repository với client giả, validate tham số
  integration/           # ClickHouse container + schema infrastructure + fixture
docker-compose.test.yml  # clickhouse:24.8 + mount schema/fixture
Dockerfile, pyproject.toml, .env.example, README.md
.github/workflows/ci.yml
```

Lý do: tách route/repository để test SQL riêng; model sinh từ OpenAPI để không lệch hợp đồng; FastAPI tự sinh `/docs` giúp D kiểm tra nhanh.

## 4. Milestone

### API-M0 — Khung dự án
- **Tuần**: 3 · **Phụ trách**: C
- **Phụ thuộc**: INF-M0.2 (`openapi.yaml`), Q7 (stack).
- **Mock**: chưa cần ClickHouse — `/health` trả `clickhouse: "down"` khi không kết nối được.

| Task | File | Mô tả | Công | Nhánh |
|---|---|---|---|---|
| API-M0.1 | `pyproject.toml`, `app/main.py`, `app/config.py` | FastAPI + uvicorn, ruff, pytest, cấu hình từ env | S | `feat/api-skeleton` |
| API-M0.2 | `scripts/sync_contracts.py`, `contracts/v1/`, `app/schemas.py` | Vendor OpenAPI, sinh Pydantic model | S | `feat/api-contract-models` |
| API-M0.3 | `app/main.py` | Envelope `{"data","meta"}` và lỗi `{"error":{"code","message"}}` theo hợp đồng; CORS `API_CORS_ORIGINS` | S | `feat/api-skeleton` |
| API-M0.4 | `Dockerfile`, `.env.example`, `README.md` | Image `python:3.11-slim`, port 8000 | S | `feat/api-docker` |
| API-M0.5 | `.github/workflows/ci.yml` | ruff + pytest | S | `chore/ci-api` |

**Hoàn thành khi**:
```bash
ruff check . && pytest -q
uvicorn app.main:app --port 8000 &
curl -s http://localhost:8000/api/v1/health            # {"data":{"status":"ok","clickhouse":"down"},"meta":{...}}
curl -s http://localhost:8000/openapi.json | python -m json.tool > /dev/null
```

### API-M1 — Lớp truy cập ClickHouse và môi trường test
- **Tuần**: 4 · **Phụ trách**: C
- **Phụ thuộc**: API-M0; INF-M3 (schema v1 + fixture).
- **Mock**: nếu INF-M3 chưa merge, copy tạm `001_schema.sql` + fixture tự tạo vào `tests/integration/fixtures/` (xóa khi INF-M3 xong).

| Task | File | Mô tả | Công | Nhánh |
|---|---|---|---|---|
| API-M1.1 | `app/db.py` | Client dùng chung, timeout, `ping()` cho `/health` | S | `feat/api-clickhouse-client` |
| API-M1.2 | `docker-compose.test.yml`, `tests/integration/conftest.py` | ClickHouse 24.8, mount `../infrastructure/clickhouse/init` (hoặc bản vendor) + nạp fixture | M | `test/api-integration-env` |
| API-M1.3 | `app/repositories/candles.py` | `SELECT ... FROM market_data FINAL WHERE symbol={s:String} AND open_time BETWEEN ... ORDER BY open_time LIMIT {n:UInt32}`; 5m từ `candles_5m` | M | `feat/api-clickhouse-client` |

**Hoàn thành khi**:
```bash
docker compose -f docker-compose.test.yml up -d
pytest -q tests/integration/test_candles_repo.py      # đọc đúng số nến fixture, không trùng
curl -s http://localhost:8000/api/v1/health            # clickhouse: "ok"
```

### API-M2 — Endpoint nến, tin tức, symbol, trạng thái
- **Tuần**: 5 · **Phụ trách**: C
- **Phụ thuộc**: API-M1. Sang repo khác: dữ liệu thật cần **INF-M4** (Spark ghi `market_data`, `news_data`) và DP đang chạy — điểm tích hợp **IP3 cuối tuần 5**.
- **Mock**: fixture ClickHouse (INF-M3.2).

| Task | File | Mô tả | Công | Nhánh |
|---|---|---|---|---|
| API-M2.1 | `app/routes/candles.py` | `GET /candles`: `symbol` bắt buộc ∈ `BINANCE_SYMBOLS`, `interval ∈ {1m,5m}`, `from`/`to` ISO, `limit ≤ 1000`; lỗi → 422 envelope | M | `feat/api-candles` |
| API-M2.2 | `app/routes/news.py`, `repositories/news.py` | `GET /news`: lọc `has(symbols, {s})`, sắp giảm dần `published_at`, `limit ≤ 100` | S | `feat/api-news` |
| API-M2.3 | `app/routes/meta.py` | `GET /symbols`, `GET /status` (`max(open_time)` theo symbol, `lag_seconds = now - max(close_time)`, `max(published_at)`) | S | `feat/api-status` |
| API-M2.4 | `tests/` | Unit (tham số sai), integration (dữ liệu fixture), contract test so response với `openapi.yaml` (`schemathesis run`) | M | `test/api-contract` |

**Hoàn thành khi**:
```bash
pytest -q
schemathesis run contracts/v1/openapi.yaml --base-url http://localhost:8000/api/v1 --checks all
curl -s "http://localhost:8000/api/v1/candles?symbol=BTCUSDT&interval=1m&limit=5"     # 5 phần tử tăng dần
curl -s "http://localhost:8000/api/v1/candles?symbol=DOGE&interval=1m"                # 422 INVALID_PARAMETER
# IP3 (dữ liệu thật): open_time của nến cuối nằm trong 3 phút gần nhất
curl -s "http://localhost:8000/api/v1/status"
```

### API-M3 — Endpoint phân tích
- **Tuần**: 6 · **Phụ trách**: C
- **Phụ thuộc**: API-M2. Sang repo khác: **INF-M5** ghi `indicators`, `signals` (đường găng).
- **Mock**: fixture `indicators.jsonl`, `signals.jsonl` (INF-M3.2).

| Task | File | Mô tả | Công | Nhánh |
|---|---|---|---|---|
| API-M3.1 | `app/routes/indicators.py`, `repositories/indicators.py` | `GET /indicators?symbol&name=ma20&interval=5m&from&to&limit` | S | `feat/api-indicators` |
| API-M3.2 | `app/routes/signals.py`, `repositories/signals.py` | `GET /signals?symbol&type=volume_anomaly&from&to&limit≤200`, giảm dần `event_time` | S | `feat/api-signals` |
| API-M3.3 | `tests/` | Integration + contract test cho 2 endpoint | S | `test/api-analysis` |

**Hoàn thành khi**:
```bash
pytest -q
curl -s "http://localhost:8000/api/v1/indicators?symbol=BTCUSDT&name=ma20&interval=5m&limit=20"   # 20 điểm
curl -s "http://localhost:8000/api/v1/signals?symbol=BTCUSDT&limit=5"
```

### API-M4 — Hiệu năng và tích hợp toàn hệ thống
- **Tuần**: 7 · **Phụ trách**: C (D hỗ trợ compose)
- **Phụ thuộc**: API-M3; INF-M6.1 (profile `app`).

| Task | File | Mô tả | Công | Nhánh |
|---|---|---|---|---|
| API-M4.1 | `app/repositories/*.py` | Kiểm tra `EXPLAIN` dùng đúng `ORDER BY`; cache TTL 5 s cho `/status`, `/symbols` | S | `perf/api-queries` |
| API-M4.2 | `app/main.py` | Log request (method, path, status, duration ms) để đo độ trễ truy vấn | S | `feat/api-request-log` |
| API-M4.3 | `tests/perf/locustfile.py` hoặc script `hey` | Đo p95 với 20 người dùng đồng thời | S | `test/api-perf` |
| API-M4.4 | `README.md`, `docs/API.md` | Cách chạy, ví dụ request, giới hạn | S | `docs/api-usage` |

**Hoàn thành khi**:
```bash
docker compose -f ../infrastructure/docker-compose.yml --profile full up -d
hey -n 2000 -c 20 "http://localhost:8000/api/v1/candles?symbol=BTCUSDT&limit=500"   # p95 < 200 ms
```

## 5. Test và CI tối thiểu

- `.github/workflows/ci.yml` (mọi PR, Python 3.11):
  1. `ruff check .`
  2. `pytest -q tests/unit`
  3. Job `integration`: service container `clickhouse/clickhouse-server:24.8`, nạp schema vendor + fixture, `pytest -q tests/integration`
  4. `schemathesis` chạy với app khởi động trong job (contract test)
  5. `docker build .`
- Không có secret trong repo; CI dùng mật khẩu ClickHouse giả đặt trong workflow env.

## 6. Rủi ro riêng và giảm thiểu

| Rủi ro | Giảm thiểu |
|---|---|
| Không có dữ liệu thật tới cuối tuần 5 | Phát triển hoàn toàn trên fixture INF-M3.2; contract test đảm bảo khi nối dữ liệu thật không phải sửa FE |
| C kiêm cả ClickHouse schema (INF-M0, INF-M3) | INF-M0/INF-M3 xong trước tuần 4; API-M0 nhỏ (S); D hỗ trợ OpenAPI |
| Truy vấn chậm do quên `FINAL`/không lọc theo khóa | Repository tập trung SQL; review bắt buộc lọc `symbol` + khoảng thời gian; `limit` tối đa |
| SQL injection qua tham số | Chỉ dùng tham số của `clickhouse-connect`; validate `symbol` trong danh sách cho phép |
| Lệch hợp đồng với FE | Model sinh từ OpenAPI + schemathesis trong CI |

## 7. Vấn đề rà soát thuộc repo này

### Nghiêm trọng
| # | Vấn đề | Vị trí | Sửa ở |
|---|---|---|---|
| API-C1 | Repo trống, chưa có API nào — chặn tuần 7 của kế hoạch | toàn repo (chỉ `README.md` 0 byte) | API-M0 → API-M3 |

### Cao
| # | Vấn đề | Vị trí | Sửa ở |
|---|---|---|---|
| API-H1 | Nguồn dữ liệu ClickHouse chưa có writer (infrastructure C2) | `infrastructure/clickhouse/init/001_schema.sql:3-41` | phụ thuộc INF-M4; mock bằng fixture |
| API-H2 | Chưa chốt stack và hợp đồng response | — | Q7, INF-M0.2 |

### Trung bình
| # | Vấn đề | Vị trí | Sửa ở |
|---|---|---|---|
| API-M1 | Port 8080 đã dùng cho Spark master UI; API phải dùng 8000 | `infrastructure/docker-compose.yml:117` | API-M0.4 |

### Thấp
| # | Vấn đề | Vị trí | Sửa ở |
|---|---|---|---|
| L2 | `README.md` rỗng | `README.md` | API-M0.4 |
| L3 | Không có CI | — | API-M0.5 |
