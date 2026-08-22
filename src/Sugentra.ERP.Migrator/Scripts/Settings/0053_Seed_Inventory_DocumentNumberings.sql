-- Seed initial Document Numbering configs for the Inventory module's transaction headers
-- (Inventory itself isn't implemented yet - this only prepares the numbering scheme referenced
-- in docs/modules/inventory.md so it's ready once StockMutations/StockOpnames tables exist).
IF NOT EXISTS (SELECT 1 FROM Setting_DocumentNumberings WHERE DocumentType = 'StockMutation')
BEGIN
    INSERT INTO Setting_DocumentNumberings (DocumentType, Prefix, Suffix, NumberLength, ResetPeriod, CurrentNumber, FormatTemplate)
    VALUES ('StockMutation', 'MUT/', NULL, 4, 'Yearly', 0, '{Prefix}{Year}/{Number}');
END

IF NOT EXISTS (SELECT 1 FROM Setting_DocumentNumberings WHERE DocumentType = 'StockOpname')
BEGIN
    INSERT INTO Setting_DocumentNumberings (DocumentType, Prefix, Suffix, NumberLength, ResetPeriod, CurrentNumber, FormatTemplate)
    VALUES ('StockOpname', 'OPN/', NULL, 4, 'Monthly', 0, '{Prefix}{Year}{Month}/{Number}');
END
GO
