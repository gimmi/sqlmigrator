CREATE TABLE Details(
	Id int NOT NULL PRIMARY KEY,
	MasterId int NOT NULL REFERENCES Masters(Id),
	Description nvarchar(50) NULL
)

-- @DOWN

DROP TABLE Details
