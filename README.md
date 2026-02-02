# High-Performance UI Virtualization Study (Unity/C#)

This project is a technical exploration of UI virtualization logic, designed to handle massive datasets—ranging from 10,000 to 1,000,000 items—within a constrained memory budget. By decoupling the data model from the visual representation, the system ensures that application performance remains stable regardless of the total element count.

[Playable Demo (itch.io)](https://snaf-og.itch.io/virtualized-list)

## Technical Overview

The core challenge addressed in this study is the "O(n) UI Problem," where traditional layout groups crash or stutter when populating thousands of elements. This solution implements a virtualization algorithm that dynamically calculates the indices of visible elements based on scroll position, ensuring the number of active GameObjects remains constant.

### Key Features

* **Constant Memory Footprint:** The system only instantiates the number of UI elements that can fit on the user's screen (plus a small buffer), regardless of whether the underlying data list contains 10 or 1,000,000 records.
* **Dynamic View Calculation:** The system automatically calculates the "Fit Amount" by checking screen resolution and prefab dimensions at runtime, adjusting the virtualization range accordingly.
* **Data-View Separation:** Utilizes a lightweight `DataElement` struct for the data layer. This minimizes memory overhead by avoiding the weight of a full MonoBehaviour for every non-visible item.
* **Inverted Scroll Logic:** Implements a normalized scrollbar mapping to accurately project data indices onto the scrollable content area.

### Optimization Logic

The virtualization algorithm operates in three main stages:
1. **Model Preparation:** A data-only list is generated. At 1,000,000 items, this consumes minimal RAM compared to visual objects.
2. **Bounds Calculation:** The system determines the "Middle Element" based on the scrollbar's normalized value (0.0 to 1.0).
3. **Dynamic Culling/Instantiation:** Elements outside of the calculated "Fit Range" are destroyed, and new elements entering the range are instantiated at precise coordinate offsets within the scroll content.

### Technical Note on Object Pooling
In this specific study, the focus was on the index-mapping algorithm and virtualization logic. In a full production environment, this system would be combined with an Object Pooling pattern to eliminate the CPU overhead of Instantiate and Destroy calls, further refining the frame budget.

## Technical Specifications
* **Engine:** Unity
* **Language:** C#
* **Performance Metric:** Verified stability at 1,000,000+ data elements.
