from pathlib import Path
import zipfile

publish_dir = Path("publish")
zip_path = Path("incidentplatform-eb.zip")

if zip_path.exists():
    zip_path.unlink()

with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zf:
    for file_path in publish_dir.rglob("*"):
        if file_path.is_file():
            arcname = file_path.relative_to(publish_dir).as_posix()
            zf.write(file_path, arcname)

print(f"Created {zip_path}")