
create table actions (
	id bigint primary key identity,
	title varchar(32) not null
);

create table profiles (
	id bigint primary key identity,
	username varchar(32) not null,
	display_name varchar(64),
	employee_id varchar(32),
	exempt_plan bit not null default 0,
	exempt_task bit not null default 0,
	can_task bit not null default 1,
	active bit not null default 1,
	org_code varchar(32),
	manager bigint foreign key references profiles (id)
);

create unique index profiles_i1 on profiles (username);

create table alerts (
	id bigint primary key identity,
	profile_id bigint not null foreign key references profiles (id),
	creator_id bigint not null foreign key references profiles (id),
	entry_date datetime not null default getdate(),
	subject text not null,
	message text not null,
	linked_id bigint,
	linked_type varchar(16) check (linked_type in ('', 'TASK', 'BARRIER')),
	acknowledged bit not null default 0
);

create index alerts_i1 on alerts (profile_id);
create index alerts_i2 on alerts (creator_id);
create index alerts_i3 on alerts (acknowledged);

create table teams (
	id bigint primary key identity,
	parent_id bigint foreign key references teams (id),
	title varchar(128) not null,
	complexity_based bit not null default 0,
	active bit not null default 1,
	owner_id bigint foreign key references profiles (id),
	type varchar(32) not null check (type in ('ORG', 'DIRECTORATE', 'TEAM'))
);

create unique index teams_i1 on teams (title);

create table barriers (
	id bigint primary key identity,
	team_id bigint not null foreign key references teams (id),
	parent_id bigint foreign key references barriers (id),
	code varchar(32) not null,
	title varchar(128) not null,
	description text,
	active bit not null default 1
);

create unique index barriers_i1 on barriers (code);
create unique index barriers_i2 on barriers (parent_id, id);

create table task_types (
	id bigint primary key identity,
	parent_id bigint foreign key references task_types (id),
	team_id bigint foreign key references teams (id),
	title varchar(128) not null,
	description text,
	active bit not null default 1
);

create unique index taks_types_i1 on task_types (team_id, parent_id, title);

create table complexities (
	id bigint primary key identity,
	team_id bigint not null foreign key references teams (id),
	task_type_id bigint not null foreign key references task_types (id),
	title varchar(128) not null,
	active bit not null default 1,
	hours float not null
);

create unique index complexities_i1 on complexities (team_id, task_type_id, title);

create table programs (
	id bigint primary key identity,
	title varchar(128) not null,
	active bit not null default 1
);

create unique index programs_i1 on programs (title);

create table favorites (
	id bigint primary key identity,
	profile_id bigint not null foreign key references profiles (id),
	program_id bigint foreign key references programs (id),
	complexity_id bigint foreign key references complexities (id),
	task_type_id bigint foreign key references task_types (id),
	title varchar(128) not null,
	hours float,
	estimate float,
	template bit not null default 0
);

create unique index favorites_i1 on favorites (profile_id, title);
create index favorites_i2 on favorites (profile_id);
create index favorites_i3 on favorites (profile_id, template);

create table favorite_plan_hours (
	favorite_id bigint not null foreign key references favorites (id),
	day_of_week int not null check (day_of_week >= 0 and day_of_week < 7),
	hours float not null,
	constraint favorites_pk primary key (favorite_id, day_of_week)
);

create index favorite_plan_hours_i1 on favorite_plan_hours (favorite_id);

create table profile_log (
	id bigint primary key identity,
	profile_id bigint not null foreign key references profiles (id),
	entry_date datetime not null default getdate(),
	category varchar(32) not null,
	comment text,
	source_id bigint
);

create table preferences (
	id bigint primary key identity,
	profile_id bigint not null foreign key references profiles (id),
	name varchar(32) not null,
	value varchar(MAX) not null
);

create table report_groups (
	id bigint primary key identity,
	title varchar(128) not null,
	description text,
	profile_id bigint not null foreign key references profiles (id),
	public_flag bit not null default 0
);

