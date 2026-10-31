import os, sys, re
import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_ALIGN_VERTICAL
from docx.oxml import parse_xml, OxmlElement
from docx.oxml.ns import nsdecls, qn

base_dir = r'UnityProject/SentinelXR/Documentation/AcademicReport'
md_path = os.path.join(base_dir, 'SENTINEL_XR_SLA_STAGE_3_REPORT.md')
docx_path = os.path.join(base_dir, 'SENTINEL_XR_SLA_STAGE_3_REPORT.docx')
pdf_path = os.path.join(base_dir, 'SENTINEL_XR_SLA_STAGE_3_REPORT.pdf')
fig_dir = os.path.join(base_dir, 'FIGURES')

print('Starting academic DOCX & PDF generation...')

doc = docx.Document()

# 1. Page Setup - A4 Portrait with Academic Margins (Left 1.25", Right 1.0", Top 1.0", Bottom 1.0")
for section in doc.sections:
    section.page_width = Inches(8.27)  # A4 width
    section.page_height = Inches(11.69) # A4 height
    section.top_margin = Inches(1.0)
    section.bottom_margin = Inches(1.0)
    section.left_margin = Inches(1.25) # Binding margin
    section.right_margin = Inches(1.0)

# 2. Styles Setup
styles = doc.styles

# Normal Body Style
normal_style = styles['Normal']
normal_style.font.name = 'Times New Roman'
normal_style.font.size = Pt(12)
normal_style.font.color.rgb = RGBColor(0x1a, 0x1a, 0x1a)
normal_style.paragraph_format.line_spacing = 1.35
normal_style.paragraph_format.space_after = Pt(6)
normal_style.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY

def set_cell_background(cell, fill_hex):
    tcPr = cell._tc.get_or_add_tcPr()
    tcPr.append(parse_xml(f'<w:shd {nsdecls("w")} w:fill="{fill_hex}"/>'))

def set_cell_margins(cell, top=100, bottom=100, left=150, right=150):
    tcPr = cell._tc.get_or_add_tcPr()
    tcMar = parse_xml(f'<w:tcMar {nsdecls("w")}><w:top w:w="{top}" w:type="dxa"/><w:bottom w:w="{bottom}" w:type="dxa"/><w:left w:w="{left}" w:type="dxa"/><w:right w:w="{right}" w:type="dxa"/></w:tcMar>')
    tcPr.append(tcMar)

def add_heading_1(text):
    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(16)
    p.paragraph_format.space_after = Pt(8)
    p.paragraph_format.keep_with_next = True
    p.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.LEFT
    run = p.add_run(text)
    run.font.name = 'Times New Roman'
    run.font.size = Pt(16)
    run.font.bold = True
    run.font.color.rgb = RGBColor(0x0f, 0x17, 0x2a) # Dark Navy
    return p

def add_heading_2(text):
    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(12)
    p.paragraph_format.space_after = Pt(6)
    p.paragraph_format.keep_with_next = True
    p.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.LEFT
    run = p.add_run(text)
    run.font.name = 'Times New Roman'
    run.font.size = Pt(13.5)
    run.font.bold = True
    run.font.color.rgb = RGBColor(0x1e, 0x29, 0x3b)
    return p

def add_heading_3(text):
    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(8)
    p.paragraph_format.space_after = Pt(4)
    p.paragraph_format.keep_with_next = True
    p.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.LEFT
    run = p.add_run(text)
    run.font.name = 'Times New Roman'
    run.font.size = Pt(12)
    run.font.bold = True
    run.font.italic = True
    run.font.color.rgb = RGBColor(0x33, 0x41, 0x55)
    return p

