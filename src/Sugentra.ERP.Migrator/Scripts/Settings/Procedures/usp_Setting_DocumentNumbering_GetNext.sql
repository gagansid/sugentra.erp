-- usp_Setting_DocumentNumbering_GetNext: concurrency-safe next-number generation (UPDLOCK+HOLDLOCK avoids
-- read-then-write races between two requests for the same DocumentType under load).
CREATE OR ALTER PROCEDURE usp_Setting_DocumentNumbering_GetNext
    @DocumentType NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @Prefix NVARCHAR(20), @Suffix NVARCHAR(20), @NumberLength INT, @ResetPeriod NVARCHAR(20),
            @CurrentNumber INT, @LastResetDate DATETIME2, @FormatTemplate NVARCHAR(100), @Now DATETIME2 = GETDATE();

    SELECT
        @Prefix = Prefix, @Suffix = Suffix, @NumberLength = NumberLength, @ResetPeriod = ResetPeriod,
        @CurrentNumber = CurrentNumber, @LastResetDate = LastResetDate, @FormatTemplate = FormatTemplate
    FROM Setting_DocumentNumberings WITH (UPDLOCK, HOLDLOCK)
    WHERE DocumentType = @DocumentType AND IsDeleted = 0;

    IF @@ROWCOUNT = 0
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('DocumentType "%s" is not configured in Setting_DocumentNumberings.', 16, 1, @DocumentType);
        RETURN;
    END

    DECLARE @ShouldReset BIT = 0;
    IF @ResetPeriod = 'Yearly' AND (@LastResetDate IS NULL OR YEAR(@LastResetDate) <> YEAR(@Now)) SET @ShouldReset = 1;
    IF @ResetPeriod = 'Monthly' AND (@LastResetDate IS NULL OR EOMONTH(@LastResetDate) <> EOMONTH(@Now)) SET @ShouldReset = 1;
    IF @ResetPeriod = 'Weekly' AND (@LastResetDate IS NULL OR DATEPART(YEAR, @LastResetDate) <> DATEPART(YEAR, @Now) OR DATEPART(ISO_WEEK, @LastResetDate) <> DATEPART(ISO_WEEK, @Now)) SET @ShouldReset = 1;
    IF @ResetPeriod = 'Daily' AND (@LastResetDate IS NULL OR CAST(@LastResetDate AS DATE) <> CAST(@Now AS DATE)) SET @ShouldReset = 1;

    SET @CurrentNumber = CASE WHEN @ShouldReset = 1 THEN 1 ELSE @CurrentNumber + 1 END;

    UPDATE Setting_DocumentNumberings
    SET CurrentNumber = @CurrentNumber,
        LastResetDate = CASE WHEN @ShouldReset = 1 THEN @Now ELSE LastResetDate END,
        UpdatedAt = @Now
    WHERE DocumentType = @DocumentType;

    COMMIT TRANSACTION;

    DECLARE @PaddedNumber NVARCHAR(20) = RIGHT(REPLICATE('0', @NumberLength) + CAST(@CurrentNumber AS NVARCHAR(20)), @NumberLength);
    DECLARE @FormattedNumber NVARCHAR(200) = COALESCE(@FormatTemplate, '{Prefix}{Number}{Suffix}');

    -- Date-part tokens (all derived from @Now, offered in every common presentation so any DocumentType/locale need is covered).
    DECLARE @MonthNum INT = MONTH(@Now), @DayNum INT = DAY(@Now);
    DECLARE @MonthRoman NVARCHAR(4) = CASE @MonthNum
        WHEN 1 THEN 'I' WHEN 2 THEN 'II' WHEN 3 THEN 'III' WHEN 4 THEN 'IV' WHEN 5 THEN 'V' WHEN 6 THEN 'VI'
        WHEN 7 THEN 'VII' WHEN 8 THEN 'VIII' WHEN 9 THEN 'IX' WHEN 10 THEN 'X' WHEN 11 THEN 'XI' ELSE 'XII' END;

    SET @FormattedNumber = REPLACE(@FormattedNumber, '{Prefix}', ISNULL(@Prefix, ''));
    SET @FormattedNumber = REPLACE(@FormattedNumber, '{Suffix}', ISNULL(@Suffix, ''));
    SET @FormattedNumber = REPLACE(@FormattedNumber, '{Number}', @PaddedNumber);
    -- Year: {Year} = 2026, {YY} = 26
    SET @FormattedNumber = REPLACE(@FormattedNumber, '{Year}', CAST(YEAR(@Now) AS NVARCHAR(4)));
    SET @FormattedNumber = REPLACE(@FormattedNumber, '{YY}', RIGHT(CAST(YEAR(@Now) AS NVARCHAR(4)), 2));
    -- Month: {Month}/{MM} = 08, {M} = 8, {MonthShort} = Aug, {MonthName} = August, {MonthRoman} = VIII
    SET @FormattedNumber = REPLACE(@FormattedNumber, '{MonthShort}', LEFT(DATENAME(MONTH, @Now), 3));
    SET @FormattedNumber = REPLACE(@FormattedNumber, '{MonthName}', DATENAME(MONTH, @Now));
    SET @FormattedNumber = REPLACE(@FormattedNumber, '{MonthRoman}', @MonthRoman);
    SET @FormattedNumber = REPLACE(@FormattedNumber, '{Month}', RIGHT('0' + CAST(@MonthNum AS NVARCHAR(2)), 2));
    SET @FormattedNumber = REPLACE(@FormattedNumber, '{MM}', RIGHT('0' + CAST(@MonthNum AS NVARCHAR(2)), 2));
    SET @FormattedNumber = REPLACE(@FormattedNumber, '{M}', CAST(@MonthNum AS NVARCHAR(2)));
    -- Day: {Day}/{DD} = 05, {D} = 5, {DayShort} = Mon, {DayName} = Monday
    SET @FormattedNumber = REPLACE(@FormattedNumber, '{DayShort}', LEFT(DATENAME(WEEKDAY, @Now), 3));
    SET @FormattedNumber = REPLACE(@FormattedNumber, '{DayName}', DATENAME(WEEKDAY, @Now));
    SET @FormattedNumber = REPLACE(@FormattedNumber, '{Day}', RIGHT('0' + CAST(@DayNum AS NVARCHAR(2)), 2));
    SET @FormattedNumber = REPLACE(@FormattedNumber, '{DD}', RIGHT('0' + CAST(@DayNum AS NVARCHAR(2)), 2));
    SET @FormattedNumber = REPLACE(@FormattedNumber, '{D}', CAST(@DayNum AS NVARCHAR(2)));

    SELECT @CurrentNumber AS CurrentNumber, @FormattedNumber AS FormattedNumber;
END
GO
