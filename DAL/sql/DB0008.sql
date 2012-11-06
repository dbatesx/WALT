
alter table tasks add instantiated bit not null default 0;
alter table tasks add fully_allocated bit not null default 0;
