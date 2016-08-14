
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 08/07/2016 11:11:02
-- Generated from EDMX file: C:\Users\krevay\documents\visual studio 2012\Projects\MyMoridgeServer\MyMoridgeServer\Models\MyMoridgeServerModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [MyMoridgeServer];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_BookingLogResource]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BookingLogs] DROP CONSTRAINT [FK_BookingLogResource];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[CustomerVehicles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CustomerVehicles];
GO
IF OBJECT_ID(N'[dbo].[ErrorLogSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ErrorLogSet];
GO
IF OBJECT_ID(N'[dbo].[BookingLogs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BookingLogs];
GO
IF OBJECT_ID(N'[dbo].[Resources]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Resources];
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO
IF OBJECT_ID(N'[dbo].[EmailLogs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[EmailLogs];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'CustomerVehicles'
CREATE TABLE [dbo].[CustomerVehicles] (
    [CustomerOrgNo] nvarchar(30)  NOT NULL,
    [VehicleRegNo] nvarchar(10)  NOT NULL
);
GO

-- Creating table 'ErrorLogSet'
CREATE TABLE [dbo].[ErrorLogSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ErrorMessage] nvarchar(max)  NOT NULL,
    [DatetTimeStamp] datetime  NOT NULL
);
GO

-- Creating table 'BookingLogs'
CREATE TABLE [dbo].[BookingLogs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [StartDateTime] datetime  NOT NULL,
    [EndDateTime] datetime  NOT NULL,
    [CustomerOrgNo] nvarchar(100)  NOT NULL,
    [CustomerEmail] nvarchar(100)  NOT NULL,
    [CustomerAddress] nvarchar(max)  NOT NULL,
    [VehicleRegNo] nvarchar(10)  NOT NULL,
    [CompanyName] nvarchar(max)  NULL,
    [BookingMessage] nvarchar(max)  NULL,
    [ResourceId] int  NOT NULL,
    [SupplierEmailAddress] nvarchar(100)  NOT NULL,
    [BookingHeader] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Resources'
CREATE TABLE [dbo].[Resources] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(100)  NOT NULL,
    [CalendarEmail] nvarchar(100)  NOT NULL,
    [CalendarServiceAccountEmail] nvarchar(200)  NOT NULL,
    [MaxBookingsBeforeLunch] int  NOT NULL,
    [MaxBookingsAfterLunch] int  NOT NULL,
    [DaysBeforeBooking] int  NOT NULL,
    [BookingPriority] int  NOT NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserName] nvarchar(100)  NOT NULL,
    [Password] nvarchar(100)  NOT NULL,
    [LastLogin] datetime  NOT NULL
);
GO

-- Creating table 'EmailLogs'
CREATE TABLE [dbo].[EmailLogs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [CustomerEmail] nvarchar(100)  NOT NULL,
    [CompanyName] nvarchar(max)  NOT NULL,
    [VehicleRegNo] nvarchar(10)  NOT NULL,
    [Sent] datetime  NOT NULL
);
GO

-- Creating table 'InvitationVouchers'
CREATE TABLE [dbo].[InvitationVouchers] (
    [VoucherId] uniqueidentifier  NOT NULL,
    [BookingLogId] int  NOT NULL,
    [StartDateTime] datetime  NOT NULL,
    [EndDateTime] datetime  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [CustomerOrgNo], [VehicleRegNo] in table 'CustomerVehicles'
ALTER TABLE [dbo].[CustomerVehicles]
ADD CONSTRAINT [PK_CustomerVehicles]
    PRIMARY KEY CLUSTERED ([CustomerOrgNo], [VehicleRegNo] ASC);
GO

-- Creating primary key on [Id] in table 'ErrorLogSet'
ALTER TABLE [dbo].[ErrorLogSet]
ADD CONSTRAINT [PK_ErrorLogSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'BookingLogs'
ALTER TABLE [dbo].[BookingLogs]
ADD CONSTRAINT [PK_BookingLogs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Resources'
ALTER TABLE [dbo].[Resources]
ADD CONSTRAINT [PK_Resources]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'EmailLogs'
ALTER TABLE [dbo].[EmailLogs]
ADD CONSTRAINT [PK_EmailLogs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [VoucherId] in table 'InvitationVouchers'
ALTER TABLE [dbo].[InvitationVouchers]
ADD CONSTRAINT [PK_InvitationVouchers]
    PRIMARY KEY CLUSTERED ([VoucherId] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [ResourceId] in table 'BookingLogs'
ALTER TABLE [dbo].[BookingLogs]
ADD CONSTRAINT [FK_BookingLogResource]
    FOREIGN KEY ([ResourceId])
    REFERENCES [dbo].[Resources]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_BookingLogResource'
CREATE INDEX [IX_FK_BookingLogResource]
ON [dbo].[BookingLogs]
    ([ResourceId]);
GO

-- Creating foreign key on [BookingLogId] in table 'InvitationVouchers'
ALTER TABLE [dbo].[InvitationVouchers]
ADD CONSTRAINT [FK_BookingLogInvitationVoucher]
    FOREIGN KEY ([BookingLogId])
    REFERENCES [dbo].[BookingLogs]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_BookingLogInvitationVoucher'
CREATE INDEX [IX_FK_BookingLogInvitationVoucher]
ON [dbo].[InvitationVouchers]
    ([BookingLogId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------