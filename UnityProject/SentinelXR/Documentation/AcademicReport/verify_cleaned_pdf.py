import pypdf
import re
import os
import sys

if sys.stdout and hasattr(sys.stdout, 'reconfigure'):
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')

pdf_path = r"UnityProject/SentinelXR/Documentation/AcademicReport/SENTINEL_XR_SLA_STAGE_3_REPORT.pdf"
if not os.path.exists(pdf_path):
    print("PDF not found:", pdf_path)
    exit(1)

reader = pypdf.PdfReader(pdf_path)
total_pages = len(reader.pages)
print(f"Total pages in PDF: {total_pages}")

full_text = ""
for i, page in enumerate(reader.pages):
    txt = page.extract_text() or ""
    full_text += f"\n--- PAGE {i+1} ---\n" + txt

vspace_matches = re.findall(r"\\?vspace", full_text, re.IGNORECASE)
newpage_matches = re.findall(r"\\?newpage", full_text, re.IGNORECASE)
dollar_matches = re.findall(r"\$[^\$\n]+\$", full_text)
sarika_matches = re.findall(r"SARIKA RANE", full_text, re.IGNORECASE)

print(f"Matches for vspace: {len(vspace_matches)}")
if vspace_matches:
    print("Sample vspace:", vspace_matches[:5])
print(f"Matches for newpage: {len(newpage_matches)}")
print(f"Matches for dollar math: {len(dollar_matches)}")
if dollar_matches:
    print("Sample dollar math:", dollar_matches[:5])
print(f"Matches for SARIKA RANE: {len(sarika_matches)}")

# Also check for any common uncleaned latex tags:
latex_tags = re.findall(r"\\[a-zA-Z]+(?:\{[^}]*\})?", full_text)
filtered_tags = [t for t in latex_tags if any(k in t for k in ['vspace', 'newpage', 'text', 'textbf', 'textit', 'quad', 'frac', 'cdot'])]
print(f"Matches for residual LaTeX tags: {len(filtered_tags)}")
if filtered_tags:
    print("Residual LaTeX tags:", set(filtered_tags))
