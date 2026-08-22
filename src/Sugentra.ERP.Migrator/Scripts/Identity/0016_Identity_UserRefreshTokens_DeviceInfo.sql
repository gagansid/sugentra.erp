-- Adds device/IP capture to support the Users > Detail > Login History/Sessions UI tab.
ALTER TABLE Identity_UserRefreshTokens ADD DeviceInfo NVARCHAR(300) NULL;
ALTER TABLE Identity_UserRefreshTokens ADD IpAddress NVARCHAR(64) NULL;
