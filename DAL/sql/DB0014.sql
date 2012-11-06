alter table weekly_plan add plan_approved_by bigint foreign key references profiles (id);
alter table weekly_plan add log_approved_by bigint foreign key references profiles (id);

update weekly_plan set plan_approved_by = approved_by where state = 'PLAN_APPROVED' or state = 'LOG_READY';
update weekly_plan set log_approved_by = approved_by, plan_approved_by = approved_by where state = 'LOG_APPROVED';

	DECLARE @chk_name nvarchar(64)
	SELECT @chk_name = CONSTRAINT_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE WHERE TABLE_NAME = 'weekly_plan' AND COLUMN_NAME = 'approved_by'

	DECLARE @sql nvarchar(512)
	SET @sql = 'ALTER TABLE weekly_plan DROP CONSTRAINT ' + @chk_name
	EXEC sp_executesql @sql;

alter table weekly_plan drop column approved_by;
