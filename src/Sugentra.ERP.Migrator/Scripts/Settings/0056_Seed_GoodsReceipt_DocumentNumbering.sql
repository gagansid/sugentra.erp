-- Document numbering for the new Goods Receipt document type (format mirrors StockMutation's scheme).
IF NOT EXISTS (SELECT 1 FROM Setting_DocumentNumberings WHERE DocumentType = 'GoodsReceipt')
BEGIN
    INSERT INTO Setting_DocumentNumberings (DocumentType, Prefix, Suffix, NumberLength, ResetPeriod, CurrentNumber, FormatTemplate)
    VALUES ('GoodsReceipt', 'GR/', NULL, 4, 'Yearly', 0, '{Prefix}{Year}/{Number}');
END
GO
