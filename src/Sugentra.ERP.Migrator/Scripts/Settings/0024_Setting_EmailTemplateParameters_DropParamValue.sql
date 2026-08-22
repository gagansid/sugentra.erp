-- ParamValue is redundant with DefaultValue now that this table is a placeholder catalog
-- (metadata only), not a live key/value settings store.
ALTER TABLE Setting_EmailTemplateParameters DROP COLUMN ParamValue;