create table report_group_profiles (
	report_group_id bigint not null foreign key references report_groups (id),
	profile_id bigint not null foreign key references profiles (id),
	constraint report_group_profiles_pk primary key (report_group_id, profile_id)
);

create unique index report_group_profiles_i1 on report_group_profiles (report_group_id, profile_id);

create table report_group_teams (
	report_group_id bigint not null foreign key references report_groups (id),
	team_id bigint not null foreign key references teams (id),
	constraint report_group_teams_pk primary key (report_group_id, team_id)
);

create unique index report_group_teams_i1 on report_group_teams (report_group_id, team_id);

create table reports (
	id bigint primary key identity,
	profile_id bigint not null foreign key references profiles (id),
	report_group_id bigint not null foreign key references report_groups (id),
	title varchar(128) not null,
	description text,
	public_flag bit not null default 0,
	type varchar(128) not null,
	from_date datetime,
	to_date datetime,
	percent_base int default 0,
	percent_goal int default 0
);

create table roles (
	id bigint primary key identity,
	title varchar(128) not null,
	description text,
	active bit not null default 1
);

create table role_actions (
	role_id bigint not null foreign key references roles (id),
	action_id bigint not null foreign key references actions (id),
	constraint role_actions_pk primary key (role_id, action_id)
);

create unique index role_actions_i1 on role_actions (role_id, action_id);

create table role_profiles (
	role_id bigint not null foreign key references roles (id),
	profile_id bigint not null foreign key references profiles (id)
	constraint role_profiles_pk primary key (role_id, profile_id)
);

create unique index role_profiles_i1 on role_profiles (role_id, profile_id);

create table tasks (
	id bigint primary key identity,
	parent_id bigint foreign key references tasks (id),
	task_type_id bigint foreign key references task_types (id),
	profile_id bigint foreign key references profiles (id),
	owner_id bigint not null foreign key references profiles (id),
	program_id bigint foreign key references programs (id),
	complexity_id bigint foreign key references complexities (id),
	title varchar(128) not null,
	active bit not null default 1,
	source varchar(32),
	source_id varchar(32),	
	start_date datetime,
	due_date datetime,
	status varchar(16) not null check (status in ('OPEN', 'HOLD', 'OBE', 'COMPLETED', 'REJECTED')) default 'OPEN',
	hours float,
	completed_date datetime,
	estimate float,
	exit_criteria text,
	wbs varchar(64),
	owner_comments text,
	assignee_comments text
);

create index tasks_i1 on tasks (profile_id);
create index tasks_i2 on tasks (parent_id);
create index tasks_i3 on tasks (owner_id);

create table task_issues (
	id bigint primary key identity,
	task_id bigint not null foreign key references tasks (id),
	profile_id bigint not null foreign key references profiles (id),
	elevate bit not null default 1,
	comment text not null,
	acknowleged bit not null default 0
);

create table team_barriers (
	team_id bigint not null foreign key references teams (id),
	barrier_id bigint not null foreign key references barriers (id),
	constraint team_barriers_pk primary key (team_id, barrier_id)
);

create table team_profiles (
	team_id bigint not null foreign key references teams (id),
	profile_id bigint not null foreign key references profiles (id),
	role varchar(16) check (role in ('ADMIN', 'MANAGER', 'MEMBER')),
	constraint team_profiles_pk primary key (team_id, profile_id, role)
);

create table team_task_types (
	team_id bigint not null foreign key references teams (id),
	task_type_id bigint not null foreign key references task_types (id),
	constraint team_task_types_pk primary key (team_id, task_type_id)
);

create table team_org_codes (
	team_id bigint not null foreign key references teams (id),
	org_code varchar(32),
	constraint team_org_codes_pk primary key (team_id, org_code)
);

