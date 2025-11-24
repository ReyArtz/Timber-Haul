using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimberHaul.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:equipment_type", "chainsaw,truck,winch,other")
                .Annotation("Npgsql:Enum:load_status", "pending,on_truck,in_transit,delivered")
                .Annotation("Npgsql:Enum:payment_method", "cash,card,bank_transfer,other")
                .Annotation("Npgsql:Enum:payment_status", "unpaid,paid,overdue")
                .Annotation("Npgsql:Enum:user_role", "forester,delivery,customer")
                .Annotation("Npgsql:Enum:wood_type", "oak,beech,pine,spruce,birch,other");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    role = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "customer_profiles",
                columns: table => new
                {
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    delivery_address = table.Column<string>(type: "text", nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_profiles", x => x.customer_id);
                    table.ForeignKey(
                        name: "FK_customer_profiles_users_customer_id",
                        column: x => x.customer_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "delivery_profiles",
                columns: table => new
                {
                    driver_id = table.Column<Guid>(type: "uuid", nullable: false),
                    license_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    vehicle_plate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    total_deliveries = table.Column<int>(type: "integer", nullable: false),
                    rating = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_profiles", x => x.driver_id);
                    table.ForeignKey(
                        name: "FK_delivery_profiles_users_driver_id",
                        column: x => x.driver_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "equipment",
                columns: table => new
                {
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipment_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    equipment_type = table.Column<int>(type: "integer", nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    runtime_hours = table.Column<int>(type: "integer", nullable: false),
                    last_service_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_service_due = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    service_interval_hours = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment", x => x.equipment_id);
                    table.ForeignKey(
                        name: "FK_equipment_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "forester_profiles",
                columns: table => new
                {
                    forester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    tax_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    total_wood_available = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forester_profiles", x => x.forester_id);
                    table.ForeignKey(
                        name: "FK_forester_profiles_users_forester_id",
                        column: x => x.forester_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_log",
                columns: table => new
                {
                    log_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    maintenance_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    cost = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    performed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    performed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintenance_log", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_maintenance_log_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "equipment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maintenance_log_users_performed_by",
                        column: x => x.performed_by,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "forest_plots",
                columns: table => new
                {
                    plot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    forester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plot_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    location = table.Column<string>(type: "text", nullable: false),
                    total_area = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forest_plots", x => x.plot_id);
                    table.ForeignKey(
                        name: "FK_forest_plots_forester_profiles_forester_id",
                        column: x => x.forester_id,
                        principalTable: "forester_profiles",
                        principalColumn: "forester_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    forester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    wood_type = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    price_per_unit = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    min_order_volume = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    available_stock = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    product_image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_available = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_products_forester_profiles_forester_id",
                        column: x => x.forester_id,
                        principalTable: "forester_profiles",
                        principalColumn: "forester_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cart_items",
                columns: table => new
                {
                    cart_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    volume = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    price_per_unit = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cart_items", x => x.cart_item_id);
                    table.ForeignKey(
                        name: "FK_cart_items_customer_profiles_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer_profiles",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cart_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    forester_id = table.Column<Guid>(type: "uuid", nullable: true),
                    volume = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    price_per_unit = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    delivery_address = table.Column<string>(type: "text", nullable: false),
                    delivery_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    delivery_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    customer_notes = table.Column<string>(type: "text", nullable: true),
                    order_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.order_id);
                    table.ForeignKey(
                        name: "FK_orders_customer_profiles_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer_profiles",
                        principalColumn: "customer_id");
                    table.ForeignKey(
                        name: "FK_orders_forester_profiles_forester_id",
                        column: x => x.forester_id,
                        principalTable: "forester_profiles",
                        principalColumn: "forester_id");
                    table.ForeignKey(
                        name: "FK_orders_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id");
                });

            migrationBuilder.CreateTable(
                name: "wood_inventory",
                columns: table => new
                {
                    inventory_id = table.Column<Guid>(type: "uuid", nullable: false),
                    forester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plot_id = table.Column<Guid>(type: "uuid", nullable: true),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    available_volume = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    reserved_volume = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wood_inventory", x => x.inventory_id);
                    table.ForeignKey(
                        name: "FK_wood_inventory_forest_plots_plot_id",
                        column: x => x.plot_id,
                        principalTable: "forest_plots",
                        principalColumn: "plot_id");
                    table.ForeignKey(
                        name: "FK_wood_inventory_forester_profiles_forester_id",
                        column: x => x.forester_id,
                        principalTable: "forester_profiles",
                        principalColumn: "forester_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_wood_inventory_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "loads",
                columns: table => new
                {
                    load_id = table.Column<Guid>(type: "uuid", nullable: false),
                    load_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    forester_id = table.Column<Guid>(type: "uuid", nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: true),
                    plot_id = table.Column<Guid>(type: "uuid", nullable: true),
                    product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    wood_type = table.Column<int>(type: "integer", nullable: false),
                    volume = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    price_per_cubic_meter = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    delivery_location = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    payment_status = table.Column<int>(type: "integer", nullable: false),
                    before_load_photo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    on_truck_photo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    delivered_photo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    loaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loads", x => x.load_id);
                    table.ForeignKey(
                        name: "FK_loads_customer_profiles_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer_profiles",
                        principalColumn: "customer_id");
                    table.ForeignKey(
                        name: "FK_loads_delivery_profiles_driver_id",
                        column: x => x.driver_id,
                        principalTable: "delivery_profiles",
                        principalColumn: "driver_id");
                    table.ForeignKey(
                        name: "FK_loads_forest_plots_plot_id",
                        column: x => x.plot_id,
                        principalTable: "forest_plots",
                        principalColumn: "plot_id");
                    table.ForeignKey(
                        name: "FK_loads_forester_profiles_forester_id",
                        column: x => x.forester_id,
                        principalTable: "forester_profiles",
                        principalColumn: "forester_id");
                    table.ForeignKey(
                        name: "FK_loads_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "order_id");
                    table.ForeignKey(
                        name: "FK_loads_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id");
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    load_id = table.Column<Guid>(type: "uuid", nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    forester_id = table.Column<Guid>(type: "uuid", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    payment_method = table.Column<int>(type: "integer", nullable: true),
                    payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    invoice_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.payment_id);
                    table.ForeignKey(
                        name: "FK_payments_customer_profiles_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer_profiles",
                        principalColumn: "customer_id");
                    table.ForeignKey(
                        name: "FK_payments_forester_profiles_forester_id",
                        column: x => x.forester_id,
                        principalTable: "forester_profiles",
                        principalColumn: "forester_id");
                    table.ForeignKey(
                        name: "FK_payments_loads_load_id",
                        column: x => x.load_id,
                        principalTable: "loads",
                        principalColumn: "load_id");
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    review_id = table.Column<Guid>(type: "uuid", nullable: false),
                    load_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.review_id);
                    table.ForeignKey(
                        name: "FK_reviews_customer_profiles_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer_profiles",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reviews_delivery_profiles_driver_id",
                        column: x => x.driver_id,
                        principalTable: "delivery_profiles",
                        principalColumn: "driver_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_reviews_loads_load_id",
                        column: x => x.load_id,
                        principalTable: "loads",
                        principalColumn: "load_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cart_items_customer_id_product_id",
                table: "cart_items",
                columns: new[] { "customer_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cart_items_product_id",
                table: "cart_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_owner_id",
                table: "equipment",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_forest_plots_forester_id",
                table: "forest_plots",
                column: "forester_id");

            migrationBuilder.CreateIndex(
                name: "IX_loads_customer_id",
                table: "loads",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_loads_driver_id",
                table: "loads",
                column: "driver_id");

            migrationBuilder.CreateIndex(
                name: "IX_loads_forester_id",
                table: "loads",
                column: "forester_id");

            migrationBuilder.CreateIndex(
                name: "IX_loads_load_number",
                table: "loads",
                column: "load_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loads_order_id",
                table: "loads",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_loads_payment_status",
                table: "loads",
                column: "payment_status");

            migrationBuilder.CreateIndex(
                name: "IX_loads_plot_id",
                table: "loads",
                column: "plot_id");

            migrationBuilder.CreateIndex(
                name: "IX_loads_product_id",
                table: "loads",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_loads_status",
                table: "loads",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_log_equipment_id",
                table: "maintenance_log",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_log_performed_by",
                table: "maintenance_log",
                column: "performed_by");

            migrationBuilder.CreateIndex(
                name: "IX_orders_customer_id",
                table: "orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_forester_id",
                table: "orders",
                column: "forester_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_order_number",
                table: "orders",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_order_status",
                table: "orders",
                column: "order_status");

            migrationBuilder.CreateIndex(
                name: "IX_orders_product_id",
                table: "orders",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_customer_id",
                table: "payments",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_due_date",
                table: "payments",
                column: "due_date");

            migrationBuilder.CreateIndex(
                name: "IX_payments_forester_id",
                table: "payments",
                column: "forester_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_invoice_number",
                table: "payments",
                column: "invoice_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_load_id",
                table: "payments",
                column: "load_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_status",
                table: "payments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_products_forester_id",
                table: "products",
                column: "forester_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_is_available",
                table: "products",
                column: "is_available");

            migrationBuilder.CreateIndex(
                name: "IX_products_product_name",
                table: "products",
                column: "product_name");

            migrationBuilder.CreateIndex(
                name: "IX_products_wood_type",
                table: "products",
                column: "wood_type");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_customer_id",
                table: "reviews",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_driver_id",
                table: "reviews",
                column: "driver_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_load_id",
                table: "reviews",
                column: "load_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wood_inventory_forester_id",
                table: "wood_inventory",
                column: "forester_id");

            migrationBuilder.CreateIndex(
                name: "IX_wood_inventory_plot_id",
                table: "wood_inventory",
                column: "plot_id");

            migrationBuilder.CreateIndex(
                name: "IX_wood_inventory_product_id",
                table: "wood_inventory",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cart_items");

            migrationBuilder.DropTable(
                name: "maintenance_log");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "wood_inventory");

            migrationBuilder.DropTable(
                name: "equipment");

            migrationBuilder.DropTable(
                name: "loads");

            migrationBuilder.DropTable(
                name: "delivery_profiles");

            migrationBuilder.DropTable(
                name: "forest_plots");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "customer_profiles");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "forester_profiles");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
