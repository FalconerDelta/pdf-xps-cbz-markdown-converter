# DocToMarkdown (DocToMd)

A lightweight, high-performance Windows Forms utility written in C# to convert **PDF**, **XPS**, and **CBZ** documents into clean, structured Markdown files with embedded image and diagram extraction.

Powered by **MuPDF.NET**, this application runs locally, handles DPI-scaling gracefully, and supports drag-and-drop file operations.

---

## Features

* **Multi-Format Support:** Convert PDF, XPS, and CBZ archives seamlessly.
* **Granular Extraction Levels:**
  * **Minimal:** Strips down to raw body text paragraphs without structural markup.
  * **Default:** Preserves document hierarchy, page breaks, and standard text flow.
  * **Full:** Extracts both text and embedded figures/diagrams simultaneously.
* **Smart Image Heuristics:** Automatically filters out tiny styling artifacts, icons, and extreme strip/divider lines using customizable minimum dimension thresholds and aspect-ratio checking.
* **Clean Folder Layout:** Automatically generates a dedicated self-contained folder for each converted document, keeping `.md` files and extracted PNG assets side-by-side with short relative paths (`![Diagram 1](img_1.png)`).
* **Drag-and-Drop Ready:** Drag files directly onto the UI or launch them straight from the Windows shell via command-line arguments.
* **Zero Modal Pop-ups:** Clean inline status bar feedback right in the main window.

---

## Tech Stack

* **Language:** C# 12 / .NET 8+
* **UI Framework:** Windows Forms (WinForms)
* **Core Engine:** [MuPDF.NET](https://www.nuget.org/packages/MuPDF.NET/)

---

## Building & Publishing

To build a streamlined, single-file executable that relies on a shared .NET runtime, run the following command in your terminal:

```bash
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
```

The resulting compiled single-file `.exe` will be located under `bin\Release\net8.0\win-x64\publish\`.

---

## Usage

1. Launch **DocToMd**.
2. **Select or Drag & Drop** a supported document (`.pdf`, `.xps`, or `.cbz`) into the window.
3. Choose your desired **Detail Level** (Minimal, Default, or Full).
4. (Optional) Adjust the **Min Image Size Filter** pixel threshold to fine-tune diagram extraction.
5. Click **Convert to Markdown**. A dedicated folder containing your `.md` file and clean image assets will be created automatically.

---

## Acknowledgments

* Coded and engineered in collaboration with **Google Gemini** (October 2026).

---

## License

Distributed under the MIT License. See `LICENSE` for more information.
