from pathlib import Path
import sys

# Make shared python security package importable without per-machine PYTHONPATH setup.
_repo_root = Path(__file__).resolve().parents[3]
_shared_py = _repo_root / "Shared" / "Security" / "py"
if _shared_py.exists():
    sys.path.insert(0, str(_shared_py))
