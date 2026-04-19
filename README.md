# AliceOne

**AliceOne** AliceOne is a high-performance, command-line CSV editor designed for rapid data manipulation. With a syntax-driven interface, it allows for fast loading, querying, and updating of CSV records without the overhead of a heavy GUI.

---

## Key Features

* **Fast Data Selection:** Powerful selection syntax for exact matches, logical comparisons, or range-based queries.
* **In-Memory Manipulation:** Sort, update, and delete rows instantly.
* **Clean CLI Interface:** Designed for power users and terminal enthusiasts who prefer keyboard-driven workflows.

---

## Command Reference

AliceOne uses intuitive single-letter commands to manage your data.

| Command | Action | Syntax / Example |
| :--- | :--- | :--- |
| **L** | **Load** | `L file.csv` |
| **W** | **Write** | `W save.csv` |
| **C** | **Clear** | `C` |
| **Q** | **Quit** | `Q` |

## Selection & Filtering (S)
The S command is used to search for data:

By Comparison: S Age == 24 

Operators:  <, >, ==, !=, <=, >=

| Command | Action | Syntax / Example |
| :--- | :--- | :--- |
| **S** | **Select Rows** | `S` |
| **S** | **Select by Value** | `S [col] [op] [v]` |
| **S** | **Select by Value** | `S [col] [op] [v] [col] [o] [v] ` |


## Data Modification

| Command | Action | Syntax / Example |
| :--- | :--- | :--- |
| **I** | **Insert** | `I value1,value2,value3` |
| **D** | **Delete** | `D [col] == [id]` |
| **X** | **Delete Duplicates** | `X` |

## Sorting and Metadata

| Command | Action | Syntax / Example |
| :--- | :--- | :--- |
| **O** | **Order** | `O` (Sort by id) |
| **A** | **Status** | `A` (Show status) |

---

## Usage Tips
* Order Matters: When using the Update (U) command, ensure the Column and Row ID are accurate to avoid data corruption.
* Memory Management: Use the Clear (C) command before loading a new large dataset to ensure optimal performance.
