# Quick Reference - Orders Management

## URLs & Ports

```
Frontend:    http://localhost:5500/admin_dashboard.html  (or file://)
Backend API: http://localhost:5035/api
```

## Menu Navigation

```
Sidebar > Commerce > Orders
```

## API Endpoints Available

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/api/orders` | Fetch all orders |
| GET | `/api/orders/{id}` | Get single order with details |
| GET | `/api/orders/search?q=query` | Search orders |
| GET | `/api/orders/table/{tableNumber}` | Get orders by table |
| POST | `/api/orders` | Create new order |
| PUT | `/api/orders/{id}` | Update entire order |
| PATCH | `/api/orders/{id}/status` | Update only status |
| DELETE | `/api/orders/{id}` | Delete order |

## Status Values (Case-Insensitive)

```
✓ "Chờ xử lý"     (Pending)
✓ "Đang làm"      (Preparing)
✓ "Đã hoàn thành" (Completed)
✓ "Đã hủy"        (Cancelled)
```

## Column Names in Response

```
id                  Order ID (int)
orderDate          Created timestamp (DateTime)
totalAmount        Total price (decimal)
tableId            Table number (int)
status             Current status (string)
creatorFullName    Staff name (string)
userId             Creator ID (int)
customerId         Customer ID (int)
details            Array of OrderDetailDTO
  ├─ orderDetailId
  ├─ productId
  ├─ productName
  ├─ quantity
  ├─ unitPrice
  └─ totalPrice
```

## Table Filter Options

```
B1, B2, B3, B4, B5, B6
(Edit filter-group select for more options)
```

## Filter Logic

```
Filters combine with AND logic:
IF (tableFilter) AND (statusFilter) AND (searchTerm) THEN show order
```

## Authentication

```
Location: localStorage.user.token
Format: JWT Bearer Token
Expires: Based on backend configuration
Auto-redirect: Login page if 401 Unauthorized
```

## JavaScript Functions Available

```javascript
loadOrders()              // Fetch and display all orders
renderOrdersTable(data)   // Render HTML table from data
applyFilters()           // Apply all active filters
updateOrderStatus(id)    // Show status update dialog
setOrderStatus(id, status) // Update status via API
viewOrderDetails(id)     // Display order details
deleteOrder(id)          // Delete with confirmation
toggleDropdown(id)       // Show/hide action menu
showToast(msg, type)     // Display notification
showOrdersPage()         // Switch to orders section
showDashboard()          // Switch to dashboard section
```

## Debug Console Commands

```javascript
// View all orders in memory
console.log(allOrders);

// Test API call
fetch('http://localhost:5035/api/orders', {
  headers: {
    'Authorization': `Bearer ${getToken()}`
  }
}).then(r => r.json()).then(d => console.log(d));

// Get current token
console.log(getToken());

// View current user
console.log(window.currentUser);

// Manually load orders
loadOrders();

// Test filter
applyFilters();
```

## Common Issues & Fixes

| Issue | Solution |
|-------|----------|
| Orders won't load | Check API running: http://localhost:5035/api/orders |
| 401 Unauthorized | Clear localStorage, login again |
| CORS errors | Verify CORS enabled in Program.cs for current origin |
| Status update fails | Verify value is exact: "Chờ xử lý", etc. |
| Styles broken | Hard refresh: Ctrl+Shift+R |
| Tables not showing | Check Font Awesome CDN loaded |
| Filters not working | Check Input IDs match JavaScript selectors |

## Useful Database Queries

```sql
-- Check orders in DB
SELECT * FROM orders ORDER BY created_at DESC;

-- Check order status values used
SELECT DISTINCT status FROM orders;

-- Check table numbers
SELECT DISTINCT table_id FROM orders WHERE table_id IS NOT NULL;

-- Get orders by status
SELECT * FROM orders WHERE status = 'Chờ xử lý';

-- Count orders by status
SELECT status, COUNT(*) as count FROM orders GROUP BY status;
```

## Files to Check/Edit

| File | Purpose |
|------|---------|
| `HTML/admin_dashboard.html` | Frontend source (edit and redeploy) |
| `wwwroot/admin_dashboard.html` | Frontend deployed (served to clients) |
| `Common/StatusConstants.cs` | Valid status values |
| `services/OrderService.cs` | Order business logic |
| `Controllers/OrdersController.cs` | API endpoints |
| `DTOs/order/OrderDisplayDTO.cs` | Response structure |

## Performance Tips

- Orders list loads automatically on page navigation
- Filters applied client-side (no new API calls)
- Max display: all orders returned by API
- For large datasets: consider pagination
- Toast notifications auto-dismiss (no manual close needed)

## Mobile Responsive

- Sidebar collapses on < 960px width
- Table scrolls horizontally on small screens
- Filter controls stack vertically
- Buttons remain functional on touch devices

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| Enter | Execute search (when cursor in search box) |
| Esc | Close dropdown (implement if needed) |
| Tab | Navigate between elements |

## Default Values

```
API Base URL: http://localhost:5035/api
Default Status: "Chờ xử lý"
Date Format: Vietnamese (vi-VN)
Currency: Vietnamese Đồng (₫)
Toast Duration: 3000ms
Table Fetch Format: DD/MM/YYYY HH:mm:ss
```
