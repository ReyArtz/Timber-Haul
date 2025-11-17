-- TIMBER HAUL DATABASE SCHEMA

CREATE TYPE user_role AS ENUM ('forester', 'delivery', 'customer');
CREATE TYPE load_status AS ENUM ('pending', 'on_truck', 'in_transit', 'delivered');
CREATE TYPE payment_status AS ENUM ('unpaid', 'paid', 'overdue');
CREATE TYPE payment_method AS ENUM ('cash', 'card', 'bank_transfer', 'other');
CREATE TYPE equipment_type AS ENUM ('chainsaw', 'truck', 'winch', 'other');
CREATE TYPE wood_type AS ENUM ('oak', 'beech', 'pine', 'spruce', 'birch', 'other');

-- Users
CREATE TABLE users (
    user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    role user_role NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Forester Profiles
CREATE TABLE forester_profiles (
    forester_id UUID PRIMARY KEY REFERENCES users(user_id) ON DELETE CASCADE,
    company_name VARCHAR(255),
    tax_id VARCHAR(50),
    total_wood_available DECIMAL(10, 2) DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Delivery Driver Profiles
CREATE TABLE delivery_profiles (
    driver_id UUID PRIMARY KEY REFERENCES users(user_id) ON DELETE CASCADE,
    license_number VARCHAR(50),
    vehicle_plate VARCHAR(20),
    total_deliveries INTEGER DEFAULT 0,
    rating DECIMAL(3, 2) DEFAULT 0.00,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Customer Profiles
CREATE TABLE customer_profiles (
    customer_id UUID PRIMARY KEY REFERENCES users(user_id) ON DELETE CASCADE,
    delivery_address TEXT,
    city VARCHAR(100),
    postal_code VARCHAR(20),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Forest Plots (Wood Ramps/Cutting Areas)
CREATE TABLE forest_plots (
    plot_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    forester_id UUID REFERENCES forester_profiles(forester_id) ON DELETE CASCADE,
    plot_name VARCHAR(255) NOT NULL,
    location TEXT NOT NULL,
    total_area DECIMAL(10, 2),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Products (Wood Products for Sale)
CREATE TABLE products (
    product_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    forester_id UUID REFERENCES forester_profiles(forester_id) ON DELETE CASCADE,
    product_name VARCHAR(255) NOT NULL,
    wood_type wood_type NOT NULL,
    description TEXT,
    price_per_unit DECIMAL(10, 2) NOT NULL,
    min_order_volume DECIMAL(10, 2) DEFAULT 1.0,
    available_stock DECIMAL(10, 2) NOT NULL,
    product_image VARCHAR(500),
    is_available BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Wood Inventory (Stock by Plot)
CREATE TABLE wood_inventory (
    inventory_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    forester_id UUID REFERENCES forester_profiles(forester_id) ON DELETE CASCADE,
    plot_id UUID REFERENCES forest_plots(plot_id) ON DELETE SET NULL,
    product_id UUID REFERENCES products(product_id) ON DELETE CASCADE,
    available_volume DECIMAL(10, 2) NOT NULL,
    reserved_volume DECIMAL(10, 2) DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Shopping Cart
CREATE TABLE cart_items (
    cart_item_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id UUID REFERENCES customer_profiles(customer_id) ON DELETE CASCADE,
    product_id UUID REFERENCES products(product_id) ON DELETE CASCADE,
    volume DECIMAL(10, 2) NOT NULL,
    price_per_unit DECIMAL(10, 2) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(customer_id, product_id)
);

-- Orders
CREATE TABLE orders (
    order_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_number VARCHAR(50) UNIQUE NOT NULL,
    customer_id UUID REFERENCES customer_profiles(customer_id) ON DELETE SET NULL,
    product_id UUID REFERENCES products(product_id) ON DELETE SET NULL,
    forester_id UUID REFERENCES forester_profiles(forester_id) ON DELETE SET NULL,
    volume DECIMAL(10, 2) NOT NULL,
    price_per_unit DECIMAL(10, 2) NOT NULL,
    total_amount DECIMAL(10, 2) NOT NULL,
    delivery_address TEXT NOT NULL,
    delivery_city VARCHAR(100),
    delivery_postal_code VARCHAR(20),
    customer_notes TEXT,
    order_status VARCHAR(50) DEFAULT 'pending',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    confirmed_at TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Loads (Timber Deliveries)
CREATE TABLE loads (
    load_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    load_number VARCHAR(50) UNIQUE NOT NULL,
    order_id UUID REFERENCES orders(order_id) ON DELETE SET NULL,
    forester_id UUID REFERENCES forester_profiles(forester_id) ON DELETE SET NULL,
    customer_id UUID REFERENCES customer_profiles(customer_id) ON DELETE SET NULL,
    driver_id UUID REFERENCES delivery_profiles(driver_id) ON DELETE SET NULL,
    plot_id UUID REFERENCES forest_plots(plot_id) ON DELETE SET NULL,
    product_id UUID REFERENCES products(product_id) ON DELETE SET NULL,
    wood_type wood_type NOT NULL,
    volume DECIMAL(10, 2) NOT NULL,
    price_per_cubic_meter DECIMAL(10, 2) NOT NULL,
    total_amount DECIMAL(10, 2) NOT NULL,
    delivery_location TEXT NOT NULL,
    notes TEXT,
    status load_status DEFAULT 'pending',
    payment_status payment_status DEFAULT 'unpaid',
    before_load_photo VARCHAR(500),
    on_truck_photo VARCHAR(500),
    delivered_photo VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    loaded_at TIMESTAMP,
    delivered_at TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Payments/Invoices
CREATE TABLE payments (
    payment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    load_id UUID REFERENCES loads(load_id) ON DELETE CASCADE,
    customer_id UUID REFERENCES customer_profiles(customer_id) ON DELETE SET NULL,
    forester_id UUID REFERENCES forester_profiles(forester_id) ON DELETE SET NULL,
    amount DECIMAL(10, 2) NOT NULL,
    payment_method payment_method,
    payment_date TIMESTAMP,
    due_date TIMESTAMP NOT NULL,
    status payment_status DEFAULT 'unpaid',
    invoice_number VARCHAR(50) UNIQUE,
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Reviews
CREATE TABLE reviews (
    review_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    load_id UUID REFERENCES loads(load_id) ON DELETE CASCADE,
    customer_id UUID REFERENCES customer_profiles(customer_id) ON DELETE CASCADE,
    driver_id UUID REFERENCES delivery_profiles(driver_id) ON DELETE SET NULL,
    rating INTEGER NOT NULL CHECK (rating >= 1 AND rating <= 5),
    comment TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Equipment
CREATE TABLE equipment (
    equipment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    owner_id UUID REFERENCES users(user_id) ON DELETE CASCADE,
    equipment_name VARCHAR(255) NOT NULL,
    equipment_type equipment_type NOT NULL,
    model VARCHAR(100),
    runtime_hours INTEGER DEFAULT 0,
    last_service_date DATE,
    next_service_due DATE,
    service_interval_hours INTEGER,
    is_active BOOLEAN DEFAULT true,
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Maintenance Log
CREATE TABLE maintenance_log (
    log_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    equipment_id UUID REFERENCES equipment(equipment_id) ON DELETE CASCADE,
    maintenance_type VARCHAR(100) NOT NULL,
    description TEXT,
    cost DECIMAL(10, 2),
    performed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    performed_by UUID REFERENCES users(user_id) ON DELETE SET NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Indexes
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_role ON users(role);
CREATE INDEX idx_products_forester ON products(forester_id);
CREATE INDEX idx_products_wood_type ON products(wood_type);
CREATE INDEX idx_products_available ON products(is_available);
CREATE INDEX idx_orders_customer ON orders(customer_id);
CREATE INDEX idx_orders_status ON orders(order_status);
CREATE INDEX idx_cart_customer ON cart_items(customer_id);
CREATE INDEX idx_loads_forester ON loads(forester_id);
CREATE INDEX idx_loads_customer ON loads(customer_id);
CREATE INDEX idx_loads_driver ON loads(driver_id);
CREATE INDEX idx_loads_status ON loads(status);
CREATE INDEX idx_loads_payment_status ON loads(payment_status);
CREATE INDEX idx_payments_status ON payments(status);
CREATE INDEX idx_payments_due_date ON payments(due_date);
CREATE INDEX idx_equipment_owner ON equipment(owner_id);
CREATE INDEX idx_wood_inventory_forester ON wood_inventory(forester_id);
CREATE INDEX idx_forest_plots_forester ON forest_plots(forester_id);

-- Auto-update timestamp trigger
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_forester_profiles_updated_at BEFORE UPDATE ON forester_profiles
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_delivery_profiles_updated_at BEFORE UPDATE ON delivery_profiles
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_customer_profiles_updated_at BEFORE UPDATE ON customer_profiles
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_products_updated_at BEFORE UPDATE ON products
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_orders_updated_at BEFORE UPDATE ON orders
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_cart_items_updated_at BEFORE UPDATE ON cart_items
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_wood_inventory_updated_at BEFORE UPDATE ON wood_inventory
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_loads_updated_at BEFORE UPDATE ON loads
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_payments_updated_at BEFORE UPDATE ON payments
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_equipment_updated_at BEFORE UPDATE ON equipment
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();