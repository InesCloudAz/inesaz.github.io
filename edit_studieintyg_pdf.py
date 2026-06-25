from pathlib import Path

from pypdf.generic import ContentStream, NameObject
from pypdf import PdfReader, PdfWriter


SOURCE = Path(r"C:\Users\inesa\Desktop\Files\Studieintyg Ines Azouz CUA24S.pdf")
OUTPUT = Path(r"C:\Users\inesa\source\Uppgift1-Frontend_Ines\Studieintyg Ines Azouz CUA24S - uppdaterad.pdf")


F1_CODES = {
    " ": 0x0003,
    "-": 0x0212,
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


def replace_text_runs(content: ContentStream, old: str, new: str) -> None:
    old_encoded = encode_f1(old)
    new_encoded = encode_f1(new)
    if len(old_encoded) != len(new_encoded):
        raise ValueError("Replacement must keep the same encoded length.")

    text_ops = [
        index
        for index, (operands, operator) in enumerate(content.operations)
        if operator == b"Tj" and operands and isinstance(operands[0], str)
    ]
    stream_text = "".join(content.operations[index][0][0] for index in text_ops)
    start = stream_text.find(old_encoded)
    if start == -1:
        raise ValueError(f"Could not find encoded text for {old!r}.")

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


def main() -> None:
    reader = PdfReader(str(SOURCE))
    writer = PdfWriter()

    first_page = reader.pages[0]
    content = ContentStream(first_page.get_contents(), reader)
    replace_text_runs(content, "2025-03-14", "2026-06-15")
    replace_text_runs(content, "2026-06-05", "2026-07-31")
    first_page[NameObject("/Contents")] = content

    writer.add_page(first_page)
    for page in reader.pages[1:]:
        writer.add_page(page)

    with OUTPUT.open("wb") as f:
        writer.write(f)

    print(OUTPUT)


if __name__ == "__main__":
    main()
