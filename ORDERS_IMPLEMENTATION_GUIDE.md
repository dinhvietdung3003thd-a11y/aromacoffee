# Orders Management Dashboard - Implementation Guide

## Overview

Đã hoàn thành triển khai giao diện quản lý đơn hàng (**Orders Management**) với kết nối đầy đủ đến backend API. Khi bạn click vào **Commerce > Orders** trong menu, giao diện sẽ hiển thị danh sách tất cả đơn hàng với các chức năng quản lý.

## Features Implemented

### 1. **Orders Listing Interface** 

Hiển thị bảng các đơn hàng với các cột:
- **Mã đơn** (#ID) - Order ID
- **Bàn** - Table Number / Table ID
- **Thời gian** - Order Date & Time (Vietnamese locale format)
- **Khách hàng** - Customer Name
- **Tổng tiền** - Total Amount (formatted Vietnamese currency)
- **Trạng thái** - Status with color-coded badges
- **Thao tác** - Action buttons

### 2. **Status Badges (Color-Coded)**

```
Yellow  → "Chờ xử lý"     (Pending/Waiting)
Blue    → "Đang làm"      (In Progress/Preparing)
Green   → "Đã hoàn thành" (Completed/Done)
Red     → "Đã hủy"        (Cancelled)
```

### 3. **Filter Features**

Located at the top of the orders section:

**Filter by Table (Bàn):**
- Dropdown with predefined tables: B1, B2, B3, B4, B5, B6
- Real-time filtering

**Filter by Status (Trạng thái):**
- Select specific status to filter
- Real-time filtering

**Search:**
- Search by Order ID or Customer Name
- Click "Tìm kiếm" button or press Enter to search

### 4. **Action Buttons (Dropdown Menu)**

Click the **kebab menu** (⋮) on each row to access:

#### **Cập nhật trạng thái** (Update Status)
- Opens a prompt dialog
- Select from 4 options:
  1. Chờ xử lý
  2. Đang làm
  3. Đã hoàn thành
  4. Đã hủy
- Sends PATCH request to `/api/orders/{id}/status`
- Updates UI immediately on success

#### **Xem chi tiết** (View Details)
- Displays complete order information:
  - Order ID
  - Table Number
  - Order Date & Time
  - Total Amount
  - Current Status
  - Staff Member who created it
  - List of products with quantities and prices
- Opens in alert dialog

#### **Xóa** (Delete)
- Confirmation dialog before deletion
- Sends DELETE request to `/api/orders/{id}`
- Removes order from list immediately on success
- Red text color for visual distinction

### 5. **User Feedback**

- **Toast Notifications:** Success/Error messages appear at bottom-right
  - Green for success
  - Red for errors
  - Auto-dismiss after 3 seconds
  
- **Loading State:** Spinner shows while loading data
- **Empty State:** Message displays if no orders exist
- **Error Handling:** Redirects to login if JWT token expires (401)

## API Integration

### Connected Endpoints

#### 1. **Get All Orders**
```
GET /api/orders
Authorization: Bearer {token}
```
- Fetches complete list of all orders
- Called when navigating to Orders page

#### 2. **Update Order Status**
```
PATCH /api/orders/{id}/status
Authorization: Bearer {token}
Content-Type: application/json

Body: "Đã hoàn thành"  (or other status)
```
- Updates order status
- Only accepts valid status values from StatusConstants

#### 3. **Delete Order**
```
DELETE /api/orders/{id}
Authorization: Bearer {token}
```
- Permanently removes an order
- Returns 200 OK on success

## Backend Changes Made

### 1. **StatusConstants.cs** - Updated

Changed Order Status values from English to Vietnamese:

**Before:**
```csharp
"Pending", "Paid", "Completed", "Cancelled"
```

**After:**
```csharp
"Chờ xử lý", "Đang làm", "Đã hoàn thành", "Đã hủy"
```

### 2. **OrderService.cs** - Updated

Changed default status initialization:
- All instances of `"Pending"` → `"Chờ xử lý"`
- Ensures consistency across the application

## Files Modified

1. **c:\...\HTML\admin_dashboard.html**
   - Added Orders interface section
   - Added filter controls
   - Added JavaScript functions for API calls
   - Added CSS styles for tables and dropdowns
   - Added Toast notification system

2. **c:\...\wwwroot\admin_dashboard.html**
   - Synced copy of above HTML file
   - This is the file served to clients

3. **Common\StatusConstants.cs**
   - Updated OrderStatuses to Vietnamese values

4. **services\OrderService.cs**
   - Updated default status values to Vietnamese

## Configuration

### API Base URL
```javascript
const API_BASE_URL = 'http://localhost:5035/api';
```

**Adjust if:**
- Your API runs on a different port
- Deployment environment uses different URL
- Edit in both HTML files if changed

### Authentication
- Uses JWT token from `localStorage.user.token`
- Automatically stored after login
- Token sent in `Authorization: Bearer {token}` header

## How to Use

### Step 1: Run Backend
```bash
cd WebApplication1
dotnet run
```
API will start on `http://localhost:5035`

### Step 2: Access Dashboard
1. Open `admin_dashboard.html` in browser
2. Login with valid credentials
3. Click **Commerce** in sidebar menu > **Orders**

### Step 3: Manage Orders
- **View all orders:** Automatic on page load
- **Filter:** Select table/status dropdowns or search
- **Update Status:** Click ⋮ > Cập nhật trạng thái
- **View Details:** Click ⋮ > Xem chi tiết
- **Delete:** Click ⋮ > Xóa (with confirmation)

## Testing Checklist

- [ ] Backend API running on port 5035
- [ ] Navigate to Orders page loads orders list
- [ ] Orders display with correct data from database
- [ ] Filters by table work correctly
- [ ] Filters by status work correctly
- [ ] Search by ID or customer name works
- [ ] Update status button opens prompt
- [ ] Status updates reflected in table immediately
- [ ] View details shows all order information
- [ ] Delete order removes it from list
- [ ] Toast notifications appear for success/error
- [ ] Token expiration redirects to login
- [ ] Responsive design on mobile

## Troubleshooting

### Orders not loading?
1. Check API is running: `http://localhost:5035/api/orders`
2. Verify JWT token is valid (check browser console)
3. Check CORS is enabled in Program.cs

### Status update fails?
1. Verify status value is one of: "Chờ xử lý", "Đang làm", "Đã hoàn thành", "Đã hủy"
2. Check OrderService validation
3. Check database status column allows updates

### Styling issues?
1. Clear browser cache (Ctrl+Shift+Delete)
2. Hard refresh page (Ctrl+Shift+R)
3. Check Font Awesome CDN is loaded

## Future Enhancements

Possibilities for further development:
- Add pagination for large order lists
- Bulk actions (select multiple orders)
- Export orders to CSV
- Order timeline/history tracking
- Real-time updates with SignalR
- Print order receipts
- Advanced date range filtering
- Order notes/comments
- Customer contact details
- Revenue analytics by status

## Notes

- Orders are sorted by date (newest first)
- Dates display in Vietnamese locale format
- Currency formatted with thousands separator (₫)
- All status values are case-insensitive in backend
- Filters work together (AND logic) not separate (OR)
- Toast notifications auto-dismiss after 3 seconds
- Dropdowns close when clicking outside
