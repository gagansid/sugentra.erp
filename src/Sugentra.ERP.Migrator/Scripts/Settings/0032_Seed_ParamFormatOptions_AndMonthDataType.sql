-- Adds the "Month" DataType option and seeds Setting_ParamFormatOptions with the full set of FormatStrings per
-- DataType, replacing the old EmailParamFormatString rows in Setting_SystemParameters (kept in place, just unused).
INSERT INTO Setting_SystemParameters (ParamCategory, ParamKey, ParamValue, Description)
SELECT v.ParamCategory, v.ParamKey, v.ParamValue, v.Description
FROM (VALUES
    ('EmailParamDataType', 'Month', 'Month', NULL)
) AS v(ParamCategory, ParamKey, ParamValue, Description)
WHERE NOT EXISTS (
    SELECT 1 FROM Setting_SystemParameters p
    WHERE p.ParamCategory = v.ParamCategory AND p.ParamKey = v.ParamKey
);

INSERT INTO Setting_ParamFormatOptions (DataType, FormatString, Label, SortOrder)
SELECT v.DataType, v.FormatString, v.Label, v.SortOrder
FROM (VALUES
    ('String', '', 'Plain text', 1),

    ('Int', '0', 'Plain number, e.g. 1234', 1),
    ('Int', 'N0', 'Thousands separator, e.g. 1,234', 2),
    ('Int', 'D3', 'Zero-padded, e.g. 007', 3),

    ('Decimal', '0.##', 'Up to 2 decimals, trimmed', 1),
    ('Decimal', '0.0', '1 decimal place', 2),
    ('Decimal', '0.00', '2 decimal places', 3),
    ('Decimal', '0.0000', '4 decimal places', 4),
    ('Decimal', 'N2', 'Thousands separator + 2 decimals, e.g. 1,234.56', 5),
    ('Decimal', 'C', 'Currency, e.g. $1,234.56', 6),
    ('Decimal', 'P2', 'Percentage, e.g. 12.34%', 7),

    ('Float', '0.##', 'Up to 2 decimals, trimmed', 1),
    ('Float', '0.00', '2 decimal places', 2),
    ('Float', 'N2', 'Thousands separator + 2 decimals', 3),

    ('Date', 'yyyy-MM-dd', 'ISO date, e.g. 2026-08-08', 1),
    ('Date', 'dd/MM/yyyy', 'Day/Month/Year, e.g. 08/08/2026', 2),
    ('Date', 'MM/dd/yyyy', 'Month/Day/Year, e.g. 08/08/2026', 3),
    ('Date', 'dd-MM-yyyy', 'Day-Month-Year, e.g. 08-08-2026', 4),
    ('Date', 'dd MMMM yyyy', 'Long date (English), e.g. 08 August 2026', 5),
    ('Date', 'dd MMM yyyy', 'Short month name (English), e.g. 08 Aug 2026', 6),
    ('Date', 'yyyy-MM-dd HH:mm', 'Date + time (24h)', 7),
    ('Date', 'yyyy-MM-dd HH:mm:ss', 'Date + time with seconds', 8),

    ('Month', 'MM', 'Zero-padded number, e.g. 08', 1),
    ('Month', 'M', 'Number, e.g. 8', 2),
    ('Month', 'MMMM', 'Full name (English), e.g. August', 3),
    ('Month', 'MMM', 'Short name (English), e.g. Aug', 4),
    ('Month', 'MMMM_id', 'Full name (Indonesian), e.g. Agustus', 5),
    ('Month', 'MMM_id', 'Short name (Indonesian), e.g. Agu', 6),

    ('Bool', '', 'Yes / No', 1),
    ('Bool', 'Ya;Tidak', 'Ya / Tidak', 2),
    ('Bool', 'Active;Inactive', 'Active / Inactive', 3),

    ('List', ', ', 'Comma-separated', 1),
    ('List', '; ', 'Semicolon-separated', 2),
    ('List', '<br/>', 'One per line (HTML)', 3)
) AS v(DataType, FormatString, Label, SortOrder)
WHERE NOT EXISTS (
    SELECT 1 FROM Setting_ParamFormatOptions o
    WHERE o.DataType = v.DataType AND o.FormatString = v.FormatString
);
