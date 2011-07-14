CREATE TABLE Details(
	Id int NOT NULL PRIMARY KEY,
	MasterId int NOT NULL, -- REFERENCES Masters(Id),
	Description nvarchar(50) NULL
)
GO
ALTER TABLE Details ADD CONSTRAINT FK_Detail_Master FOREIGN KEY(MasterId)
REFERENCES Masters(Id)
GO

-- @DOWN

DROP TABLE Details
