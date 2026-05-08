# Benchmarks

| Scale (Rows) | Disk Load Time | RAM Usage | Linear Search | Binary Search |
| :--- | :--- | :--- | :--- | :--- |
| **10,000** | 7ms | 3MB | 0ms | 0ms |
| **100,000** | 82ms | 24MB | 1ms | 0ms |
| **500,000** | 442ms | 121MB | 6ms | 0ms |


**Note:** The Binary Search performance remains near-instant across all test scales, showcasing the efficiency of $O(\log n)$ algorithmic complexity.