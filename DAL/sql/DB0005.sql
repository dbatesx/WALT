create table barriers_descriptions (
	team_id bigint not null foreign key references teams (id),
	barrier_id bigint not null foreign key references barriers (id),
	description text,
	constraint barriers_descriptions_pk primary key (team_id, barrier_id)
);

create table unplanned_codes_descriptions (
	team_id bigint not null foreign key references teams (id),
	unplanned_code_id bigint not null foreign key references unplanned_codes (id),
	description text,
	constraint unplanned_codes_descriptions_pk primary key (team_id, unplanned_code_id)
);