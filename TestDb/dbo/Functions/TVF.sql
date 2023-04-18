CREATE FUNCTION [dbo].[TVF]
(
	@param1 int
)
RETURNS @returntable TABLE
(
	c1 int
)
AS
BEGIN
	INSERT @returntable
	SELECT @param1
	RETURN
END
