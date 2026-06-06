namespace Anycode.NetCore.Shared.Helpers;

public static class VaultHelper
{
	// Loads secrets from HashiCorp Vault (KV v2) into configuration. The secret is stored as a single nested
	// JSON document whose top-level keys mirror configuration sections (e.g. ConnectionStrings, JwtConfig), so
	// it binds through the same IConfiguration keys as the JSON files - application code stays unchanged.
	// Read once at startup; restart the app to pick up rotated secrets. Fails loud if Vault cannot be read.
	//
	// Environment variables:
	//   VAULT_ADDR                     Vault base address (default http://127.0.0.1:8200)
	//   VAULT_MOUNT                    KV v2 mount point (default "secret")
	//   VAULT_SECRET_PATH              secret path under the mount, e.g. "appName/demo" (required)
	//   VAULT_TOKEN / VAULT_TOKEN_FILE token value, or a file to read it from (file default /run/secrets/vault-token)
	//   VAULT_STARTUP_TIMEOUT_SECONDS  how long to wait for Vault/secret on boot (default 0 = fail fast)
	public static void AddVaultSecrets(this IConfigurationBuilder configuration, NLogLogger log)
	{
		var address = (Environment.GetEnvironmentVariable("VAULT_ADDR") ?? "http://127.0.0.1:8200").TrimEnd('/');
		var mount = Environment.GetEnvironmentVariable("VAULT_MOUNT") ?? "secret";
		var path = Environment.GetEnvironmentVariable("VAULT_SECRET_PATH")
		           ?? throw new InvalidOperationException("VAULT_SECRET_PATH is required when SECRETS_PROVIDER=vault");
		var token = ResolveVaultToken();
		var timeout = TimeSpan.FromSeconds(
			int.TryParse(Environment.GetEnvironmentVariable("VAULT_STARTUP_TIMEOUT_SECONDS"), out var seconds) ? seconds : 0);

		// KV v2 read endpoint: /v1/{mount}/data/{path}
		var requestUri = $"{address}/v1/{mount}/data/{path}";

		using var http = new HttpClient();
		http.Timeout = TimeSpan.FromSeconds(10);
		http.DefaultRequestHeaders.Add("X-Vault-Token", token);

		var deadline = DateTimeOffset.UtcNow + timeout;
		while (true)
		{
			try
			{
				using var response = http.GetAsync(requestUri).GetAwaiter().GetResult();
				if (response.IsSuccessStatusCode)
				{
					var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					using var doc = JsonDocument.Parse(body);
					// KV v2 wraps the payload twice: { "data": { "data": { ...sections... }, "metadata": {...} } }
					var secrets = doc.RootElement.GetProperty("data").GetProperty("data");
					configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(secrets.GetRawText())));
					log.Info("Secrets loaded from Vault: {Mount}/{Path}", mount, path);
					return;
				}

				if (DateTimeOffset.UtcNow >= deadline)
					throw new InvalidOperationException(
						$"Vault returned {(int)response.StatusCode} for '{mount}/{path}'. Secrets were not loaded.");
			}
			catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
			{
				if (DateTimeOffset.UtcNow >= deadline)
					throw new InvalidOperationException($"Vault is unreachable at {address}", e);
			}

			log.Warn("Waiting for Vault secret '{Mount}/{Path}'...", mount, path);
			Thread.Sleep(TimeSpan.FromSeconds(1));
		}
	}

	private static string ResolveVaultToken()
	{
		var token = Environment.GetEnvironmentVariable("VAULT_TOKEN");
		if (!string.IsNullOrWhiteSpace(token))
			return token;

		var tokenFile = Environment.GetEnvironmentVariable("VAULT_TOKEN_FILE") ?? "/run/secrets/vault-token";
		if (File.Exists(tokenFile))
			return File.ReadAllText(tokenFile).Trim();

		throw new InvalidOperationException(
			"Vault token not configured: set VAULT_TOKEN or provide VAULT_TOKEN_FILE (default /run/secrets/vault-token).");
	}
}