-- Add missing columns to existing tables

-- Add missing columns to Products table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'IsPrintifyProduct')
    ALTER TABLE [Products] ADD [IsPrintifyProduct] bit NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'LastSyncedAt')
    ALTER TABLE [Products] ADD [LastSyncedAt] datetime2 NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'PrintifyProductId')
    ALTER TABLE [Products] ADD [PrintifyProductId] nvarchar(max) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'PrintifyShopId')
    ALTER TABLE [Products] ADD [PrintifyShopId] nvarchar(max) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'PrintifyVariantData')
    ALTER TABLE [Products] ADD [PrintifyVariantData] nvarchar(max) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'AdditionalImages')
    ALTER TABLE [Products] ADD [AdditionalImages] nvarchar(max) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'PrintifyOptionsData')
    ALTER TABLE [Products] ADD [PrintifyOptionsData] nvarchar(max) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'Visible')
    ALTER TABLE [Products] ADD [Visible] bit NOT NULL DEFAULT 1;

-- Add missing columns to AspNetUsers table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'City')
    ALTER TABLE [AspNetUsers] ADD [City] nvarchar(max) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Name')
    ALTER TABLE [AspNetUsers] ADD [Name] nvarchar(max) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'PostalCode')
    ALTER TABLE [AspNetUsers] ADD [PostalCode] nvarchar(max) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'State')
    ALTER TABLE [AspNetUsers] ADD [State] nvarchar(max) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'StreetAddress')
    ALTER TABLE [AspNetUsers] ADD [StreetAddress] nvarchar(max) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Country')
    ALTER TABLE [AspNetUsers] ADD [Country] nvarchar(max) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'CompanyId')
    ALTER TABLE [AspNetUsers] ADD [CompanyId] int NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'StripeCustomerId')
    ALTER TABLE [AspNetUsers] ADD [StripeCustomerId] nvarchar(max) NULL;

-- Add missing columns to ShoppingCarts table
IF OBJECT_ID('ShoppingCarts') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ShoppingCarts') AND name = 'OrderTotal')
        ALTER TABLE [ShoppingCarts] ADD [OrderTotal] decimal(18,2) NOT NULL DEFAULT 0;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ShoppingCarts') AND name = 'SessionId')
        ALTER TABLE [ShoppingCarts] ADD [SessionId] nvarchar(max) NULL;
END

-- Add missing columns to OrderHeaders table
IF OBJECT_ID('OrderHeaders') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('OrderHeaders') AND name = 'SessionId')
        ALTER TABLE [OrderHeaders] ADD [SessionId] nvarchar(max) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('OrderHeaders') AND name = 'StripeCustomerId')
        ALTER TABLE [OrderHeaders] ADD [StripeCustomerId] nvarchar(max) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('OrderHeaders') AND name = 'PrintifyOrderId')
        ALTER TABLE [OrderHeaders] ADD [PrintifyOrderId] nvarchar(max) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('OrderHeaders') AND name = 'SentToPrintify')
        ALTER TABLE [OrderHeaders] ADD [SentToPrintify] bit NOT NULL DEFAULT 0;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('OrderHeaders') AND name = 'SentToPrintifyAt')
        ALTER TABLE [OrderHeaders] ADD [SentToPrintifyAt] datetime2 NULL;
END

-- Create migration history table if not exists
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

-- Record migrations as applied (only if not already recorded)
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260204151019_AddCategoryTableDb')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260204151019_AddCategoryTableDb', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260205084240_SeedCategoryData')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260205084240_SeedCategoryData', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260208130201_AddProductsToDb')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260208130201_AddProductsToDb', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260208132124_UpdatePrices')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260208132124_UpdatePrices', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260208140739_AddForeignKeyForCatProductRelation')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260208140739_AddForeignKeyForCatProductRelation', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260211143759_addIdentityTables')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260211143759_addIdentityTables', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260211145327_extendIdentityUser')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260211145327_extendIdentityUser', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260212152636_updateUser')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260212152636_updateUser', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260213083614_addShoppingcartToDb')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260213083614_addShoppingcartToDb', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260213114127_addProductImages')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260213114127_addProductImages', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260227090554_addOrderHeaderAndOrderDetailsToDb')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260227090554_addOrderHeaderAndOrderDetailsToDb', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260227104341_syncDatabaseSchema')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260227104341_syncDatabaseSchema', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260227110106_updateShoppingCartAndApplicationUser')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260227110106_updateShoppingCartAndApplicationUser', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260227112837_AddCompanyIdToApplicationUser')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260227112837_AddCompanyIdToApplicationUser', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260227125727_addSessionIdToOrderHeader')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260227125727_addSessionIdToOrderHeader', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260227140158_addStripeUpdateAndWebhooks')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260227140158_addStripeUpdateAndWebhooks', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260227140908_addStripeCustomerId')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260227140908_addStripeCustomerId', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260302121130_addPrintifyApiSetup')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260302121130_addPrintifyApiSetup', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260303095616_AdditionalImagesfield')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260303095616_AdditionalImagesfield', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260303112104_AddPrintifyOptionData')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260303112104_AddPrintifyOptionData', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260303130437_UpdatePrintifyPublishingError')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260303130437_UpdatePrintifyPublishingError', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260303131334_AddVisibleFieldToProduct')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260303131334_AddVisibleFieldToProduct', N'10.0.0');

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260304152830_AddSessionIdToShoppingCart')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260304152830_AddSessionIdToShoppingCart', N'10.0.0');
