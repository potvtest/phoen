Use ValuePortal

SET IDENTITY_INSERT VPPriorityMaster ON
Insert into VPPriorityMaster(ID,Name,Description,IsActive) VALUES(1,'Lowest','Lowest',1)
Insert into VPPriorityMaster(ID,Name,Description,IsActive) VALUES(2,'Vert Low','Vert Low',1)
Insert into VPPriorityMaster(ID,Name,Description,IsActive) VALUES(3,'Low','Low',1)
Insert into VPPriorityMaster(ID,Name,Description,IsActive) VALUES(4,'Medium','Medium',1)
Insert into VPPriorityMaster(ID,Name,Description,IsActive) VALUES(5,'Medium High','Medium High',1)
Insert into VPPriorityMaster(ID,Name,Description,IsActive) VALUES(6,'Highest','Highest',1)
SET IDENTITY_INSERT VPPriorityMaster OFF

GO

SET IDENTITY_INSERT VPStatusMaster ON
Insert into VPStatusMaster(ID,Name,Description,IsActive) VALUES(1,'Open','Open',1)
Insert into VPStatusMaster(ID,Name,Description,IsActive) VALUES(2,'Hold','Hold',1)
Insert into VPStatusMaster(ID,Name,Description,IsActive) VALUES(3,'Rejected','Rejected',1)
Insert into VPStatusMaster(ID,Name,Description,IsActive) VALUES(4,'Approved','Approved',1)
SET IDENTITY_INSERT VPStatusMaster OFF

GO

