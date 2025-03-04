USE [master]
GO
/****** Object:  Database [ValuePortal]    Script Date: 7/21/2021 11:24:21 AM ******/
CREATE DATABASE [ValuePortal]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'ValuePortal', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\ValuePortal.mdf' , SIZE = 4096KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'ValuePortal_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\ValuePortal_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [ValuePortal] SET COMPATIBILITY_LEVEL = 110
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [ValuePortal].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [ValuePortal] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [ValuePortal] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [ValuePortal] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [ValuePortal] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [ValuePortal] SET ARITHABORT OFF 
GO
ALTER DATABASE [ValuePortal] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [ValuePortal] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [ValuePortal] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [ValuePortal] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [ValuePortal] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [ValuePortal] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [ValuePortal] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [ValuePortal] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [ValuePortal] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [ValuePortal] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [ValuePortal] SET  DISABLE_BROKER 
GO
ALTER DATABASE [ValuePortal] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [ValuePortal] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [ValuePortal] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [ValuePortal] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [ValuePortal] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [ValuePortal] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [ValuePortal] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [ValuePortal] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [ValuePortal] SET  MULTI_USER 
GO
ALTER DATABASE [ValuePortal] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [ValuePortal] SET DB_CHAINING OFF 
GO
ALTER DATABASE [ValuePortal] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [ValuePortal] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
USE [ValuePortal]
GO
/****** Object:  Table [dbo].[VPIdeaDetails]    Script Date: 7/21/2021 11:24:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VPIdeaDetails](
	[ID] [bigint] NOT NULL,
	[VCIdeaMasterID] [bigint] NOT NULL,
	[Comments] [nvarchar](max) NULL,
	[PriorityID] [smallint] NULL,
	[StatusId] [smallint] NULL,
	[BenefitScore] [float] NULL,
	[Cost] [float] NULL,
	[FinalScore] [float] NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LastUpdatedDate] [datetime] NOT NULL,
	[UpdatedBy] [int] NOT NULL,
 CONSTRAINT [PK_VCIdeaDetails] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[VPIdeaMaster]    Script Date: 7/21/2021 11:24:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VPIdeaMaster](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[IdeaHeadline] [nvarchar](250) NOT NULL,
	[IdeaDescription] [nvarchar](max) NOT NULL,
	[IdeaBenefits] [nvarchar](max) NOT NULL,
	[RequiredEffort] [nvarchar](1000) NOT NULL,
	[RequiredResources] [nvarchar](1000) NOT NULL,
	[RequiredTechnologies] [nvarchar](1000) NOT NULL,
	[SubmittedBy] [int] NOT NULL,
	[UpdatedBy] [int] NULL,
	[ExecutionApproach] [nvarchar](max) NULL,
	[IsEmailReceiptRequired] [bit] NOT NULL,
	[StatusID] [smallint] NOT NULL,
	[PriorityID] [smallint] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LastUpdatedDate] [datetime] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_VCIdeaMaster] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[VPPriorityMaster]    Script Date: 7/21/2021 11:24:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VPPriorityMaster](
	[ID] [smallint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[Description] [nvarchar](2000) NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_VCPriorityMaster] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[VPStatusMaster]    Script Date: 7/21/2021 11:24:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VPStatusMaster](
	[ID] [smallint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[Description] [nvarchar](2000) NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_VCStatusMaster] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET IDENTITY_INSERT [dbo].[VPPriorityMaster] ON 

INSERT [dbo].[VPPriorityMaster] ([ID], [Name], [Description], [IsActive]) VALUES (1, N'Lowest', N'Lowest', 1)
INSERT [dbo].[VPPriorityMaster] ([ID], [Name], [Description], [IsActive]) VALUES (2, N'Vert Low', N'Vert Low', 1)
INSERT [dbo].[VPPriorityMaster] ([ID], [Name], [Description], [IsActive]) VALUES (3, N'Low', N'Low', 1)
INSERT [dbo].[VPPriorityMaster] ([ID], [Name], [Description], [IsActive]) VALUES (4, N'Medium', N'Medium', 1)
INSERT [dbo].[VPPriorityMaster] ([ID], [Name], [Description], [IsActive]) VALUES (5, N'Medium High', N'Medium High', 1)
INSERT [dbo].[VPPriorityMaster] ([ID], [Name], [Description], [IsActive]) VALUES (6, N'Highest', N'Highest', 1)
SET IDENTITY_INSERT [dbo].[VPPriorityMaster] OFF
SET IDENTITY_INSERT [dbo].[VPStatusMaster] ON 

INSERT [dbo].[VPStatusMaster] ([ID], [Name], [Description], [IsActive]) VALUES (1, N'Open', N'Open', 1)
INSERT [dbo].[VPStatusMaster] ([ID], [Name], [Description], [IsActive]) VALUES (2, N'Hold', N'Hold', 1)
INSERT [dbo].[VPStatusMaster] ([ID], [Name], [Description], [IsActive]) VALUES (3, N'Rejected', N'Rejected', 1)
INSERT [dbo].[VPStatusMaster] ([ID], [Name], [Description], [IsActive]) VALUES (4, N'Approved', N'Approved', 1)
SET IDENTITY_INSERT [dbo].[VPStatusMaster] OFF
ALTER TABLE [dbo].[VPIdeaDetails] ADD  CONSTRAINT [DF_VCIdeaDetails_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[VPIdeaDetails] ADD  CONSTRAINT [DF_VCIdeaDetails_LastUpdatedDate]  DEFAULT (getdate()) FOR [LastUpdatedDate]
GO
ALTER TABLE [dbo].[VPIdeaMaster] ADD  CONSTRAINT [DF_VCIdeaMaster_IsEmailReceiptRequired]  DEFAULT ((0)) FOR [IsEmailReceiptRequired]
GO
ALTER TABLE [dbo].[VPIdeaMaster] ADD  CONSTRAINT [DF_VCIdeaMaster_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[VPIdeaMaster] ADD  CONSTRAINT [DF_VCIdeaMaster_LastUpdatedDate]  DEFAULT (getdate()) FOR [LastUpdatedDate]
GO
ALTER TABLE [dbo].[VPIdeaMaster] ADD  CONSTRAINT [DF_VPIdeaMaster_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[VPPriorityMaster] ADD  CONSTRAINT [DF_VCPriorityMaster_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[VPStatusMaster] ADD  CONSTRAINT [DF_VCStatusMaster_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[VPIdeaDetails]  WITH CHECK ADD  CONSTRAINT [FK_VCIdeaDetails_VCIdeaMaster] FOREIGN KEY([VCIdeaMasterID])
REFERENCES [dbo].[VPIdeaMaster] ([ID])
GO
ALTER TABLE [dbo].[VPIdeaDetails] CHECK CONSTRAINT [FK_VCIdeaDetails_VCIdeaMaster]
GO
ALTER TABLE [dbo].[VPIdeaDetails]  WITH CHECK ADD  CONSTRAINT [FK_VCIdeaDetails_VCPriorityMaster] FOREIGN KEY([PriorityID])
REFERENCES [dbo].[VPPriorityMaster] ([ID])
GO
ALTER TABLE [dbo].[VPIdeaDetails] CHECK CONSTRAINT [FK_VCIdeaDetails_VCPriorityMaster]
GO
ALTER TABLE [dbo].[VPIdeaDetails]  WITH CHECK ADD  CONSTRAINT [FK_VCIdeaDetails_VCStatusMaster] FOREIGN KEY([StatusId])
REFERENCES [dbo].[VPStatusMaster] ([ID])
GO
ALTER TABLE [dbo].[VPIdeaDetails] CHECK CONSTRAINT [FK_VCIdeaDetails_VCStatusMaster]
GO
ALTER TABLE [dbo].[VPIdeaMaster]  WITH CHECK ADD  CONSTRAINT [FK_VCIdeaMaster_VCPriorityMaster] FOREIGN KEY([PriorityID])
REFERENCES [dbo].[VPPriorityMaster] ([ID])
GO
ALTER TABLE [dbo].[VPIdeaMaster] CHECK CONSTRAINT [FK_VCIdeaMaster_VCPriorityMaster]
GO
ALTER TABLE [dbo].[VPIdeaMaster]  WITH CHECK ADD  CONSTRAINT [FK_VCIdeaMaster_VCStatusMaster] FOREIGN KEY([StatusID])
REFERENCES [dbo].[VPStatusMaster] ([ID])
GO
ALTER TABLE [dbo].[VPIdeaMaster] CHECK CONSTRAINT [FK_VCIdeaMaster_VCStatusMaster]
GO
USE [master]
GO
ALTER DATABASE [ValuePortal] SET  READ_WRITE 
GO
