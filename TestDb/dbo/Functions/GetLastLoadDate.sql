CREATE FUNCTION [dbo].[GetLastLoadDate]()
RETURNS DATETIME
AS
BEGIN
	DECLARE @lastLoadDate DATETIME;
	SELECT @lastLoadDate = MAX(LoadDate) FROM dbo.LogTable;
	RETURN @lastLoadDate;
END
