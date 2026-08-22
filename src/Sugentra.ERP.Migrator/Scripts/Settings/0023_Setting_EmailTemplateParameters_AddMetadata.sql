-- Adds metadata columns to support future dynamic template rendering: typed values (arithmetic/round via
-- FormatString), loop-able list parameters (table rows in email body), required-ness, and a fallback default.
ALTER TABLE Setting_EmailTemplateParameters ADD
    DataType NVARCHAR(20) NOT NULL DEFAULT 'String', -- String | Int | Decimal | Float | Bool | Date | List
    IsLoop BIT NOT NULL DEFAULT 0,                    -- true when ParamValue represents a repeating collection
    FormatString NVARCHAR(50) NULL,                   -- e.g. "N2", "0.00", "yyyy-MM-dd" (also drives rounding)
    IsRequired BIT NOT NULL DEFAULT 0,
    DefaultValue NVARCHAR(500) NULL;
