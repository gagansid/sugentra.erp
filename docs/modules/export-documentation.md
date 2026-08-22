# Module Log: Export Documentation & Compliance

> Not yet implemented. This file currently only records the cross-module dependency touchpoints with Inventory, captured while designing the Inventory module, so context isn't lost by the time Export Documentation is actually built. Full module log starts once implementation begins.

## Dependency on Inventory (recorded during Inventory design phase)
See [inventory.md](inventory.md#end-to-end-flow-cross-module-dependencies-annotated) for the full sequence diagram.

- Export Documentation needs quantity, dimensions, gross/net weight, and cubic measurement for the shipped batches to auto-populate the Packing List and Commercial Invoice — this data is read from Inventory via a query-style contract (e.g. `IShipmentDataProvider.GetShipmentDetailsAsync(shipmentReference)`), not duplicated into Export Documentation's own tables.
- Legality documents (SVLK/FSC certificate references) captured on `Inventory_Batches` at receiving time must also be exposed through that same contract so Export Documentation can attach them to Certificate of Origin / compliance filings without re-uploading.
- Export Documentation's own approval gate (Export Manager validating draft Packing List/Commercial Invoice) is internal and has no Inventory dependency.
