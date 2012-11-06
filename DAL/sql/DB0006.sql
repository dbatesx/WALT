alter table tasks add created datetime not null default getdate();

alter table log add entry_type varchar(8) not null default 'INFO' check (entry_type in ('INFO', 'WARNING', 'ERROR'));

