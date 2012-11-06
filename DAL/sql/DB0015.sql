UPDATE tasks SET source = 'WALT' WHERE source is null or source = '';
UPDATE tasks SET source_id = 'WALT_' + convert(varchar(16), id) WHERE source_id is null or source_id = '';

ALTER TABLE tasks ALTER COLUMN source varchar(32) not null;
ALTER TABLE tasks ALTER COLUMN source_id varchar(64) not null;

CREATE UNIQUE INDEX tasks_i4 on tasks (source, source_id);
