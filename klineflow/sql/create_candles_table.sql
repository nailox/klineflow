-- SQL script to create the Candles table for Postgres/Supabase
-- Run this in the Supabase SQL editor or via psql connected to your Supabase database

CREATE SCHEMA IF NOT EXISTS public;

CREATE TABLE IF NOT EXISTS public."Candles" (
 "Id" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
 "Symbol" varchar(30) NOT NULL,
 "OpenTime" bigint NOT NULL,
 "Open" numeric(38,18) NOT NULL,
 "High" numeric(38,18) NOT NULL,
 "Low" numeric(38,18) NOT NULL,
 "Close" numeric(38,18) NOT NULL,
 "Volume" numeric(38,18) NOT NULL,
 "CloseTime" bigint NOT NULL,
 "QuoteAssetVolume" numeric(38,18) NOT NULL,
 "NumberOfTrades" integer NOT NULL,
 "TakerBuyBaseAssetVolume" numeric(38,18) NOT NULL,
 "TakerBuyQuoteAssetVolume" numeric(38,18) NOT NULL
);

-- Unique index to prevent duplicate candles for the same symbol and open time
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Candles_Symbol_OpenTime"
 ON public."Candles" ("Symbol", "OpenTime");

-- Example insert with ON CONFLICT DO NOTHING (use from Supabase REST or psql)
-- INSERT INTO public."Candles" ("Symbol","OpenTime","Open","High","Low","Close","Volume","CloseTime","QuoteAssetVolume","NumberOfTrades","TakerBuyBaseAssetVolume","TakerBuyQuoteAssetVolume")
-- VALUES ('BTCUSDT',1633024800000,43000.123456789012345678,43500.123456789012345678,42900.123456789012345678,43300.123456789012345678,120.123456789012345678,1633028399999,5200000.123456789012345678,1234,60.123456789012345678,2600000.123456789012345678)
-- ON CONFLICT ("Symbol","OpenTime") DO NOTHING;
