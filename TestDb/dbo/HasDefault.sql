CREATE TABLE [dbo].[HasDefault]
(
	[Id] INT NOT NULL PRIMARY KEY
	, Description varchar(50) Constraint DF_HasDefault_Description Default 'Test' NOT NULL
)
