import pypdf

reader = pypdf.PdfReader('UnityProject/SentinelXR/Documentation/AcademicReport/SENTINEL_XR_SLA_STAGE_3_REPORT.pdf')
full_text = "\n".join([p.extract_text() or "" for p in reader.pages])

print(f"Total Pages: {len(reader.pages)}")

terms = ["appendix", "vspace", "newpage", "zombie", "jayesh", "deepak", "$$"]
for t in terms:
    hits = [i + 1 for i, p in enumerate(reader.pages) if t in (p.extract_text() or "").lower()]
    print(f"Occurrences of '{t}': {len(hits)} (Pages: {hits})")

print("\n--- Last Page Content Preview ---")
last_page_text = reader.pages[-1].extract_text() or ""
for line in last_page_text.splitlines()[:15]:
    print(" ", line.strip())