create table unplanned_codes (
	id bigint primary key identity,
	parent_id bigint foreign key references unplanned_codes (id),
	team_id bigint not null foreign key references teams (id),
	code varchar(32) not null,
	title varchar(128) not null,
	description text,
	active bit not null default 1
);

create table team_unplanned_codes (
	team_id bigint not null foreign key references teams (id),
	unplanned_code_id bigint not null foreign key references unplanned_codes (id)
	constraint team_unplanned_codes_pk primary key (team_id, unplanned_code_id)
);

create table weekly_plan (
	id bigint primary key identity,
	profile_id bigint not null foreign key references profiles (id),
	approved_by bigint foreign key references profiles (id),
	team_id bigint not null foreign key references teams (id),
	week_ending datetime not null,
	plan_submitted datetime,
	log_submitted datetime,
	state varchar(16) check (state in ('NEW', 'PLAN_READY', 'PLAN_APPROVED', 'LOG_READY', 'LOG_APPROVED')),
	modified datetime not null default getdate()
);

create unique index weekly_plan_i1 on weekly_plan (profile_id, week_ending);
create index weekly_plan_i2 on weekly_plan (team_id, week_ending);
create index weekly_plan_i3 on weekly_plan (state);

create table weekly_tasks (
	id bigint primary key identity,
	weekly_plan_id bigint not null foreign key references weekly_plan (id),
	task_id bigint not null foreign key references tasks (id),
	unplanned bit not null default 0,
	unplanned_code_id bigint foreign key references unplanned_codes (id),
	comment text,
	plan_day_complete int check (plan_day_complete >= 0 and plan_day_complete < 7),
	actual_day_complete int check (actual_day_complete >= 0 and actual_day_complete < 7)
);

create unique index weekly_tasks_i1 on weekly_tasks (weekly_plan_id, task_id);
create index weekly_tasks_i2 on weekly_tasks (task_id);
create index weekly_tasks_i3 on weekly_tasks (weekly_plan_id);

create table weekly_task_actuals (
	weekly_task_id bigint not null foreign key references weekly_tasks (id),	
	day_of_week int not null check (day_of_week >= 0 and day_of_week < 7),
	hours float not null,
	constraint weekly_task_actuals_pk primary key (weekly_task_id, day_of_week)
);

create index weekly_task_actuals_i1 on weekly_task_actuals (weekly_task_id);

create table weekly_task_hours (
	weekly_task_id bigint not null foreign key references weekly_tasks (id),
	day_of_week int not null check (day_of_week >= 0 and day_of_week < 7),
	hours float not null,
	constraint weekly_task_hours_pk primary key (weekly_task_id, day_of_week)
);

create index weekly_task_hours_i1 on weekly_task_hours (weekly_task_id);

create table weekly_task_barriers (
	id bigint primary key identity,
	barrier_id bigint not null foreign key references barriers (id),
	barrier_type varchar(16) not null check (barrier_type in ('EFFICIENCY', 'DELAY')),
	weekly_task_id bigint not null foreign key references weekly_tasks (id),
	comment varchar(512) not null,
	ticket varchar(64)
);

create unique index weekly_task_barriers_i1 on weekly_task_barriers (barrier_id, barrier_type, weekly_task_id, comment);

create table weekly_task_barrier_hours (
	weekly_task_barrier_id bigint not null foreign key references weekly_task_barriers(id),
	day_of_week int not null check (day_of_week >= 0 and day_of_week < 7),
	hours float not null,
	constraint weekly_task_barrier_hours_pk primary key (weekly_task_barrier_id, day_of_week)
);

create index weekly_task_barrier_hours_i1 on weekly_task_barrier_hours (weekly_task_barrier_id);

create table log (
	id bigint primary key identity,
	entry_date datetime not null default getdate(),
	profile_id bigint not null foreign key references profiles (id),
	category varchar(32) not null,
	source_id bigint not null,
	comment text not null	
);
