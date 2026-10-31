import pypdf

reader = pypdf.PdfReader('UnityProject/SentinelXR/Documentation/AcademicReport/SENTINEL_XR_SLA_STAGE_3_REPORT.pdf')
full_text = "\n".join([p.extract_text() or "" for p in reader.pages])

print(f"Total Pages: {len(reader.pages)}")

tables = ["Table 2.1", "Table 4.1", "Table 4.2", "Table 6.1", "Table 7.1", "Table 8.1", "Table 9.1", "Table 11.1"]
print("--- Tables Check ---")
for t in tables:
    status = "FOUND" if t in full_text else "MISSING"
    print(f"{t}: {status}")

figures = ["Figure 1.1", "Figure 5.1", "Figure 6.1", "Figure 7.1", "Figure 7.2", "Figure 7.3", "Figure 8.1", "Figure 8.2", "Figure 9.1", "Figure 9.2", "Figure 12.1", "Figure A.1"]
print("--- Figures Check ---")
for f in figures:
    status = "FOUND" if f in full_text else "MISSING"
    print(f"{f}: {status}")