def add_callout_box(text, border_hex="3b82f6", bg_hex="f8fafc"):
    table = doc.add_table(rows=1, cols=1)
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    table.autofit = False
    table.columns[0].width = Inches(5.8)
    cell = table.cell(0, 0)
    set_cell_background(cell, bg_hex)
    set_cell_margins(cell, top=140, bottom=140, left=200, right=200)
    
    # Left border only or full border
    tcPr = cell._tc.get_or_add_tcPr()
    borders = parse_xml(f'<w:tcBorders {nsdecls("w")}><w:top w:val="none"/><w:left w:val="single" w:sz="24" w:space="0" w:color="{border_hex}"/><w:bottom w:val="none"/><w:right w:val="none"/></w:tcBorders>')
    tcPr.append(borders)
    
    p = cell.paragraphs[0]
    p.paragraph_format.line_spacing = 1.15
    p.paragraph_format.space_after = Pt(0)
    run = p.add_run(text)
    run.font.name = 'Consolas'
    run.font.size = Pt(9.5)
    run.font.color.rgb = RGBColor(0x0f, 0x17, 0x2a)
    doc.add_paragraph() # space after

def add_figure_image(fig_name, caption_text):
    fig_path = os.path.join(fig_dir, fig_name)
    if os.path.exists(fig_path):
        p_img = doc.add_paragraph()
        p_img.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_img.paragraph_format.space_before = Pt(8)
        p_img.paragraph_format.space_after = Pt(4)
        run_img = p_img.add_run()
        # Scale image to max width 5.5 inches
        run_img.add_picture(fig_path, width=Inches(5.5))
        
        # Caption
        p_cap = doc.add_paragraph()
        p_cap.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_cap.paragraph_format.space_before = Pt(2)
        p_cap.paragraph_format.space_after = Pt(10)
        p_cap.paragraph_format.keep_with_next = False
        run_cap = p_cap.add_run(caption_text)
        run_cap.font.name = 'Times New Roman'
        run_cap.font.size = Pt(10)
        run_cap.font.bold = True
        run_cap.font.italic = True
        run_cap.font.color.rgb = RGBColor(0x33, 0x41, 0x55)
    else:
        print(f"Warning: Figure not found: {fig_path}")

print("Parsing markdown content...")
with open(md_path, 'r', encoding='utf-8') as f:
    lines = f.readlines()

in_code_block = False
code_lines = []
in_table = False
table_lines = []

fig_map = {
    'FIGURE 1.1': ('fig01_project_overview.png', 'Figure 1.1: SENTINEL-XR 2.0 Web Operations Dashboard & 3D Spatial Topology Hub'),
    'FIGURE 6.1': ('fig02_unity_hierarchy_scenes.png', 'Figure 6.1: SENTINEL-XR Multi-Scene Architecture & Execution Pipeline'),
    'FIGURE 6.2': ('fig03_bootstrap_lifecycle.png', 'Figure 6.2: Bootstrap Lifecycle & Persistent Singleton Hierarchy'),
    'FIGURE 6.3': ('fig04_main_menu_interface.png', 'Figure 6.3: Main Menu Architecture & Operational Routing'),
    'FIGURE 6.4': ('fig05_mission01_arena_layout.png', 'Figure 6.4: Mission 01: 44m x 44m Cyber Arena Spatial Blueprint'),
    'FIGURE 7.1': ('fig17_project_folder_structure.png', 'Figure 7.1: Unity Project Structure & Assets Organization Tree'),
    'FIGURE 7.2': ('fig07_sentinel_player_inspector.png', 'Figure 7.2: SentinelPlayerController Architectural Decomposition'),
    'FIGURE 7.3': ('fig08_defensive_tools_system.png', 'Figure 7.3: Defensive Cyber Tool Arsenal & Mechanics Contract'),
    'FIGURE 7.4': ('fig09_enemy_threat_entities.png', 'Figure 7.4: Synthetic Hostile Threat Entities & Attack Profiles'),
    'FIGURE 7.5': ('fig10_dmz_gateway_node.png', 'Figure 7.5: DMZ Gateway Node Corruption & Interactive Repair Flow'),
    'FIGURE 7.6': ('fig11_breach_core_alpha.png', 'Figure 7.6: Breach Core Alpha: State Transitions & Barrier Mechanics'),
    'FIGURE 7.7': ('fig12_mission_hud_glass_zones.png', 'Figure 7.7: In-Game Obsidian Glass HUD: 6-Zone Telemetry Architecture'),
    'FIGURE 7.8': ('fig18_xr_operations_center.png', 'Figure 7.8: Extended Reality (XR) Operations Center & WebSockets Synchronization'),
    'FIGURE 9.1': ('fig13_missing_component_audit.png', 'Figure 9.1: Missing Component Forensic Audit Verification Results'),
    'FIGURE 9.2': ('fig14_test_runner_editmode.png', 'Figure 9.2: Unity Test Runner EditMode Results (15/15 PASS - 100%)'),
    'FIGURE 9.3': ('fig15_test_runner_playmode.png', 'Figure 9.3: Unity Test Runner PlayMode Results (4/4 PASS - 100%)'),
    'FIGURE A.1': ('fig17_project_folder_structure.png', 'Figure A.1: Complete Project Physical Directory Organization')
}

