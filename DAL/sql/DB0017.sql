
drop index barriers.barriers_i1;
drop index barriers.barriers_i2;
create unique index barriers_i1 on barriers (team_id, code);
create unique index barriers_i2 on barriers (team_id, parent_id, title);

create unique index unplanned_codes_i1 on unplanned_codes (team_id, code);
create unique index unplanned_codes_i2 on unplanned_codes (team_id, parent_id, title);

create unique index team_org_codes_i1 on team_org_codes (org_code);
