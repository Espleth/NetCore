namespace Anycode.NetCore.Shared.Enums;

public enum CheckDigitType
{
	None,
	Luhn,

	/// <summary>
	/// Weights 8 6 4 2 3 5 9 7 - identical to UPU S10
	/// </summary>
	Mod11,
}