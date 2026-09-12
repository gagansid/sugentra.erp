-- Seed Document Numbering configs for Procurement's transaction headers (PurchaseRequisition, PurchaseOrder).
IF NOT EXISTS (SELECT 1 FROM Setting_DocumentNumberings WHERE DocumentType = 'PurchaseRequisition')
BEGIN
    INSERT INTO Setting_DocumentNumberings (DocumentType, Prefix, Suffix, NumberLength, ResetPeriod, CurrentNumber, FormatTemplate)
    VALUES ('PurchaseRequisition', 'PR/', NULL, 4, 'Yearly', 0, '{Prefix}{Year}/{Number}');
END

IF NOT EXISTS (SELECT 1 FROM Setting_DocumentNumberings WHERE DocumentType = 'PurchaseOrder')
BEGIN
    INSERT INTO Setting_DocumentNumberings (DocumentType, Prefix, Suffix, NumberLength, ResetPeriod, CurrentNumber, FormatTemplate)
    VALUES ('PurchaseOrder', 'PO/', NULL, 4, 'Yearly', 0, '{Prefix}{Year}/{Number}');
END
GO
