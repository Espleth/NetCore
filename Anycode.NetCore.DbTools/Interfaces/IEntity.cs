namespace Anycode.NetCore.DbTools.Interfaces;

public interface IEntity<out TKey>
{
	TKey Id { get; }
}