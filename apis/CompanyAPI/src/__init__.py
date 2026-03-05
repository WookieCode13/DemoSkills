
from pathlib import Path
import sys

# Make shared python security package importable without per-machine PYTHONPATH setup.
for parent in Path(__file__).resolve().parents:
    shared_py = parent / "Shared" / "Security" / "py"
    if shared_py.exists():
        sys.path.insert(0, str(shared_py))
        break
