from io import BytesIO
from pathlib import Path

from pypdf import PdfReader, PdfWriter
from pypdf.generic import ContentStream, NameObject
from reportlab.pdfgen import canvas


SOURCE = Path(r"C:\Users\inesa\Desktop\Files\Studieintyg Ines Azouz CUA24S.pdf")
OUTPUT = Path(
    r"C:\Users\inesa\source\Uppgift1-Frontend_Ines\Studieintyg Ines Azouz CUA24S - ovningsexempel.pdf"
)


F1_CODES = {
    " ": 0x0003,
    "(": 0x021B,
    ")": 0x021C,
    "+": 0x02AD,
    "-": 0x0212,
    ":": 0x01FF,
    "0": 0x01D5,
    "1": 0x01D6,
    "2": 0x01D7,
    "3": 0x01D8,
    "4": 0x01D9,
    "5": 0x01DA,
    "6": 0x01DB,
    "7": 0x01DC,
    "8": 0x01DD,
    "9": 0x01DE,
}


def encode_f1(text: str) -> str:
    return "".join(chr(F1_CODES[char] >> 8) + chr(F1_CODES[char] & 0xFF) for char in text)


def encode_f1_codepoints(text: str) -> str:
    return "".join(chr(F1_CODES[char]) for char in text)


def replace_text_runs(content: ContentStream, old: str, new: str, count: int = 1) -> int:
    encoded_pairs = [(encode_f1(old), encode_f1(new)), (encode_f1_codepoints(old), encode_f1_codepoints(new))]
    if any(len(old_encoded) != len(new_encoded) for old_encoded, new_encoded in encoded_pairs):
        raise ValueError("Replacement must keep the same encoded length.")

    replaced = 0
    while replaced < count:
        text_ops = [
            index
            for index, (operands, operator) in enumerate(content.operations)
            if operator == b"Tj" and operands and isinstance(operands[0], str)
        ]
        stream_text = "".join(content.operations[index][0][0] for index in text_ops)
        old_encoded, new_encoded, start = next(
            (
                (old_candidate, new_candidate, match_start)
                for old_candidate, new_candidate in encoded_pairs
                for match_start in [stream_text.find(old_candidate)]
                if match_start != -1
            ),
            (None, None, -1),
        )
        if start == -1:
            break

        cursor = 0
        for op_index in text_ops:
            operands = content.operations[op_index][0]
            run = operands[0]
            run_end = cursor + len(run)
            overlap_start = max(start, cursor)
            overlap_end = min(start + len(old_encoded), run_end)
            if overlap_start < overlap_end:
                local_start = overlap_start - cursor
                local_end = overlap_end - cursor
                replacement_start = overlap_start - start
                replacement_end = overlap_end - start
                operands[0] = type(run)(
                    run[:local_start]
                    + new_encoded[replacement_start:replacement_end]
                    + run[local_end:]
                )
            cursor = run_end
        replaced += 1
    return replaced


def make_watermark(width: float, height: float) -> BytesIO:
    packet = BytesIO()
    c = canvas.Canvas(packet, pagesize=(width, height))

    c.saveState()
    c.setFillColorRGB(0.75, 0, 0, alpha=0.18)
    c.translate(width / 2, height / 2)
    c.rotate(36)
    c.setFont("Helvetica-Bold", 34)
    c.drawCentredString(0, 0, "OVNINGSEXEMPEL - EJ GILTIGT DOKUMENT")
    c.restoreState()

    c.setFillColorRGB(0.75, 0, 0)
    c.setFont("Helvetica-Bold", 10)
    c.drawCentredString(width / 2, height - 20, "OVNINGSEXEMPEL - EJ GILTIGT DOKUMENT")
    c.drawCentredString(width / 2, 20, "OVNINGSEXEMPEL - EJ GILTIGT DOKUMENT")

    c.save()
    packet.seek(0)
    return packet


def apply_watermark(page, reader: PdfReader) -> None:
    width = float(page.mediabox.width)
    height = float(page.mediabox.height)
    watermark = PdfReader(make_watermark(width, height)).pages[0]
    page.merge_page(watermark)


def main() -> None:
    reader = PdfReader(str(SOURCE))
    writer = PdfWriter()

    first_page = reader.pages[0]
    first_content = ContentStream(first_page.get_contents(), reader)
    replace_text_runs(first_content, "2025-03-14", "2026-06-15")
    replace_text_runs(first_content, "2026-06-05", "2026-07-31")
    first_page[NameObject("/Contents")] = first_content
    apply_watermark(first_page, reader)
    writer.add_page(first_page)

    second_page = reader.pages[1]
    second_content = ContentStream(second_page.get_contents(), reader)
    replacements = [
        ("2025-03-14", "2026-06-15", 3),
        ("10:45:18", "14:02:33", 1),
        ("10:45:19", "14:02:35", 2),
    ]
    for old, new, count in replacements:
        if replace_text_runs(second_content, old, new, count=count) != count:
            raise ValueError(f"Could not replace all occurrences of {old!r}.")
    second_page[NameObject("/Contents")] = second_content
    apply_watermark(second_page, reader)
    writer.add_page(second_page)

    with OUTPUT.open("wb") as f:
        writer.write(f)

    print(OUTPUT)


if __name__ == "__main__":
    main()
