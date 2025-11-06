-- SQL script to create the Candles table for Postgres/Supabase
-- Run this in the Supabase SQL editor or via psql connected to your Supabase database

CREATE SCHEMA IF NOT EXISTS public;

CREATE TABLE IF NOT EXISTS public."candles" (
 "id" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
 "symbol" varchar(30) NOT NULL,
 "opentime" bigint NOT NULL,
 "open" numeric(38,18) NOT NULL,
 "high" numeric(38,18) NOT NULL,
 "low" numeric(38,18) NOT NULL,
 "close" numeric(38,18) NOT NULL,
 "volume" numeric(38,18) NOT NULL,
 "closetime" bigint NOT NULL,
 "quoteassetvolume" numeric(38,18) NOT NULL,
 "numberoftrades" integer NOT NULL,
 "takerbuybaseassetvolume" numeric(38,18) NOT NULL,
 "takerbuyquoteassetvolume" numeric(38,18) NOT NULL
);

-- Unique index to prevent duplicate candles for the same symbol and open time
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Candles_Symbol_OpenTime"
 ON public."candles" ("symbol", "opentime");

-- Example insert with ON CONFLICT DO NOTHING (use from Supabase REST or psql)
-- INSERT INTO public."candles" ("symbol","opentime","open","high","low","close","volume","closetime","quoteassetvolume","numberoftrades","takerbuybaseassetvolume","takerbuyquoteassetvolume")
-- VALUES ('BTCUSDT',1633024800000,43000.123456789012345678,43500.123456789012345678,42900.123456789012345678,43300.123456789012345678,120.123456789012345678,1633028399999,5200000.123456789012345678,1234,60.123456789012345678,2600000.123456789012345678)
-- ON CONFLICT ("symbol","opentime") DO NOTHING;
