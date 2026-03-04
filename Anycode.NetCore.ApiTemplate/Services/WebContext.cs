namespace Anycode.NetCore.ApiTemplate.Services;

public class WebContext(IHttpContextAccessor httpContextAccessor, LanguagesCacheService languagesCache, JwtConfig jwtConfig, ILogger<WebContext> log)
{
	private HttpContext HttpContext => httpContextAccessor.HttpContext ?? throw new Exception("HttpContext is null");

	public int GetUserLanguageId()
	{
		var language = HttpContext.GetHeader(AppHeaders.Language);
		if (language == null)
			return AppConstants.DefaultLanguage;

		if (languagesCache.GetAllEntitiesByCodes().TryGetValue(language, out var languageEntity))
			return languageEntity.Id;

		return AppConstants.DefaultLanguage;
	}

	public UserIpInfo GetIpInfo()
	{
		string? ip;
		string? countryCode;
		var useCloudFlare = false;
		if (useCloudFlare) // Non-ru domain - CloudFlare
		{
			ip = HttpContext.GetHeader(DefaultHeaders.CloudflareIp);
			countryCode = HttpContext.GetHeader(DefaultHeaders.CloudflareCountryCode);
		}
		else // Ru domain, Nginx proxy
		{
			ip = HttpContext.GetXIp();
			countryCode = HttpContext.GetHeader(DefaultHeaders.NginxCountryCode);
		}

		if (ip?.Length > DbConstraints.MaxIpLength)
		{
			log.Warn("Client's IP is too long: {ClientIp}", ip);
			ip = ip[..DbConstraints.MaxIpLength];
		}

		if (countryCode?.Length > DbConstraints.MaxCountryCodeLength)
		{
			log.Warn("Client's country code is too long: {ClientCountryCode}", countryCode);
			countryCode = countryCode[..DbConstraints.MaxCountryCodeLength];
		}

		return new UserIpInfo
		{
			Ip = ip,
			CountryCode = countryCode,
		};
	}

	public UserSourceInfo? GetSource()
	{
		var base64Source = HttpContext.GetHeader(AppHeaders.Source);
		if (string.IsNullOrEmpty(base64Source))
			return null;

		try
		{
			var sourceJson = Encoding.UTF8.GetString(Convert.FromBase64String(base64Source));
			var result = JsonSerializer.Deserialize<UserSourceInfo>(sourceJson, JsonHelper.JsonApiOptions);
			if (result == null)
				return null;

			result.FirstPage = result.FirstPage.Truncate(DbConstraints.MaxPageLength)!;
			result.Referrer = result.Referrer?.Truncate(DbConstraints.MaxReferrerLength);
			result.UtmSource = result.UtmSource?.Truncate(DbConstraints.MaxUtmLength);
			result.UtmMedium = result.UtmMedium?.Truncate(DbConstraints.MaxUtmLength);
			result.UtmCampaign = result.UtmCampaign?.Truncate(DbConstraints.MaxUtmLength);

			return result;
		}
		catch
		{
			return null;
		}
	}

	public void SetAuthorised(JwtTokenInfo token)
	{
		foreach (var domain in jwtConfig.CookieDomains)
		{
			HttpContext.Response.Cookies.Append("Authorization", token.Token, GetCookieOptions(token, true, domain));
			HttpContext.Response.Cookies.Append("Is-Authorized", "true", GetCookieOptions(token, false, domain));
		}
	}

	private CookieOptions GetCookieOptions(JwtTokenInfo token, bool httpOnly, string domain)
	{
		var isLocal = IsLocal();

		return new CookieOptions
		{
			Expires = token.ExpiresAt,
			HttpOnly = httpOnly,
			Secure = !isLocal,
			SameSite = SameSiteMode.Lax,
			Domain = domain,
		};
	}

	private bool IsLocal()
	{
		return HttpContext.GetHeader("Is-Local").EqualsIIC("true");
	}

	public string? GetUserAgent()
	{
		var result = HttpContext.GetHeader(DefaultHeaders.UserAgent);
		if (result?.Length > DbConstraints.MaxUserAgentLength)
			result = result[..DbConstraints.MaxUserAgentLength];

		return result;
	}
}