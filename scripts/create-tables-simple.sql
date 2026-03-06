-- Simple script to create all tables for CodersGear
-- Run this in SmarterASP SQL Server management tool

-- Create migration history table
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

-- Create Categories table
IF OBJECT_ID(N'[Categories]') IS NULL
BEGIN
    CREATE TABLE [Categories] (
        [CategoryId] int NOT NULL IDENTITY,
        [Name] nvarchar(30) NOT NULL,
        [DisplayOrder] int NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([CategoryId])
    );
END;

-- Create Products table
IF OBJECT_ID(N'[Products]') IS NULL
BEGIN
    CREATE TABLE [Products] (
        [ProductId] int NOT NULL IDENTITY,
        [ProductName] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [UPC] nvarchar(max) NOT NULL,
        [CategoryId] int NOT NULL,
        [ListPrice] decimal(18,2) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [Price50] decimal(18,2) NOT NULL,
        [Price100] decimal(18,2) NOT NULL,
        [ImageUrl] nvarchar(max) NULL,
        [IsPrintifyProduct] bit NOT NULL DEFAULT 0,
        [LastSyncedAt] datetime2 NULL,
        [PrintifyProductId] nvarchar(max) NULL,
        [PrintifyShopId] nvarchar(max) NULL,
        [PrintifyVariantData] nvarchar(max) NULL,
        [AdditionalImages] nvarchar(max) NULL,
        [PrintifyOptionsData] nvarchar(max) NULL,
        [Visible] bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Products] PRIMARY KEY ([ProductId])
    );
END;

-- Create AspNetRoles table
IF OBJECT_ID(N'[AspNetRoles]') IS NULL
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

-- Create AspNetUsers table
IF OBJECT_ID(N'[AspNetUsers]') IS NULL
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        [City] nvarchar(max) NULL,
        [Name] nvarchar(max) NULL,
        [PostalCode] nvarchar(max) NULL,
        [State] nvarchar(max) NULL,
        [StreetAddress] nvarchar(max) NULL,
        [Country] nvarchar(max) NULL,
        [CompanyId] int NULL,
        [StripeCustomerId] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

-- Create AspNetRoleClaims table
IF OBJECT_ID(N'[AspNetRoleClaims]') IS NULL
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

-- Create AspNetUserClaims table
IF OBJECT_ID(N'[AspNetUserClaims]') IS NULL
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

-- Create AspNetUserLogins table
IF OBJECT_ID(N'[AspNetUserLogins]') IS NULL
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

-- Create AspNetUserRoles table
IF OBJECT_ID(N'[AspNetUserRoles]') IS NULL
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

-- Create AspNetUserTokens table
IF OBJECT_ID(N'[AspNetUserTokens]') IS NULL
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

-- Create ShoppingCarts table
IF OBJECT_ID(N'[ShoppingCarts]') IS NULL
BEGIN
    CREATE TABLE [ShoppingCarts] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [Count] int NOT NULL,
        [ApplicationUserId] nvarchar(450) NOT NULL,
        [OrderTotal] decimal(18,2) NOT NULL DEFAULT 0,
        [SessionId] nvarchar(max) NULL,
        CONSTRAINT [PK_ShoppingCarts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ShoppingCarts_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ShoppingCarts_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]) ON DELETE CASCADE
    );
END;

-- Create OrderHeaders table
IF OBJECT_ID(N'[OrderHeaders]') IS NULL
BEGIN
    CREATE TABLE [OrderHeaders] (
        [Id] int NOT NULL IDENTITY,
        [ApplicationUserId] nvarchar(450) NOT NULL,
        [OrderDate] datetime2 NOT NULL,
        [ShippingDate] datetime2 NOT NULL,
        [OrderTotal] decimal(18,2) NOT NULL,
        [OrderStatus] nvarchar(max) NULL,
        [PaymentStatus] nvarchar(max) NULL,
        [TrackingNumber] nvarchar(max) NULL,
        [Carrier] nvarchar(max) NULL,
        [PaymentDate] datetime2 NOT NULL,
        [PaymentDueDate] date NOT NULL,
        [PaymentIntentId] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NOT NULL,
        [StreetAddress] nvarchar(max) NOT NULL,
        [City] nvarchar(max) NOT NULL,
        [State] nvarchar(max) NOT NULL,
        [PostalCode] nvarchar(max) NOT NULL,
        [Country] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [SessionId] nvarchar(max) NULL,
        [StripeCustomerId] nvarchar(max) NULL,
        [PrintifyOrderId] nvarchar(max) NULL,
        [SentToPrintify] bit NOT NULL DEFAULT 0,
        [SentToPrintifyAt] datetime2 NULL,
        CONSTRAINT [PK_OrderHeaders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderHeaders_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

-- Create OrderDetails table
IF OBJECT_ID(N'[OrderDetails]') IS NULL
BEGIN
    CREATE TABLE [OrderDetails] (
        [Id] int NOT NULL IDENTITY,
        [OrderHeaderId] int NOT NULL,
        [ProductId] int NOT NULL,
        [Count] int NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_OrderDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderDetails_OrderHeaders_OrderHeaderId] FOREIGN KEY ([OrderHeaderId]) REFERENCES [OrderHeaders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderDetails_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]) ON DELETE CASCADE
    );
END;

