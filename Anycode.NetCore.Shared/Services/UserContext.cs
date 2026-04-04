namespace Anycode.NetCore.Shared.Services;

public abstract class BaseUserContext<TId>(IHttpContextAccessor httpContextAccessor)
	where TId : struct, IEquatable<TId>
{
	private CurrentUser? _currentUser;
	private HttpContext HttpContext => httpContextAccessor.HttpContext ?? throw new Exception("HttpContext is null");

	public TId? UserId => User.Id;

	public bool IsAuthorized => User.IsAuthorized;
	public TId UserIdAuthorized => User.AuthorizedId;

	private CurrentUser User
	{
		get
		{
			if (_currentUser != null)
				return _currentUser;

			if (!HttpContext.User.Identity?.IsAuthenticated ?? false)
				return SetAppUser(CurrentUser.UnauthorizedUser);

			if (HttpContext.User.TryGetClaimId<TId>(out var id))
				return SetAppUser(new CurrentUser(id));

			return SetAppUser(CurrentUser.UnauthorizedUser);
		}
	}

	private CurrentUser SetAppUser(CurrentUser current)
	{
		_currentUser = current;
		return current;
	}

	private class CurrentUser(TId? id)
	{
		public static CurrentUser UnauthorizedUser => new(null);

		public TId? Id { get; } = id;
		public bool IsAuthorized => Id != null;

		public TId AuthorizedId => Id ?? throw new UnauthorizedApiException();
	}
}