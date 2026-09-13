-- Document numbering for Batch codes - generated server-side via IDocumentNumberGeneratorService,
-- same as PurchaseOrder/GoodsReceipt/etc, instead of a client-generated timestamp-based code.
IF NOT EXISTS (SELECT 1 FROM Setting_DocumentNumberings WHERE DocumentType = 'Batch')
BEGIN
    INSERT INTO Setting_DocumentNumberings (DocumentType, Prefix, Suffix, NumberLength, ResetPeriod, CurrentNumber, FormatTemplate)
    VALUES ('Batch', 'BATCH/', NULL, 4, 'Yearly', 0, '{Prefix}{Year}/{Number}');
END
GO
