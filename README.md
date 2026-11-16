# 🧪 MoleculeLab

An interactive, cross-platform desktop application built with **C#** and **Avalonia UI** for constructing, visualizing, and editing 2D chemical structures and molecular bonds.

---

## 🎮 Controls & How to Use

To ensure a smooth and predictable workflow without accidental drag-and-drop issues, **MoleculeLab** uses a **click-based interaction system**:

### 1. Placing Atoms
1. Open the **Periodic Table** panel using the top toggle button.
2. **Left-Click** on an element (e.g., `C`, `H`, `O`, `N`) to select it (the button will highlight in **light blue**).
3. **Left-Click** anywhere on an empty spot on the canvas to place the atom. 
   * *Note:* The element remains selected so you can place multiple atoms in a row.
   * *Collision Protection:* Atoms cannot be placed overlapping existing atoms.
4. **Deselect:** Left-click the highlighted element in the Periodic Table again (or switch tools) to deselect it.

### 2. Moving Existing Atoms (`Select` Tool)
1. Make sure the **Select** tool is active and no Periodic Table element is currently highlighted.
2. **Left-Click** on an existing atom on the canvas to select it (it will turn **light blue**).
3. **Left-Click** on a free space on the canvas to move the atom there.
   * *Note:* Moving an atom automatically clears its old bond lines so you can cleanly reconnect it in its new position.
4. **Deselect:** Left-click the selected atom again to cancel the move.

### 3. Creating Bonds (`Single`, `Double`, `Triple Bond` Tools)
1. Select **Single Bond**, **Double Bond**, or **Triple Bond** from the left `TOOLS` sidebar.
2. **Left-Click** on the **first atom** (it will highlight in **light blue**).
3. Hold down **`Shift`** on your keyboard and **Left-Click** on the **second atom** to draw the bond between them.
4. **Deselect:** Left-click the first atom again without holding `Shift` to cancel the bond selection.

### 4. Deleting Atoms & Bonds (`Delete` Tool)
1. Select the **Delete** tool from the left `TOOLS` sidebar.
2. **Left-Click** on any atom on the canvas to remove it along with all bonds attached to it.

---

## 🛠️ Tech Stack
* **Framework:** .NET / Avalonia UI
* **Language:** C#
* **Supported Platforms:** Windows (`win-x64`), macOS (`osx-x64`, `osx-arm64`), Linux (`linux-x64`)

---

## 🚀 Building Locally

Make sure you have the [.NET SDK](https://dotnet.microsoft.com/download) installed, then run:

```bash
dotnet restore
dotnet build --configuration Release
dotnet run
```