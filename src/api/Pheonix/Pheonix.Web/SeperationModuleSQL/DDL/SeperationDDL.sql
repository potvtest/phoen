IF OBJECT_ID('Seperation') IS NOT NULL
	DROP TABLE Seperation
GO

CREATE TABLE Seperation (
	ID INT Identity(1, 1)
	,ResignDate DATETIME NOT NULL
	,ExpectedDate DATETIME NOT NULL
	,ActualDate DATETIME NOT NULL
	,SeperationReason VARCHAR(200) NOT NULL
	,Comments VARCHAR(200) NULL
	,ApprovalID INT NOT NULL
	,ApprovalDate DATETIME NOT NULL
	,StatusID INT NOT NULL
	,PersonID INT NOT NULL
	,CreatedBy INT NOT NULL
	,CreatedOn DATETIME NOT NULL
	,UpdatedBy INT NOT NULL
	,UpdatedOn INT NOT NULL
	,CONSTRAINT pk_seperationid PRIMARY KEY (ID)
	,CONSTRAINT fk_person_seperation_approvalid FOREIGN KEY (ApprovalID) REFERENCES Person(ID)
	,CONSTRAINT fk_person_seperation_personid FOREIGN KEY (PersonID) REFERENCES Person(ID)
	,
	)

	IF OBJECT_ID('SeperationProcess') IS NOT NULL
	DROP TABLE SeperationProcess
GO
CREATE TABLE [dbo].[SeperationProcess](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RoleID] [int] NOT NULL,
	[SeperationID] [int] NOT NULL,
	[ChecklistAuthorizePersonId] [int] NOT NULL,
	[StatusID] [int] NOT NULL,
	[Comments] [nvarchar](1000) NOT NULL,
	[ChecklistProcessedData] [nvarchar](max) NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedBy] [int] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
 CONSTRAINT [pk_seperationprocessid] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER TABLE [dbo].[SeperationProcess]  WITH CHECK ADD  CONSTRAINT [fk_seperationprocess_checklistvalidatorid] FOREIGN KEY([ChecklistAuthorizePersonId])
REFERENCES [dbo].[Person] ([ID])
GO
ALTER TABLE [dbo].[SeperationProcess] CHECK CONSTRAINT [fk_seperationprocess_checklistvalidatorid]
GO
ALTER TABLE [dbo].[SeperationProcess]  WITH CHECK ADD  CONSTRAINT [fk_seperationprocess_role] FOREIGN KEY([RoleID])
REFERENCES [dbo].[Role] ([ID])
GO
ALTER TABLE [dbo].[SeperationProcess] CHECK CONSTRAINT [fk_seperationprocess_role]
GO
ALTER TABLE [dbo].[SeperationProcess]  WITH CHECK ADD  CONSTRAINT [fk_seperationprocess_seperation] FOREIGN KEY([SeperationID])
REFERENCES [dbo].[Seperation] ([ID])
GO
ALTER TABLE [dbo].[SeperationProcess] CHECK CONSTRAINT [fk_seperationprocess_seperation]
GO