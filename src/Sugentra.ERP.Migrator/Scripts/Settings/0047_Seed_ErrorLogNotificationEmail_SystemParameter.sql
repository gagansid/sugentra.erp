-- Default recipient used to pre-fill the "Send Email" modal on the Error Log Detail page; admin-editable.
INSERT INTO Setting_SystemParameters (ParamCategory, ParamKey, ParamValue, Description)
SELECT 'ErrorLog', 'NotificationEmail', 'gaganbaonkk@gmail.com', 'Default recipient for error log notification emails.'
WHERE NOT EXISTS (
    SELECT 1 FROM Setting_SystemParameters
    WHERE ParamCategory = 'ErrorLog' AND ParamKey = 'NotificationEmail'
);
