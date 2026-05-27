from pathlib import Path
import re
import sys

attr_re = re.compile(r'\[(Range|Length)\s*\(')
prop_re = re.compile(
    r'\b(?:public|private|protected|internal)\s+'
    r'(?:override\s+|virtual\s+|required\s+|static\s+|readonly\s+)*?'
    r'(?P<type>[\w<>\[\]?.,]+)\s+(?P<name>\w+)\s*(?:\{|=|;|\))'
)
param_re = re.compile(
    r'\[(?P<attr>Range|Length)\s*\([^\]]+\)\]\s*'
    r'(?P<type>[\w<>\[\]?.,]+)\s+(?P<name>\w+)'
)


def classify_type(t):
    base = t.replace('?', '').strip()
    if base in {'string', 'char'}:
        return 'string'
    if base in {
        'int', 'long', 'short', 'byte', 'uint', 'ulong', 'ushort',
        'decimal', 'double', 'float', 'DateTime', 'DateTimeOffset', 'DateOnly', 'TimeOnly'
    }:
        return 'scalar'
    if base.endswith('[]') or base.startswith((
        'List<', 'IReadOnlyList<', 'IReadOnlyCollection<', 'IEnumerable<', 'HashSet<', 'Dictionary<'
    )):
        return 'collection'
    return 'other'


issues = []
unknown = []
count = 0
for path in Path('.').rglob('*.cs'):
    if any(part in {'bin', 'obj'} for part in path.parts):
        continue
    lines = path.read_text(errors='ignore').splitlines()
    for i, line in enumerate(lines):
        m = attr_re.search(line)
        if not m:
            continue
        attr = m.group(1)
        decl = None
        pm = param_re.search(line)
        if pm:
            decl = (pm.group('type'), pm.group('name'))
        else:
            for j in range(i + 1, min(i + 10, len(lines))):
                s = lines[j].strip()
                if not s or s.startswith('[') or s.startswith('///'):
                    continue
                dm = prop_re.search(s)
                if dm:
                    decl = (dm.group('type'), dm.group('name'))
                break
        count += 1
        if decl is None:
            unknown.append((str(path), i + 1, attr))
            continue
        typ, name = decl
        kind = classify_type(typ)
        if attr == 'Range' and kind in {'string', 'collection'}:
            issues.append((str(path), i + 1, attr, typ, name, 'Range on non-scalar'))
        if attr == 'Length' and kind == 'scalar':
            issues.append((str(path), i + 1, attr, typ, name, 'Length on scalar'))

print(f'Checked uses: {count}')
print(f'Potential issues: {len(issues)}')
for p, l, a, t, n, msg in issues:
    print(f'{p}:{l}: [{a}] {t} {n} -- {msg}')
print(f'Unknown declarations: {len(unknown)}')
for p, l, a in unknown:
    print(f'{p}:{l}: [{a}] <unknown>')

if issues or unknown:
    sys.exit(1)

