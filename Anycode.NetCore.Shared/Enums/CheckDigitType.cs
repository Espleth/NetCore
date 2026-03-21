namespace Anycode.NetCore.Shared.Enums;

public enum CheckDigitType
{
	None,

	/// <summary>
	/// Popular algorithm for validating credit card numbers, IMEI numbers, and more
	/// </summary>
	Luhn,

	/// <summary>
	/// Mod 11 with Weights 8 6 4 2 3 5 9 7 - identical to UPU S10
	/// </summary>
	UpuS10,

	/// <summary>
	/// Damm algorithm using a totally anti-symmetric quasigroup of order 10
	/// </summary>
	Damm,
}