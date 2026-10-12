"""Source-only release allowlist. Run before committing or making a release ZIP."""
from pathlib import Path
import re
import sys
import json
import zipfile
import hashlib

ROOT = Path(__file__).resolve().parents[1]
ROOT_FILES = {'README.md', 'AGENTS.md', 'LICENSE', 'THIRD_PARTY_NOTICES.md', 'package.json', '.gitignore', '.gitattributes', 'CHANGELOG.md'}
DIRS = {'Editor', 'Presets', 'Documentation~', 'Tools~'}
EXTS = {'.cs', '.asmdef', '.json', '.md', '.py', '.meta'}

def audit():
    files = []
    for p in ROOT.rglob('*'):
        if not p.is_file() or '.git' in p.relative_to(ROOT).parts:
            continue
        rel = p.relative_to(ROOT)
        if '__pycache__' in rel.parts:
            continue
        if len(rel.parts) == 1:
            allowed = rel.name in ROOT_FILES or (rel.name.endswith('.meta') and (rel.name[:-5] in ROOT_FILES or rel.name[:-5] in DIRS))
        else:
            allowed = rel.parts[0] in DIRS and p.suffix in EXTS
        if not allowed:
            raise RuntimeError(f'File outside release allowlist: {rel}')
        data = p.read_bytes()
        if len(data) > 180000:
            raise RuntimeError(f'Unexpected large source file: {rel}')
        text = data.decode('utf-8-sig')
        if '\x00' in text:
            raise RuntimeError(f'Binary data: {rel}')
        # Actual local user/project paths, access tokens and Unity asset references do not belong here.
        patterns = [r'[A-Za-z]:[\\/]Users[\\/]', r'[A-Za-z]:[\\/]VRC_Avatar[\\/]',
                    r'gh[pousr]_[A-Za-z0-9]{20,}', r'github_pat_[A-Za-z0-9_]{20,}',
                    r'fileID:\s*\d+,\s*guid:\s*[a-f0-9]{32}']
        if any(re.search(pattern, text) for pattern in patterns):
            raise RuntimeError(f'Private path, token or serialized product reference: {rel}')
        files.append(p)
    manifest = json.loads((ROOT/'package.json').read_text())
    if manifest['license'] != 'UNLICENSED':
        raise RuntimeError('License changed: obtain author decision before release.')
    return sorted(files)

if __name__ == '__main__':
    files = audit()
    print(f'PASS: {len(files)} text/source files, {sum(p.stat().st_size for p in files)} bytes; no product binaries.')
    if len(sys.argv) == 3 and sys.argv[1] == '--zip':
        target = Path(sys.argv[2]).resolve()
        if target == ROOT or ROOT in target.parents:
            raise RuntimeError('Write the archive outside the public source tree.')
        target.parent.mkdir(parents=True, exist_ok=True)
        with zipfile.ZipFile(target, 'w', zipfile.ZIP_DEFLATED) as z:
            for p in files:
                z.write(p, str(p.relative_to(ROOT)).replace('\\', '/'))
        print('SHA256 ' + hashlib.sha256(target.read_bytes()).hexdigest())
