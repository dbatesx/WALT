ALTER TABLE alerts DROP CONSTRAINT alerts_linked_type_chk;
ALTER TABLE alerts ADD CONSTRAINT alerts_linked_type_chk CHECK (linked_type in ('', 'TASK', 'WEEKLY_TASK', 'BARRIER', 'SYSTEM'));
