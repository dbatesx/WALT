

create table report_group_groups (
	parent_group_id bigint not null foreign key references report_groups (id),
	child_group_id bigint not null foreign key references report_groups (id),
	constraint report_group_groups_pk primary key (parent_group_id, child_group_id)
);

update reports set type = 'PARETO_UNPLANNED_TABLE' where type = 'Pareto - Unplanned Tasks - Table';
update reports set type = 'PARETO_UNPLANNED_GRAPH' where type = 'Pareto - Unplanned Tasks - Graph';
update reports set type = 'PARETO_UNPLANNED_RAW' where type = 'Pareto - Unplanned Tasks - Raw';
update reports set type = 'PARETO_EFFICIENCY_BARRIERS_TABLE' where type = 'Pareto - Task Efficiency Barriers - Table';
update reports set type = 'PARETO_EFFICIENCY_BARRIERS_GRAPH' where type = 'Pareto - Task Efficiency Barriers - Graph';
update reports set type = 'PARETO_EFFICIENCY_BARRIERS_RAW' where type = 'Pareto - Task Efficiency Barriers - Raw';
update reports set type = 'PARETO_DELAY_BARRIERS_TABLE' where type = 'Pareto - Task Delay Barriers - Table';
update reports set type = 'PARETO_DELAY_BARRIERS_GRAPH' where type = 'Pareto - Task Delay Barriers - Graph';
update reports set type = 'PARETO_DELAY_BARRIERS_RAW' where type = 'Pareto - Task Delay Barriers - Raw';
update reports set type = 'OPERATING_SUMMARY' where type = 'Operating Report Summary';
update reports set type = 'OPERATING_LOAD_TABLE' where type = 'Operating Report - Load - Table';
update reports set type = 'OPERATING_LOAD_GRAPH' where type = 'Operating Report - Load - Graph';
update reports set type = 'OPERATING_BARRIER_TABLE' where type = 'Operating Report - Efficiency Barrier Time - Table';
update reports set type = 'OPERATING_BARRIER_GRAPH' where type = 'Operating Report - Efficiency Barrier Time - Graph';
update reports set type = 'OPERATING_ADHERENCE_TABLE' where type = 'Operating Report - Plan Adherence - Table';
update reports set type = 'OPERATING_ADHERENCE_GRAPH' where type = 'Operating Report - Plan Adherence - Graph';
update reports set type = 'OPERATING_ATTAINMENT_TABLE' where type = 'Operating Report - Plan Attainment - Table';
update reports set type = 'OPERATING_ATTAINMENT_GRAPH' where type = 'Operating Report - Plan Attainment - Graph';
update reports set type = 'OPERATING_PRODUCTIVITY_TABLE' where type = 'Operating Report - Productivity - Table';
update reports set type = 'OPERATING_PRODUCTIVITY_GRAPH' where type = 'Operating Report - Productivity - Graph';
update reports set type = 'OPERATING_UNPLANNED_TABLE' where type = 'Operating Report - Unplanned - Table';
update reports set type = 'OPERATING_UNPLANNED_GRAPH' where type = 'Operating Report - Unplanned - Graph';
update reports set type = 'LOG_PARTICIPATION' where type = 'Auditing Report - Log Participation - Table';
update reports set type = 'SUMMARY_EFFICIENCY_BARRIER' where type = 'Summary Report - Efficiency Barrier - Table';
update reports set type = 'SUMMARY_DELAY_BARRIER' where type = 'Summary Report - Delay Barrier - Table';
update reports set type = 'SUMMARY_UNPLANNED' where type = 'Summary Report - Unplanned - Table';
update reports set type = 'TEAM_INFO' where type = 'WALT Team Information Report';
