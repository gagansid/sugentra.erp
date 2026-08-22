using Sugentra.ERP.Api.Modules.Settings.Entities;

namespace Sugentra.ERP.Api.Modules.Settings.Dtos;

public record TestEmailSettingRequest(EmailSetting Settings, string ToEmail, string? Subject = null, string? Body = null);
