import pypdf

reader = pypdf.PdfReader('UnityProject/SentinelXR/Documentation/AcademicReport/SENTINEL_XR_SLA_STAGE_3_REPORT.pdf')
print(f"Total Pages: {len(reader.pages)}")

# Extract fonts from PDF
fonts = set()
for page in reader.pages:
    if '/Resources' in page and '/Font' in page['/Resources']:
        f_dict = page['/Resources']['/Font']
        for f in f_dict.values():
            if hasattr(f, 'get') and '/BaseFont' in f:
                fonts.add(str(f['/BaseFont']))

print("Embedded Fonts in PDF:")
for font in sorted(fonts):
    print(" ", font)

# Clean audit check
terms = ["appendix", "vspace", "newpage", "zombie", "jayesh", "deepak", "$$"]
for t in terms:
    hits = [i + 1 for i, p in enumerate(reader.pages) if t in (p.extract_text() or "").lower()]
    print(f"Occurrences of '{t}': {len(hits)} (Pages: {hits})")
