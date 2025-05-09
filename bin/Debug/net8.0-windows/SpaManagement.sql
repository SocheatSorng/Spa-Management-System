USE [master]
GO
/****** Object:  Database [SpaManagement]    Script Date: 3/31/2025 4:27:02 PM ******/
CREATE DATABASE [SpaManagement]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'SpaManagement', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLEXPRESS2022\MSSQL\DATA\SpaManagement.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'SpaManagement_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLEXPRESS2022\MSSQL\DATA\SpaManagement_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [SpaManagement] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [SpaManagement].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [SpaManagement] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [SpaManagement] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [SpaManagement] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [SpaManagement] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [SpaManagement] SET ARITHABORT OFF 
GO
ALTER DATABASE [SpaManagement] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [SpaManagement] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [SpaManagement] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [SpaManagement] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [SpaManagement] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [SpaManagement] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [SpaManagement] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [SpaManagement] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [SpaManagement] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [SpaManagement] SET  DISABLE_BROKER 
GO
ALTER DATABASE [SpaManagement] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [SpaManagement] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [SpaManagement] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [SpaManagement] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [SpaManagement] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [SpaManagement] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [SpaManagement] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [SpaManagement] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [SpaManagement] SET  MULTI_USER 
GO
ALTER DATABASE [SpaManagement] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [SpaManagement] SET DB_CHAINING OFF 
GO
ALTER DATABASE [SpaManagement] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [SpaManagement] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [SpaManagement] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [SpaManagement] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [SpaManagement] SET QUERY_STORE = ON
GO
ALTER DATABASE [SpaManagement] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [SpaManagement]
GO
/****** Object:  Table [dbo].[tbCard]    Script Date: 3/31/2025 4:27:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbCard](
	[CardId] [varchar](255) NOT NULL,
	[Status] [varchar](20) NULL,
	[LastUsed] [datetime] NULL,
	[CreatedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[CardId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbConsumable]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbConsumable](
	[ConsumableId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Description] [text] NULL,
	[Price] [decimal](10, 2) NOT NULL,
	[Category] [varchar](50) NULL,
	[StockQuantity] [int] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[ModifiedDate] [datetime] NULL,
	[ImagePath] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[ConsumableId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbCustomer]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbCustomer](
	[CustomerId] [int] IDENTITY(1,1) NOT NULL,
	[CardId] [varchar](255) NOT NULL,
	[IssuedTime] [datetime] NULL,
	[ReleasedTime] [datetime] NULL,
	[Notes] [text] NULL,
PRIMARY KEY CLUSTERED 
(
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbInvoice]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbInvoice](
	[InvoiceId] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NOT NULL,
	[InvoiceDate] [datetime] NULL,
	[TotalAmount] [decimal](10, 2) NOT NULL,
	[Notes] [text] NULL,
PRIMARY KEY CLUSTERED 
(
	[InvoiceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbOrder]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbOrder](
	[OrderId] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[OrderTime] [datetime] NULL,
	[Notes] [text] NULL,
	[TotalAmount] [decimal](10, 2) NULL,
	[Discount] [decimal](10, 2) NULL,
	[FinalAmount] [decimal](10, 2) NULL,
	[Status] [varchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbOrderItem]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbOrderItem](
	[OrderItemId] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NOT NULL,
	[ItemType] [varchar](20) NOT NULL,
	[ItemId] [int] NOT NULL,
	[Quantity] [int] NULL,
	[UnitPrice] [decimal](10, 2) NOT NULL,
	[TotalPrice] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[OrderItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbPayment]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbPayment](
	[PaymentId] [int] IDENTITY(1,1) NOT NULL,
	[InvoiceId] [int] NOT NULL,
	[PaymentDate] [datetime] NULL,
	[PaymentMethod] [varchar](50) NOT NULL,
	[TransactionReference] [varchar](100) NULL,
	[Status] [varchar](20) NULL,
	[UserId] [int] NOT NULL,
	[Notes] [text] NULL,
PRIMARY KEY CLUSTERED 
(
	[PaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbService]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbService](
	[ServiceId] [int] IDENTITY(1,1) NOT NULL,
	[ServiceName] [varchar](100) NOT NULL,
	[Description] [text] NULL,
	[Price] [decimal](10, 2) NOT NULL,
	[CreatedDate] [datetime] NULL,
	[ModifiedDate] [datetime] NULL,
	[ImagePath] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[ServiceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tbUser]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbUser](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](50) NOT NULL,
	[Password] [varchar](255) NOT NULL,
	[CreatedDate] [datetime] NULL,
	[ModifiedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_Order_CustomerId]    Script Date: 3/31/2025 4:27:03 PM ******/
