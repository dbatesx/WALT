
alter table tasks add deleted bit not null default 0;

alter table tasks add modified datetime;