def render_table(rows_data):
    if not rows_data:
        return
    # Parse markdown table rows
    parsed_rows = []
    for r in rows_data:
        parts = [c.strip() for c in r.split('|')]
        # Drop empty first and last parts from markdown table syntax | a | b |
        if len(parts) >= 3 and parts[0] == '' and parts[-1] == '':
            parts = parts[1:-1]
        elif len(parts) >= 2:
            if parts[0] == '': parts = parts[1:]
            if parts[-1] == '': parts = parts[:-1]
        
        # Check if separator row
        if all(re.match(r'^:?-+:?$', c) for c in parts):
            continue
        parsed_rows.append(parts)
        
    if not parsed_rows:
        return
        
    num_cols = max(len(r) for r in parsed_rows)
    # Normalize row lengths
    for r in parsed_rows:
        while len(r) < num_cols:
            r.append('')
            
    tbl = doc.add_table(rows=len(parsed_rows), cols=num_cols)
    tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    tbl.autofit = True
    
    # Style Table
    for r_idx, row in enumerate(parsed_rows):
        is_header = (r_idx == 0)
        for c_idx, cell_text in enumerate(row):
            cell = tbl.cell(r_idx, c_idx)
            set_cell_margins(cell, top=80, bottom=80, left=100, right=100)
            
            p = cell.paragraphs[0]
            p.paragraph_format.line_spacing = 1.15
            p.paragraph_format.space_after = Pt(2)
            p.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.LEFT
            
            if is_header:
                set_cell_background(cell, "0f172a") # Dark navy header
                run = p.add_run(cell_text)
                run.font.name = 'Times New Roman'
                run.font.size = Pt(10)
                run.font.bold = True
                run.font.color.rgb = RGBColor(0xff, 0xff, 0xff)
            else:
                bg = "f8fafc" if r_idx % 2 == 1 else "ffffff"
                set_cell_background(cell, bg)
                run = p.add_run(cell_text)
                run.font.name = 'Times New Roman'
                run.font.size = Pt(9.5)
                run.font.color.rgb = RGBColor(0x1e, 0x29, 0x3b)
                
    doc.add_paragraph() # space after table

i = 0
is_cover_page = True

