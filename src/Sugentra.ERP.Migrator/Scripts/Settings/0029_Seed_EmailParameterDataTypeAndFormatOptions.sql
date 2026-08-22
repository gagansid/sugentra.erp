-- Options for the Email Template Parameters UI's DataType/FormatString dropdowns.
INSERT INTO Setting_SystemParameters (ParamCategory, ParamKey, ParamValue, Description)
SELECT v.ParamCategory, v.ParamKey, v.ParamValue, v.Description
FROM (VALUES
    ('EmailParamDataType', 'String', 'String', NULL),
    ('EmailParamDataType', 'Int', 'Int', NULL),
    ('EmailParamDataType', 'Decimal', 'Decimal', NULL),
    ('EmailParamDataType', 'Float', 'Float', NULL),
    ('EmailParamDataType', 'Bool', 'Bool', NULL),
    ('EmailParamDataType', 'Date', 'Date', NULL),
    ('EmailParamDataType', 'List', 'List', NULL),
    ('EmailParamFormatString', 'N0', 'N0', 'Integer with thousands separator, e.g. 1,234'),
    ('EmailParamFormatString', 'N2', 'N2', 'Decimal rounded to 2 places, e.g. 1,234.56'),
    ('EmailParamFormatString', '0.00', '0.00', 'Fixed 2 decimal places, no thousands separator'),
    ('EmailParamFormatString', 'C', 'C', 'Currency format, e.g. $1,234.56'),
    ('EmailParamFormatString', 'yyyy-MM-dd', 'yyyy-MM-dd', 'ISO date'),
    ('EmailParamFormatString', 'dd/MM/yyyy', 'dd/MM/yyyy', 'Day/Month/Year date')
) AS v(ParamCategory, ParamKey, ParamValue, Description)
WHERE NOT EXISTS (
    SELECT 1 FROM Setting_SystemParameters p
    WHERE p.ParamCategory = v.ParamCategory AND p.ParamKey = v.ParamKey
);
