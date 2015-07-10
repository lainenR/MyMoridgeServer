
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 12/31/2014 11:29:44
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
    [BookingMessage] nvarchar(max)  NULL
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

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------