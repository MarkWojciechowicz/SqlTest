CREATE TABLE [dbo].[Customer]
(
	[Id] INT NOT NULL IDENTITY , 
	[CustomerCode] NCHAR(10) NOT NULL,
    [Name] NVARCHAR(50) NULL, 
    CONSTRAINT [PK_Customer] PRIMARY KEY ([Id]), 
    CONSTRAINT [AK_Customer] UNIQUE ([CustomerCode])
)
