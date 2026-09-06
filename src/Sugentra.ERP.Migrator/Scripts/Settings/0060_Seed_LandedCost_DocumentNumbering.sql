-- Document numbering for the LandedCost document type (format mirrors GoodsReceipt/StockMutation's scheme).
IF NOT EXISTS (SELECT 1 FROM Setting_DocumentNumberings WHERE DocumentType = 'LandedCost')
BEGIN
    INSERT INTO Setting_DocumentNumberings (DocumentType, Prefix, Suffix, NumberLength, ResetPeriod, CurrentNumber, FormatTemplate)
    VALUES ('LandedCost', 'LC/', NULL, 4, 'Yearly', 0, '{Prefix}{Year}/{Number}');
END
