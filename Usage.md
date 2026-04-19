# AliceOne Usage Guide

AliceOne is a high-performance CSV editor using one-letter commands.

# Quick Reference Table

| Command | Action & Example |
| :--- | :--- |
| L	| Load a CSV file |	L data.csv |
| W	| Write and Save to file  W backup.csv  | 
| S	| Search or Filter rows	 S Id == 106 | 
| I | Insert a row	I val1,val2 | 
| O | Sort by id  O | 
| A	| Show Status  A | 
| C	| Clear  C | 
| X	| Remove duplicate Ids X | 
| Q	| Quit application  Q | 

## File Operations

### C (Clear): Removes all rows currently in memory.

### L [path] (Load): Imports a CSV file into the editor.

### W [path] (Write): Exports the current data to the specified path.

### Q (Quit): Terminate the session. 

## Viewing & Filtering

The S (Select) command supports three distinct search modes:

### 1. Show All

Example: S

### 2. Filter by Value

Syntax: S [column] [operator] [value]

Example: S Status == Pending (Shows all Pending rows in the Status column)

Supported operators include: ==, !=, <, >, <=, >=

### 3. Dual Filter 

Syntax: S [column] [operator] [value] [column] [operator] [value]

Example: S Age == 20 City == Miami


## Editing Data

### I [v1,v2...] (Insert): Insert a row.

### D [col] == [id] (Delete): Removes a specific row by ID.

### X (Remove Duplicates): Deletes redundant rows based on ID.
