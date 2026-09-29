-- Additive-only migration for Krabi STP mode. Safe to run on every startup,
-- on every install (Standard or Krabi mode) - see
-- docs/superpowers/specs/2026-09-29-krabi-stp-mode-design.md, "Database".

ALTER TABLE weight ADD COLUMN IF NOT EXISTS line_type varchar(10);
ALTER TABLE weight ADD COLUMN IF NOT EXISTS origin_weight numeric;
ALTER TABLE weight ADD COLUMN IF NOT EXISTS origin_q numeric;

CREATE TABLE IF NOT EXISTS base_setting_line (
    id serial PRIMARY KEY,
    base_setting_line_date_from date NULL,
    base_setting_line_time_from time NULL
);

-- Task 5: line-type segmented daily totals for Krabi STP mode - see
-- docs/superpowers/specs/2026-09-29-krabi-stp-mode-design.md, item 2.
ALTER TABLE base_setting_line ADD COLUMN IF NOT EXISTS base_site_name varchar(100);
ALTER TABLE base_setting_line ADD COLUMN IF NOT EXISTS num_short varchar(20);
ALTER TABLE base_setting_line ADD COLUMN IF NOT EXISTS num_long varchar(20);
ALTER TABLE base_setting_line ADD COLUMN IF NOT EXISTS num_total varchar(20);
ALTER TABLE base_setting_line ADD COLUMN IF NOT EXISTS weight_short varchar(20);
ALTER TABLE base_setting_line ADD COLUMN IF NOT EXISTS weight_long varchar(20);
ALTER TABLE base_setting_line ADD COLUMN IF NOT EXISTS weight_total varchar(20);