while i < len(lines):
    line = lines[i].rstrip('\r\n')
    
    # 1. Page breaks
    if line.strip() == '\\newpage':
        doc.add_page_break()
        is_cover_page = False
        i += 1
        continue
        
    # 2. Code blocks
    if line.startswith('```'):
        if in_code_block:
            # End code block
            code_text = '\n'.join(code_lines)
            
            # Check if this code block represents a figure box!
            fig_matched = False
            for fig_key, (fig_file, fig_cap) in fig_map.items():
                if fig_key in code_text:
                    add_figure_image(fig_file, fig_cap)
                    fig_matched = True
                    break
            if not fig_matched:
                if is_cover_page:
                    p = doc.add_paragraph()
                    p.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.CENTER
                    p.paragraph_format.space_before = Pt(2)
                    p.paragraph_format.space_after = Pt(2)
                    p.paragraph_format.line_spacing = 1.0
                    r = p.add_run(code_text)
                    r.font.name = 'Consolas'
                    r.font.size = Pt(8.5)
                    r.font.bold = True
                    r.font.color.rgb = RGBColor(0x0f, 0x17, 0x2a)
                else:
                    add_callout_box(code_text, border_hex="475569", bg_hex="f1f5f9")
                
            code_lines = []
            in_code_block = False
        else:
            in_code_block = True
            code_lines = []
        i += 1
        continue
        
    if in_code_block:
        code_lines.append(line)
        i += 1
        continue
        
    # 3. Tables
    if line.strip().startswith('|') and '|' in line.strip()[1:]:
        table_lines.append(line.strip())
        in_table = True
        i += 1
        continue
    else:
        if in_table:
            render_table(table_lines)
            table_lines = []
            in_table = False
            
    # 4. Horizontal Rule
    if line.strip() in ['---', '***', '___']:
        # subtle divider
        i += 1
        continue
        
    # 5. Headings
    if line.startswith('# '):
        p = add_heading_1(line[2:].strip())
        if is_cover_page:
            p.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.CENTER
            p.paragraph_format.space_before = Pt(4)
            p.paragraph_format.space_after = Pt(2)
        i += 1
        continue
    elif line.startswith('## '):
        p = add_heading_2(line[3:].strip())
        if is_cover_page:
            p.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.CENTER
            p.paragraph_format.space_before = Pt(2)
            p.paragraph_format.space_after = Pt(2)
            p.runs[0].font.size = Pt(11)
        i += 1
        continue
    elif line.startswith('### '):
        p = add_heading_3(line[4:].strip())
        if is_cover_page:
            p.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.CENTER
            p.paragraph_format.space_before = Pt(2)
            p.paragraph_format.space_after = Pt(2)
            p.runs[0].font.size = Pt(10)
        i += 1
        continue
        
    # 6. Regular Paragraphs
    if line.strip():
        p = doc.add_paragraph()
        raw = line.strip()
        
        # Check if centered title text or cover page
        if is_cover_page:
            p.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.CENTER
            p.paragraph_format.space_before = Pt(1)
            p.paragraph_format.space_after = Pt(1)
            p.paragraph_format.line_spacing = 1.05
        elif raw.startswith('**Date:') or raw.startswith('**Place:'):
            p.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.LEFT
            
        parts = re.split(r'(\*\*.*?\*\*|\*.*?\*|\[.*?\]\(.*?\))', raw)
        for part in parts:
            if not part:
                continue
            if part.startswith('**') and part.endswith('**'):
                r = p.add_run(part[2:-2])
                r.bold = True
                if is_cover_page:
                    r.font.size = Pt(10)
            elif part.startswith('*') and part.endswith('*'):
                r = p.add_run(part[1:-1])
                r.italic = True
                if is_cover_page:
                    r.font.size = Pt(9.5)
            elif part.startswith('[') and '](' in part and part.endswith(')'):
                link_match = re.match(r'\[(.*?)\]\((.*?)\)', part)
                if link_match:
                    r = p.add_run(link_match.group(1))
                    r.font.color.rgb = RGBColor(0x02, 0x84, 0xc7)
                    r.underline = True
                    if is_cover_page:
                        r.font.size = Pt(10)
                else:
                    p.add_run(part)
            else:
                r = p.add_run(part)
                if is_cover_page:
                    r.font.size = Pt(9.5)
    
    i += 1

if in_table:
    render_table(table_lines)

print(f"Saving DOCX to: {docx_path}...")
doc.save(docx_path)
print("DOCX successfully generated!")

# --- PDF Export via Word COM ---
print(f"Exporting PDF to: {pdf_path} via Microsoft Word COM...")
try:
    import win32com.client
    import pythoncom
    
    pythoncom.CoInitialize()
    word = win32com.client.DispatchEx("Word.Application")
    word.Visible = False
    word.DisplayAlerts = 0
    
    doc_abs = os.path.abspath(docx_path)
    pdf_abs = os.path.abspath(pdf_path)
    
    wdoc = word.Documents.Open(doc_abs, ReadOnly=True)
    # 17 = wdExportFormatPDF
    wdoc.ExportAsFixedFormat(pdf_abs, 17, OpenAfterExport=False, OptimizeFor=0)
    wdoc.Close(False)
    word.Quit()
    pythoncom.CoUninitialize()
    print("PDF successfully exported with 100% Word engine fidelity!")
except Exception as e:
    print(f"Word COM PDF export note: {e}")
    print("Checking if reportlab or alternative PDF export is needed...")
