CREATE PROCEDURE Stage.Load_Customer
AS
    BEGIN
        SET NOCOUNT ON;

        UPDATE
             target
        SET
             target.Name = source.Name
        FROM dbo.Customer        target
             JOIN Stage.Customer source
               ON target.CustomerCode = source.CustomerCode;

        INSERT INTO dbo.Customer
            (
                Name
            )
        SELECT
              Name
        FROM  Stage.Customer source
        WHERE NOT EXISTS (
                             SELECT
                                   1
                             FROM  dbo.Customer target
                             WHERE source.CustomerCode = target.CustomerCode
                         );
    END;
