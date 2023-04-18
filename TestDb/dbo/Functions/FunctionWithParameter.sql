CREATE FUNCTION [dbo].[FunctionWithParameter]
(
	@param1 int
)
RETURNS INT
AS
BEGIN
	SET @param1 = 0;
	RETURN @param1
END