-- Create indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_CategoryId' AND object_id = OBJECT_ID('Products'))
    CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'FK_Products_Categories_CategoryId' AND object_id = OBJECT_ID('Products'))
    ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([CategoryId]) ON DELETE CASCADE;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetRoleClaims_RoleId' AND object_id = OBJECT_ID('AspNetRoleClaims'))
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'RoleNameIndex' AND object_id = OBJECT_ID('AspNetRoles'))
    CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUserClaims_UserId' AND object_id = OBJECT_ID('AspNetUserClaims'))
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUserLogins_UserId' AND object_id = OBJECT_ID('AspNetUserLogins'))
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUserRoles_RoleId' AND object_id = OBJECT_ID('AspNetUserRoles'))
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'EmailIndex' AND object_id = OBJECT_ID('AspNetUsers'))
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UserNameIndex' AND object_id = OBJECT_ID('AspNetUsers'))
    CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ShoppingCarts_ApplicationUserId' AND object_id = OBJECT_ID('ShoppingCarts'))
    CREATE INDEX [IX_ShoppingCarts_ApplicationUserId] ON [ShoppingCarts] ([ApplicationUserId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ShoppingCarts_ProductId' AND object_id = OBJECT_ID('ShoppingCarts'))
    CREATE INDEX [IX_ShoppingCarts_ProductId] ON [ShoppingCarts] ([ProductId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OrderDetails_OrderHeaderId' AND object_id = OBJECT_ID('OrderDetails'))
    CREATE INDEX [IX_OrderDetails_OrderHeaderId] ON [OrderDetails] ([OrderHeaderId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OrderDetails_ProductId' AND object_id = OBJECT_ID('OrderDetails'))
    CREATE INDEX [IX_OrderDetails_ProductId] ON [OrderDetails] ([ProductId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OrderHeaders_ApplicationUserId' AND object_id = OBJECT_ID('OrderHeaders'))
    CREATE INDEX [IX_OrderHeaders_ApplicationUserId] ON [OrderHeaders] ([ApplicationUserId]);

-- Seed Categories data
IF NOT EXISTS (SELECT * FROM [Categories])
BEGIN
    SET IDENTITY_INSERT [Categories] ON;
    INSERT INTO [Categories] ([CategoryId], [DisplayOrder], [Name])
    VALUES (1, 1, N'T-shirts'),
    (2, 2, N'Hoodies'),
    (3, 3, N'Mugs'),
    (4, 4, N'Accessories');
    SET IDENTITY_INSERT [Categories] OFF;
END;

-- Seed Products data
IF NOT EXISTS (SELECT * FROM [Products])
BEGIN
    SET IDENTITY_INSERT [Products] ON;
    INSERT INTO [Products] ([ProductId], [ProductName], [Description], [UPC], [CategoryId], [ListPrice], [Price], [Price50], [Price100], [ImageUrl], [IsPrintifyProduct], [Visible])
    VALUES
    (1, N'Coder''s Gear T-shirt', N'A comfortable and stylish t-shirt for coders.', N'123456789012', 1, 28.99, 23.99, 21.99, 18.99, N'/images/product/8ab587da-7087-4790-848b-44882097a3e0.jpg', 0, 1),
    (2, N'Coder''s Gear Hoodie', N'A cozy hoodie for coders to stay warm while coding.', N'123456789013', 2, 44.99, 39.99, 36.99, 32.99, N'/images/product/a49a695e-cb70-442a-970b-0a4aed5e064d.jpg', 0, 1),
    (3, N'Coder''s Gear Mug', N'A mug for coders to enjoy their favorite beverages while coding.', N'123456789014', 3, 19.99, 14.99, 12.99, 9.99, N'/images/product/da05ef9c-05f7-4554-a5cf-02088b53c821.jpg', 0, 1),
    (4, N'Coder''s Gear Laptop Sleeve', N'A protective laptop sleeve for coders to carry their laptops in style.', N'123456789015', 4, 29.99, 24.99, 22.99, 19.99, N'/images/product/e603a324-fbf8-4c9a-9e5c-0f7ff3141cf4.jpg', 0, 1);
    SET IDENTITY_INSERT [Products] OFF;
END;

-- Record migrations as applied
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES
(N'20260204151019_AddCategoryTableDb', N'10.0.0'),
(N'20260205084240_SeedCategoryData', N'10.0.0'),
(N'20260208130201_AddProductsToDb', N'10.0.0'),
(N'20260208132124_UpdatePrices', N'10.0.0'),
(N'20260208140739_AddForeignKeyForCatProductRelation', N'10.0.0'),
(N'20260211143759_addIdentityTables', N'10.0.0'),
(N'20260211145327_extendIdentityUser', N'10.0.0'),
(N'20260212152636_updateUser', N'10.0.0'),
(N'20260213083614_addShoppingcartToDb', N'10.0.0'),
(N'20260213114127_addProductImages', N'10.0.0'),
(N'20260227090554_addOrderHeaderAndOrderDetailsToDb', N'10.0.0'),
(N'20260227104341_syncDatabaseSchema', N'10.0.0'),
(N'20260227110106_updateShoppingCartAndApplicationUser', N'10.0.0'),
(N'20260227112837_AddCompanyIdToApplicationUser', N'10.0.0'),
(N'20260227125727_addSessionIdToOrderHeader', N'10.0.0'),
(N'20260227140158_addStripeUpdateAndWebhooks', N'10.0.0'),
(N'20260227140908_addStripeCustomerId', N'10.0.0'),
(N'20260302121130_addPrintifyApiSetup', N'10.0.0'),
(N'20260303095616_AdditionalImagesfield', N'10.0.0'),
(N'20260303112104_AddPrintifyOptionData', N'10.0.0'),
(N'20260303130437_UpdatePrintifyPublishingError', N'10.0.0'),
(N'20260303131334_AddVisibleFieldToProduct', N'10.0.0'),
(N'20260304152830_AddSessionIdToShoppingCart', N'10.0.0');
