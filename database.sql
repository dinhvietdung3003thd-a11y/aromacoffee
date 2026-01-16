-- 1. Tạo Database và thiết lập Charset chuẩn tiếng Việt
CREATE DATABASE IF NOT EXISTS AromaCafeDB 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE AromaCafeDB;

-- 2. Bảng Users (Quản lý Admin và Nhân viên)
CREATE TABLE users (
    user_id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(100) NOT NULL,
    role ENUM('Admin', 'Staff') NOT NULL DEFAULT 'Staff',
    phone_number VARCHAR(15),
    is_active BOOLEAN DEFAULT TRUE, -- Dùng để khóa tài khoản thay vì xóa
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_username (username)
) ENGINE=InnoDB;

-- 3. Bảng Customers (Khách hàng thành viên)
CREATE TABLE customers (
    customer_id INT AUTO_INCREMENT PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    phone_number VARCHAR(15) NOT NULL UNIQUE,
    points INT DEFAULT 0, -- Điểm tích lũy
    membership_level VARCHAR(20) DEFAULT 'Standard', -- Hạng thành viên
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_phone (phone_number)
) ENGINE=InnoDB;
-- Thêm cột tài khoản và mật khẩu cho khách hàng
ALTER TABLE customers 
ADD COLUMN username VARCHAR(50) UNIQUE AFTER full_name,
ADD COLUMN password_hash VARCHAR(255) AFTER username;

-- 4. Bảng Categories (Danh mục món)
CREATE TABLE categories (
    category_id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- 5. Bảng Products (Món uống/đồ ăn)
CREATE TABLE products (
    product_id INT AUTO_INCREMENT PRIMARY KEY,
    category_id INT NOT NULL,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    price DECIMAL(15, 2) NOT NULL, -- Giá tiền (VND thường lớn nên để 15 số)
    image_url VARCHAR(255),
    is_available BOOLEAN DEFAULT TRUE, -- Còn bán hay ngừng kinh doanh
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (category_id) REFERENCES categories(category_id) ON DELETE RESTRICT,
    INDEX idx_product_name (name)
) ENGINE=InnoDB;

-- 6. Bảng Inventory (Nguyên liệu tồn kho)
CREATE TABLE inventory (
    ingredient_id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    unit VARCHAR(20) NOT NULL, -- Đơn vị tính (kg, lít, hộp...)
    quantity_in_stock DECIMAL(10, 2) DEFAULT 0,
    min_threshold DECIMAL(10, 2) DEFAULT 5, -- Mức cảnh báo sắp hết hàng
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- 7. Bảng Tables (Bàn - Tùy chọn nhưng cần thiết cho quán Cafe)
CREATE TABLE tables (
    table_id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE, -- Ví dụ: Bàn 1, Bàn 2, VIP 1
    status ENUM('Available', 'Occupied', 'Reserved') DEFAULT 'Available'
) ENGINE=InnoDB;

-- 8. Bảng Orders (Đơn hàng)
CREATE TABLE orders (
    order_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL, -- Nhân viên tạo đơn
    customer_id INT, -- Có thể NULL nếu là khách vãng lai
    table_id INT, -- Có thể NULL nếu mang về (Take away)
    total_amount DECIMAL(15, 2) NOT NULL DEFAULT 0,
    status ENUM('Pending', 'Completed', 'Cancelled') DEFAULT 'Pending',
    note TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE RESTRICT,
    FOREIGN KEY (customer_id) REFERENCES customers(customer_id) ON DELETE SET NULL,
    FOREIGN KEY (table_id) REFERENCES tables(table_id) ON DELETE SET NULL,
    INDEX idx_order_date (created_at)
) ENGINE=InnoDB;

-- 9. Bảng OrderDetails (Chi tiết đơn hàng)
CREATE TABLE order_details (
    order_detail_id INT AUTO_INCREMENT PRIMARY KEY,
    order_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL CHECK (quantity > 0),
    unit_price DECIMAL(15, 2) NOT NULL, -- Lưu giá tại thời điểm bán (đề phòng giá gốc thay đổi)
    subtotal DECIMAL(15, 2) NOT NULL,
    FOREIGN KEY (order_id) REFERENCES orders(order_id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE suppliers (
    supplier_id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    contact_name VARCHAR(100),
    phone VARCHAR(20),
    email VARCHAR(100),
    address TEXT
) ENGINE=InnoDB;

CREATE TABLE inventory_transactions (
    transaction_id INT AUTO_INCREMENT PRIMARY KEY,
    inventory_id INT NOT NULL,
    -- ENUM giúp giới hạn chỉ có 2 loại giao dịch: Nhập (Import) hoặc Xuất (Export)
    transaction_type ENUM('Import', 'Export') NOT NULL,
    quantity DECIMAL(15, 2) NOT NULL,
    transaction_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    user_id INT NOT NULL, -- ID của nhân viêinventory_transactionsinventoryn thực hiện giao dịch
    note TEXT,
    -- Khóa ngoại liên kết tới bảng kho và bảng nhân viên
    FOREIGN KEY (inventory_id) REFERENCES inventory(inventory_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE RESTRICT
) ENGINE=InnoDB;
-- Đổi tên từ ingredient_id thành inventory_id
ALTER TABLE inventory CHANGE ingredient_id inventory_id INT AUTO_INCREMENT;

		CREATE TABLE recipes (
			recipe_id INT AUTO_INCREMENT PRIMARY KEY,
			product_id INT NOT NULL, -- Liên kết tới bảng sản phẩm
			inventory_id INT NOT NULL, -- Liên kết tới bảng kho
			quantity_needed DECIMAL(10, 2) NOT NULL, -- Lượng nguyên liệu cần cho 1 đơn vị sản phẩm
			FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE,
			FOREIGN KEY (inventory_id) REFERENCES inventory(inventory_id) ON DELETE CASCADE
		) ENGINE=InnoDB;