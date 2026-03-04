namespace Anycode.NetCore.DbTools.Interfaces;

public interface ISecurityUser<T> : IEntity<T> where T : notnull
{
	string? SecurityStamp { get; }
}