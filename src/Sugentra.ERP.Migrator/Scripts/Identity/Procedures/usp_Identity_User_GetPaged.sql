-- usp_Identity_User_GetPaged: supports optional @Status filter (Active/Inactive/Locked/Banned).
-- Precedence mirrors the badge shown in the UI: Banned > Locked > Inactive > Active.
CREATE OR ALTER PROCEDURE usp_Identity_User_GetPaged
    @Username NVARCHAR(100) = NULL,
    @Email NVARCHAR(200) = NULL,
    @FullName NVARCHAR(200) = NULL,
    @Status NVARCHAR(20) = NULL,
    @Page INT,
    @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Username, Email, FullName, IsActive, LockoutEnd, IsBanned, COUNT(*) OVER() AS TotalCount
    FROM Identity_Users
    WHERE IsDeleted = 0
      AND (@Username IS NULL OR Username LIKE '%' + @Username + '%')
      AND (@Email IS NULL OR Email LIKE '%' + @Email + '%')
      AND (@FullName IS NULL OR FullName LIKE '%' + @FullName + '%')
      AND (
        @Status IS NULL
        OR (@Status = 'Banned' AND IsBanned = 1)
        OR (@Status = 'Locked' AND IsBanned = 0 AND LockoutEnd IS NOT NULL AND LockoutEnd > GETDATE())
        OR (@Status = 'Inactive' AND IsBanned = 0 AND (LockoutEnd IS NULL OR LockoutEnd <= GETDATE()) AND IsActive = 0)
        OR (@Status = 'Active' AND IsBanned = 0 AND (LockoutEnd IS NULL OR LockoutEnd <= GETDATE()) AND IsActive = 1)
      )
    ORDER BY Id
    OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
