-- Run this query on your PostgreSQL database to add the required columns:

ALTER TABLE public.delivery_order ADD COLUMN site_id TEXT;
ALTER TABLE public.delivery_order ADD COLUMN site_name TEXT;
