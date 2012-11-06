
	DECLARE @chk_name nvarchar(64)
	SELECT @chk_name = CONSTRAINT_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE WHERE TABLE_NAME = 'alerts' AND COLUMN_NAME = 'linked_type'

	DECLARE @sql nvarchar(512)
	SET @sql = 'ALTER TABLE alerts DROP CONSTRAINT ' + @chk_name
	EXEC sp_executesql @sql;

ALTER TABLE alerts ADD CONSTRAINT alerts_linked_type_chk CHECK (linked_type in ('', 'TASK', 'WEEKLY_TASK', 'BARRIER'));
UPDATE alerts SET linked_type = 'WEEKLY_TASK' WHERE linked_type = 'TASK';