CREATE NONCLUSTERED INDEX [IX_Order_CustomerId] ON [dbo].[tbOrder]
(
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Order_UserId]    Script Date: 3/31/2025 4:27:03 PM ******/
CREATE NONCLUSTERED INDEX [IX_Order_UserId] ON [dbo].[tbOrder]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderItem_OrderId]    Script Date: 3/31/2025 4:27:03 PM ******/
CREATE NONCLUSTERED INDEX [IX_OrderItem_OrderId] ON [dbo].[tbOrderItem]
(
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payment_InvoiceId]    Script Date: 3/31/2025 4:27:03 PM ******/
CREATE NONCLUSTERED INDEX [IX_Payment_InvoiceId] ON [dbo].[tbPayment]
(
	[InvoiceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tbCard] ADD  DEFAULT ('Available') FOR [Status]
GO
ALTER TABLE [dbo].[tbCard] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[tbConsumable] ADD  DEFAULT ((0)) FOR [StockQuantity]
GO
ALTER TABLE [dbo].[tbConsumable] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[tbConsumable] ADD  DEFAULT (getdate()) FOR [ModifiedDate]
GO
ALTER TABLE [dbo].[tbCustomer] ADD  DEFAULT (getdate()) FOR [IssuedTime]
GO
ALTER TABLE [dbo].[tbInvoice] ADD  DEFAULT (getdate()) FOR [InvoiceDate]
GO
ALTER TABLE [dbo].[tbOrder] ADD  DEFAULT (getdate()) FOR [OrderTime]
GO
ALTER TABLE [dbo].[tbOrder] ADD  DEFAULT ((0)) FOR [TotalAmount]
GO
ALTER TABLE [dbo].[tbOrder] ADD  DEFAULT ((0)) FOR [Discount]
GO
ALTER TABLE [dbo].[tbOrder] ADD  DEFAULT ((0)) FOR [FinalAmount]
GO
ALTER TABLE [dbo].[tbOrder] ADD  DEFAULT ('Active') FOR [Status]
GO
ALTER TABLE [dbo].[tbOrderItem] ADD  DEFAULT ((1)) FOR [Quantity]
GO
ALTER TABLE [dbo].[tbPayment] ADD  DEFAULT (getdate()) FOR [PaymentDate]
GO
ALTER TABLE [dbo].[tbPayment] ADD  DEFAULT ('Completed') FOR [Status]
GO
ALTER TABLE [dbo].[tbService] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[tbService] ADD  DEFAULT (getdate()) FOR [ModifiedDate]
GO
ALTER TABLE [dbo].[tbUser] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[tbUser] ADD  DEFAULT (getdate()) FOR [ModifiedDate]
GO
ALTER TABLE [dbo].[tbCustomer]  WITH CHECK ADD FOREIGN KEY([CardId])
REFERENCES [dbo].[tbCard] ([CardId])
GO
ALTER TABLE [dbo].[tbInvoice]  WITH CHECK ADD FOREIGN KEY([OrderId])
REFERENCES [dbo].[tbOrder] ([OrderId])
GO
ALTER TABLE [dbo].[tbOrder]  WITH CHECK ADD FOREIGN KEY([CustomerId])
REFERENCES [dbo].[tbCustomer] ([CustomerId])
GO
ALTER TABLE [dbo].[tbOrder]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[tbUser] ([UserId])
GO
ALTER TABLE [dbo].[tbOrderItem]  WITH CHECK ADD FOREIGN KEY([OrderId])
REFERENCES [dbo].[tbOrder] ([OrderId])
GO
ALTER TABLE [dbo].[tbPayment]  WITH CHECK ADD FOREIGN KEY([InvoiceId])
REFERENCES [dbo].[tbInvoice] ([InvoiceId])
GO
ALTER TABLE [dbo].[tbPayment]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[tbUser] ([UserId])
GO
/****** Object:  StoredProcedure [dbo].[sp_AddOrderItem]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_AddOrderItem]
    @OrderId INT,
    @ItemType VARCHAR(20),
    @ItemId INT,
    @Quantity INT = 1
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @UnitPrice DECIMAL(10,2);
    DECLARE @TotalPrice DECIMAL(10,2);
    
    -- Get the price based on item type
    IF @ItemType = 'Service'
    BEGIN
        SELECT @UnitPrice = Price FROM tbService WHERE ServiceId = @ItemId;
    END
    ELSE IF @ItemType = 'Consumable'
    BEGIN
        SELECT @UnitPrice = Price FROM tbConsumable WHERE ConsumableId = @ItemId;
        
        -- Update stock quantity
        UPDATE tbConsumable SET 
            StockQuantity = StockQuantity - @Quantity,
            ModifiedDate = GETDATE()
        WHERE ConsumableId = @ItemId;
    END
    
    SET @TotalPrice = @UnitPrice * @Quantity;
    
    -- Add item to order
    INSERT INTO tbOrderItem (OrderId, ItemType, ItemId, Quantity, UnitPrice, TotalPrice)
    VALUES (@OrderId, @ItemType, @ItemId, @Quantity, @UnitPrice, @TotalPrice);
    
    -- Update order totals
    UPDATE tbOrder
    SET TotalAmount = (SELECT SUM(TotalPrice) FROM tbOrderItem WHERE OrderId = @OrderId),
        FinalAmount = (SELECT SUM(TotalPrice) FROM tbOrderItem WHERE OrderId = @OrderId) - Discount
    WHERE OrderId = @OrderId;
    
    SELECT SCOPE_IDENTITY() AS OrderItemId;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ApplyOrderDiscount]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_ApplyOrderDiscount]
    @OrderId INT,
    @DiscountAmount DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE tbOrder
    SET Discount = @DiscountAmount,
        FinalAmount = TotalAmount - @DiscountAmount
    WHERE OrderId = @OrderId;
    
    SELECT OrderId, TotalAmount, Discount, FinalAmount
    FROM tbOrder
    WHERE OrderId = @OrderId;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_CheckCardStatus]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_CheckCardStatus]
    @CardId VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.CardId,
        c.Status,
        c.LastUsed,
        cust.CustomerId,
        cust.IssuedTime,
        o.OrderId,
        o.TotalAmount,
        o.FinalAmount
    FROM tbCard c
    LEFT JOIN tbCustomer cust ON c.CardId = cust.CardId AND cust.ReleasedTime IS NULL
    LEFT JOIN tbOrder o ON cust.CustomerId = o.CustomerId AND o.Status = 'Active'
    WHERE c.CardId = @CardId;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_CompleteOrder]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_CompleteOrder]
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    -- Update order status
    UPDATE tbOrder
    SET Status = 'Completed'
    WHERE OrderId = @OrderId;
    
    -- Create invoice
    INSERT INTO tbInvoice (OrderId, TotalAmount)
    SELECT OrderId, FinalAmount
    FROM tbOrder
    WHERE OrderId = @OrderId;
    
    DECLARE @InvoiceId INT = SCOPE_IDENTITY();
    
    COMMIT TRANSACTION;
    
    -- Return the invoice details
    SELECT 
        i.InvoiceId,
        i.OrderId,
        i.InvoiceDate,
        i.TotalAmount,
        c.CardId AS CustomerCardId
    FROM tbInvoice i
    JOIN tbOrder o ON i.OrderId = o.OrderId
    JOIN tbCustomer c ON o.CustomerId = c.CustomerId
    WHERE i.InvoiceId = @InvoiceId;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_CreateOrder]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_CreateOrder]
    @CustomerId INT,
    @UserId INT,
    @Notes TEXT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO tbOrder (CustomerId, UserId, Notes)
    VALUES (@CustomerId, @UserId, @Notes);
    
    SELECT SCOPE_IDENTITY() AS OrderId;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_CreateService]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Create a new service
CREATE PROCEDURE [dbo].[sp_CreateService]
    @ServiceName VARCHAR(100),
    @Description TEXT = NULL,
    @Price DECIMAL(10,2),
    @ImagePath VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO tbService (ServiceName, Description, Price, ImagePath)
    VALUES (@ServiceName, @Description, @Price, @ImagePath);
    
    SELECT SCOPE_IDENTITY() AS ServiceId;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_CreateUser]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_CreateUser]
    @Username VARCHAR(50),
    @Password VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO tbUser (Username, Password)
    VALUES (@Username, @Password);
    
    SELECT SCOPE_IDENTITY() AS UserId;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GenerateDailySalesReport]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GenerateDailySalesReport]
    @Date DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Daily order summary
    SELECT 
        COUNT(OrderId) AS TotalOrders,
        SUM(TotalAmount) AS GrossSales,
        SUM(Discount) AS TotalDiscounts,
        SUM(FinalAmount) AS NetSales
    FROM tbOrder
    WHERE CONVERT(DATE, OrderTime) = @Date
    AND Status = 'Completed';
    
    -- Payment method breakdown
    SELECT 
        PaymentMethod,
        COUNT(PaymentId) AS PaymentCount,
        SUM(i.TotalAmount) AS TotalAmount
    FROM tbPayment p
    JOIN tbInvoice i ON p.InvoiceId = i.InvoiceId
    WHERE CONVERT(DATE, p.PaymentDate) = @Date
    GROUP BY PaymentMethod
    ORDER BY TotalAmount DESC;
    
    -- Top selling services
    SELECT 
        s.ServiceId,
        s.ServiceName,
        COUNT(oi.OrderItemId) AS TimesSold,
        SUM(oi.Quantity) AS TotalQuantity,
        SUM(oi.TotalPrice) AS TotalSales
    FROM tbOrderItem oi
    JOIN tbService s ON oi.ItemId = s.ServiceId
    JOIN tbOrder o ON oi.OrderId = o.OrderId
    WHERE oi.ItemType = 'Service'
    AND CONVERT(DATE, o.OrderTime) = @Date
    AND o.Status = 'Completed'
    GROUP BY s.ServiceId, s.ServiceName
    ORDER BY TotalSales DESC;
    
    -- Top selling consumables
    SELECT 
        c.ConsumableId,
        c.Name,
        COUNT(oi.OrderItemId) AS TimesSold,
        SUM(oi.Quantity) AS TotalQuantity,
        SUM(oi.TotalPrice) AS TotalSales
    FROM tbOrderItem oi
    JOIN tbConsumable c ON oi.ItemId = c.ConsumableId
    JOIN tbOrder o ON oi.OrderId = o.OrderId
    WHERE oi.ItemType = 'Consumable'
    AND CONVERT(DATE, o.OrderTime) = @Date
    AND o.Status = 'Completed'
    GROUP BY c.ConsumableId, c.Name
    ORDER BY TotalSales DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetActiveOrders]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GetActiveOrders]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        o.OrderId,
        o.CustomerId,
        c.CardId AS CustomerCardId,
        o.UserId,
        u.Username AS UserName,
        o.OrderTime,
        o.TotalAmount,
        o.Discount,
        o.FinalAmount,
        o.Status
    FROM tbOrder o
    JOIN tbCustomer c ON o.CustomerId = c.CustomerId
    JOIN tbUser u ON o.UserId = u.UserId
    WHERE o.Status = 'Active'
    ORDER BY o.OrderTime DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetAvailableCards]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GetAvailableCards]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CardId,
        LastUsed,
        CreatedDate
    FROM tbCard
    WHERE Status = 'Available'
    ORDER BY LastUsed DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetAvailableConsumables]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GetAvailableConsumables]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ConsumableId,
        Name,
        Description,
        Price,
        Category,
        StockQuantity,
        CreatedDate,
        ModifiedDate
    FROM tbConsumable
    WHERE StockQuantity > 0
    ORDER BY Category, Name;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetAvailableServices]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- 10. Get Available Services
CREATE   PROCEDURE [dbo].[sp_GetAvailableServices]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ServiceId,
        ServiceName,
        Description,
        Price,
        ImagePath, -- Added ImagePath column
        CreatedDate,
        ModifiedDate
    FROM tbService
    ORDER BY ServiceName;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetOrderDetails]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GetOrderDetails]
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get order header
    SELECT 
        o.OrderId,
        o.CustomerId,
        c.CardId AS CustomerCardId,
        o.UserId,
        u.Username AS UserName,
        o.OrderTime,
        o.TotalAmount,
        o.Discount,
        o.FinalAmount,
        o.Status,
        o.Notes
    FROM tbOrder o
    JOIN tbCustomer c ON o.CustomerId = c.CustomerId
    JOIN tbUser u ON o.UserId = u.UserId
    WHERE o.OrderId = @OrderId;
    
    -- Get order items
    SELECT 
        oi.OrderItemId,
        oi.OrderId,
        oi.ItemType,
        oi.ItemId,
        CASE 
            WHEN oi.ItemType = 'Service' THEN s.ServiceName
            WHEN oi.ItemType = 'Consumable' THEN c.Name
            ELSE 'Unknown'
        END AS ItemName,
        oi.Quantity,
        oi.UnitPrice,
        oi.TotalPrice
    FROM tbOrderItem oi
    LEFT JOIN tbService s ON oi.ItemType = 'Service' AND oi.ItemId = s.ServiceId
    LEFT JOIN tbConsumable c ON oi.ItemType = 'Consumable' AND oi.ItemId = c.ConsumableId
    WHERE oi.OrderId = @OrderId
    ORDER BY oi.OrderItemId;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetServiceById]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Get service by ID
CREATE PROCEDURE [dbo].[sp_GetServiceById]
    @ServiceId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ServiceId,
        ServiceName, 
        Description,
        Price,
        ImagePath,
        CreatedDate,
        ModifiedDate
    FROM tbService
    WHERE ServiceId = @ServiceId;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_IssueCardToCustomer]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_IssueCardToCustomer]
    @CardId VARCHAR(255),
    @Notes TEXT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    -- Check if card exists and is available
    IF NOT EXISTS (SELECT 1 FROM tbCard WHERE CardId = @CardId AND Status = 'Available')
    BEGIN
        ROLLBACK;
        RAISERROR('Card not available or not registered.', 16, 1);
        RETURN;
    END
    
    -- Update card status
    UPDATE tbCard
    SET Status = 'InUse',
        LastUsed = GETDATE()
    WHERE CardId = @CardId;
    
    -- Create customer record
    INSERT INTO tbCustomer (CardId, IssuedTime, Notes)
    VALUES (@CardId, GETDATE(), @Notes);
    
    DECLARE @CustomerId INT = SCOPE_IDENTITY();
    
    COMMIT TRANSACTION;
    
    SELECT @CustomerId AS CustomerId, @CardId AS CardId;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ProcessPayment]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_ProcessPayment]
    @InvoiceId INT,
    @PaymentMethod VARCHAR(50),
    @TransactionReference VARCHAR(100) = NULL,
    @UserId INT,
    @Notes TEXT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    DECLARE @TotalAmount DECIMAL(10,2);
    DECLARE @OrderId INT;
    DECLARE @CustomerId INT;
    DECLARE @CardId VARCHAR(255);
    
    -- Get total amount and order ID from invoice
    SELECT @TotalAmount = TotalAmount, @OrderId = OrderId
    FROM tbInvoice
    WHERE InvoiceId = @InvoiceId;
    
    -- Get customer ID and card ID from order
    SELECT @CustomerId = o.CustomerId, @CardId = c.CardId
    FROM tbOrder o
    JOIN tbCustomer c ON o.CustomerId = c.CustomerId
    WHERE o.OrderId = @OrderId;
    
    -- Create payment record
    INSERT INTO tbPayment (InvoiceId, PaymentMethod, TransactionReference, UserId, Notes)
    VALUES (@InvoiceId, @PaymentMethod, @TransactionReference, @UserId, @Notes);
    
    DECLARE @PaymentId INT = SCOPE_IDENTITY();
    
    -- Update customer record
    UPDATE tbCustomer
    SET ReleasedTime = GETDATE()
    WHERE CustomerId = @CustomerId;
    
    -- Release the card
    UPDATE tbCard
    SET Status = 'Available'
    WHERE CardId = @CardId;
    
    COMMIT TRANSACTION;
    
    SELECT @PaymentId AS PaymentId;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_RegisterCard]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_RegisterCard]
    @CardId VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if card already exists
    IF EXISTS (SELECT 1 FROM tbCard WHERE CardId = @CardId)
    BEGIN
        RAISERROR('This card is already registered in the system.', 16, 1);
        RETURN;
    END
    
    -- Register new card
    INSERT INTO tbCard (CardId, Status, CreatedDate)
    VALUES (@CardId, 'Available', GETDATE());
    
    SELECT @CardId AS CardId, 'Available' AS Status;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_RegisterCardBatch]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_RegisterCardBatch]
    @Prefix VARCHAR(10),
    @StartNumber INT,
    @Count INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @i INT = 0;
    DECLARE @CardId VARCHAR(255);
    
    WHILE @i < @Count
    BEGIN
        SET @CardId = @Prefix + RIGHT('00000' + CAST(@StartNumber + @i AS VARCHAR(10)), 5);
        
        -- Only insert if it doesn't already exist
        IF NOT EXISTS (SELECT 1 FROM tbCard WHERE CardId = @CardId)
        BEGIN
            INSERT INTO tbCard (CardId, Status, CreatedDate)
            VALUES (@CardId, 'Available', GETDATE());
        END
        
        SET @i = @i + 1;
    END
    
    SELECT 'Registered ' + CAST(@Count AS VARCHAR(10)) + ' cards with prefix ' + @Prefix AS Result;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_SetCardAsDamaged]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_SetCardAsDamaged]
    @CardId VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if card is in use
    IF EXISTS (SELECT 1 FROM tbCard WHERE CardId = @CardId AND Status = 'InUse')
    BEGIN
        RAISERROR('Cannot mark card as damaged while it is in use.', 16, 1);
        RETURN;
    END
    
    UPDATE tbCard
    SET Status = 'Damaged'
    WHERE CardId = @CardId;
    
    SELECT CardId, Status FROM tbCard WHERE CardId = @CardId;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_UpdateOrderItemQuantity]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_UpdateOrderItemQuantity]
    @OrderItemId INT,
    @Quantity INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @OrderId INT;
    DECLARE @UnitPrice DECIMAL(10,2);
    
    -- Get order ID and unit price
    SELECT @OrderId = OrderId, @UnitPrice = UnitPrice 
    FROM tbOrderItem 
    WHERE OrderItemId = @OrderItemId;
    
    -- Update the order item
    UPDATE tbOrderItem
    SET Quantity = @Quantity,
        TotalPrice = @UnitPrice * @Quantity
    WHERE OrderItemId = @OrderItemId;
    
    -- Update the order totals
    UPDATE tbOrder
    SET TotalAmount = (SELECT SUM(TotalPrice) FROM tbOrderItem WHERE OrderId = @OrderId),
        FinalAmount = (SELECT SUM(TotalPrice) FROM tbOrderItem WHERE OrderId = @OrderId) - Discount
    WHERE OrderId = @OrderId;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_UpdateService]    Script Date: 3/31/2025 4:27:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Update an existing service
CREATE PROCEDURE [dbo].[sp_UpdateService]
    @ServiceId INT,
    @ServiceName VARCHAR(100),
    @Description TEXT = NULL,
    @Price DECIMAL(10,2),
    @ImagePath VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE tbService
    SET ServiceName = @ServiceName,
        Description = @Description,
        Price = @Price,
        ImagePath = @ImagePath,
        ModifiedDate = GETDATE()
    WHERE ServiceId = @ServiceId;
    
    SELECT @ServiceId AS ServiceId;
END
GO
USE [master]
GO
ALTER DATABASE [SpaManagement] SET  READ_WRITE 
GO
