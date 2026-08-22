-- Adds the "Day" DataType (day-of-month or weekday name) and its FormatString options.
INSERT INTO Setting_SystemParameters (ParamCategory, ParamKey, ParamValue, Description)
SELECT v.ParamCategory, v.ParamKey, v.ParamValue, v.Description
FROM (VALUES
    ('EmailParamDataType', 'Day', 'Day', NULL)
) AS v(ParamCategory, ParamKey, ParamValue, Description)
WHERE NOT EXISTS (
    SELECT 1 FROM Setting_SystemParameters p
    WHERE p.ParamCategory = v.ParamCategory AND p.ParamKey = v.ParamKey
);

INSERT INTO Setting_ParamFormatOptions (DataType, FormatString, Label, SortOrder)
SELECT v.DataType, v.FormatString, v.Label, v.SortOrder
FROM (VALUES
    ('Day', 'dd', 'Day of month, zero-padded, e.g. 08', 1),
    ('Day', 'd', 'Day of month, e.g. 8', 2),
    ('Day', 'dddd', 'Weekday full name (English), e.g. Saturday', 3),
    ('Day', 'ddd', 'Weekday short name (English), e.g. Sat', 4),
    ('Day', 'dddd_id', 'Weekday full name (Indonesian), e.g. Sabtu', 5),
    ('Day', 'ddd_id', 'Weekday short name (Indonesian), e.g. Sab', 6)
) AS v(DataType, FormatString, Label, SortOrder)
WHERE NOT EXISTS (
    SELECT 1 FROM Setting_ParamFormatOptions o
    WHERE o.DataType = v.DataType AND o.FormatString = v.FormatString
);
