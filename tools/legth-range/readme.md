# Length/Range attribute check

This small repository-local tool checks C# DataAnnotations usage for a common mistake:

- `[Range]` on `string` or collection-like types.
- `[Length]` on scalar types such as `int`, `decimal`, `DateTime`, `DateOnly`, etc.

The script scans all `*.cs` files under the current working directory and skips `bin/` and `obj/` folders.

## Usage

```bash
cd Anycode.NetCore
python3 tools/legth-range/check.py
```

## Exit code

- `0` — no suspicious usages found.
- `1` — suspicious usages or unrecognized declarations found.

Unknown declarations are treated as failures because they may hide a real validation-attribute mismatch.

