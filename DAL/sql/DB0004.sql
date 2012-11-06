
ALTER table alerts ALTER column subject varchar(256) not null;
ALTER table alerts ALTER column message text;

create table weekly_plan_leave (
	weekly_plan_id bigint not null foreign key references weekly_plan (id),
	day_of_week int not null check (day_of_week >= 0 and day_of_week < 7),
	plan_hours float not null default 0 check (plan_hours >= 0),
	actual_hours float not null default 0 check (actual_hours >= 0),
	planned bit not null default 0,
	constraint weekly_plan_leave_pk primary key (weekly_plan_id, day_of_week)
);

ALTER table weekly_task_hours ADD plan_hours float;
ALTER table weekly_task_hours ADD actual_hours float;

UPDATE weekly_task_hours SET plan_hours = hours, actual_hours =
	(SELECT a.hours from weekly_task_actuals as a WHERE a.weekly_task_id = weekly_task_hours.weekly_task_id and a.day_of_week = weekly_task_hours.day_of_week);

UPDATE weekly_task_hours SET actual_hours = 0 WHERE actual_hours is null;
ALTER table weekly_task_hours DROP column hours;

INSERT INTO weekly_task_hours (weekly_task_id, day_of_week, plan_hours, actual_hours)
	SELECT weekly_task_id, day_of_week, 0 as plan_hours, hours as actual_hours
	FROM weekly_task_actuals as a WHERE (SELECT count(*) FROM weekly_task_hours as p WHERE a.weekly_task_id = p.weekly_task_id and a.day_of_week = p.day_of_week) = 0;

ALTER table weekly_task_hours ADD CONSTRAINT plan_hours_dft default 0 FOR plan_hours;
ALTER table weekly_task_hours ADD CONSTRAINT actual_hours_dft default 0 FOR actual_hours;
ALTER table weekly_task_hours ALTER column plan_hours float not null;
ALTER table weekly_task_hours ALTER column actual_hours float not null;
ALTER table weekly_task_hours ADD CONSTRAINT plan_hours_chk CHECK (plan_hours >= 0);
ALTER table weekly_task_hours ADD CONSTRAINT actual_hours_chk CHECK (actual_hours >= 0);

DROP table weekly_task_actuals;

